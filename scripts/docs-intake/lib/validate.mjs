// Stage 4. CI runs markdownlint-cli2 plus lychee; lychee has no npm package and
// docker is not available locally, so the link half is a deliberate stand-in:
// on-disk resolution plus HEAD requests via global fetch. It also covers what CI
// does NOT - use{file=} embed paths, and pages missing from Summary.md.

import { readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { walk } from './manifest.mjs';
import { extractRefs, isExternal } from './refs.mjs';
import { parseSummary, orphans } from './summary.mjs';

/** Archbee structural files: never orphans, never kebab-cased. */
export const STRUCTURAL = ['Introduction.md', 'Summary.md', 'config.md'];

const RAW_URL_RE = /https:\/\/raw\.githubusercontent\.com\/[^\s)"'>]+/g;

async function docsPages(repoRoot) {
  const pages = await walk(path.join(repoRoot, 'docs'));
  return pages.filter((p) => p.endsWith('.md'));
}

export function parseShadowDocs(yamlText) {
  const out = [];
  let inBlock = false;
  for (const line of yamlText.split(/\r?\n/)) {
    if (/^shadowdocs:/.test(line)) {
      inBlock = true;
      continue;
    }
    if (!inBlock) continue;
    const item = /^\s+-\s+(.+?)\s*$/.exec(line);
    if (item) out.push(item[1]);
    else if (/^\S/.test(line)) inBlock = false;
  }
  return out;
}

export function parseLycheeIgnore(text) {
  return text
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter((l) => l && !l.startsWith('#'))
    .map((l) => new RegExp(l));
}

export function runLint(repoRoot, { fix = false } = {}) {
  const args = ['markdownlint-cli2', ...(fix ? ['--fix'] : []), 'docs/**/*.md', '*.md'];
  const res = spawnSync('npx', args, {
    cwd: repoRoot,
    encoding: 'utf8',
    shell: true,
  });
  return { ok: res.status === 0, output: `${res.stdout ?? ''}${res.stderr ?? ''}`.trim() };
}

export async function checkLocalRefs(repoRoot) {
  const docsDir = path.join(repoRoot, 'docs');
  const broken = [];

  for (const rel of await docsPages(repoRoot)) {
    const body = await readFile(path.join(docsDir, rel), 'utf8');
    const { links, embeds } = extractRefs(body);

    for (const link of links) {
      if (isExternal(link.target)) continue;
      const target = link.target.split('#')[0];
      if (!target) continue;
      const resolved = path.resolve(path.dirname(path.join(docsDir, rel)), target);
      if (!existsSync(resolved)) broken.push({ page: rel, kind: 'link', target: link.target });
    }

    // use{file=...} paths are relative to docs/, and CI does not check them.
    for (const embed of embeds) {
      const target = embed.file.split('#')[0];
      if (!existsSync(path.join(docsDir, target))) {
        broken.push({ page: rel, kind: 'embed', target: embed.file });
      }
    }
  }

  return broken;
}

export async function collectRawUrls(repoRoot) {
  const docsDir = path.join(repoRoot, 'docs');
  const urls = new Set();
  for (const rel of await docsPages(repoRoot)) {
    const body = await readFile(path.join(docsDir, rel), 'utf8');
    for (const m of body.matchAll(RAW_URL_RE)) urls.add(m[0]);
  }

  const ignorePath = path.join(repoRoot, '.lycheeignore');
  const ignore = existsSync(ignorePath)
    ? parseLycheeIgnore(await readFile(ignorePath, 'utf8'))
    : [];

  return [...urls].filter((u) => !ignore.some((re) => re.test(u))).sort();
}

export async function headCheck(urls, { concurrency = 8, fetchImpl = fetch } = {}) {
  const queue = [...urls];
  const failures = [];

  await Promise.all(
    Array.from({ length: Math.min(concurrency, queue.length) }, async () => {
      while (queue.length) {
        const url = queue.shift();
        try {
          const res = await fetchImpl(url, { method: 'HEAD', redirect: 'follow' });
          if (!res.ok) failures.push({ url, status: res.status });
        } catch (err) {
          failures.push({ url, status: String(err?.message ?? err) });
        }
      }
    }),
  );

  return failures.sort((a, b) => a.url.localeCompare(b.url));
}

export async function checkOrphans(repoRoot) {
  const pages = (await docsPages(repoRoot)).filter((p) => !p.split('/').includes('samples'));
  const parsed = parseSummary(await readFile(path.join(repoRoot, 'docs', 'Summary.md'), 'utf8'));

  const archbeePath = path.join(repoRoot, '.archbee.yaml');
  const shadowDocs = existsSync(archbeePath)
    ? parseShadowDocs(await readFile(archbeePath, 'utf8'))
    : [];

  return orphans(parsed, pages, { structural: STRUCTURAL, shadowDocs });
}

export function proposeLycheeIgnore(text, urls, { note }) {
  if (!urls.length) return { text, added: [] };
  const block = [
    '',
    `# --- TEMPORARY: assets added by ${note}. A raw-main URL 404s until it merges.`,
    '# Delete every line in this block after merge - a 404 here should mean a real',
    '# missing asset, not a visibility artifact.',
    ...urls.map((u) => u.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')),
  ].join('\n');

  return { text: `${text.replace(/\s*$/, '\n')}${block}\n`, added: [...urls] };
}
