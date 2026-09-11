// Stage 1: enumerate the inbox and classify each file. No writes happen here -
// plan() is safe to run at any time and is what --dry-run reports.

import { readdir, readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { toDestination, assetDest } from './paths.mjs';
import { extractRefs } from './refs.mjs';

// Skipped only at the inbox ROOT. A README.md inside a section is real content:
// the repo convention is one index.md landing page per section, and discarding a
// dropped landing page loses a real page. This bit on the first 77-file drop -
// docs/api/index.md is an orphan that the dropped _inbox/api/README.md would fix.
export const SKIP_AT_ROOT = new Set(['README.md', 'Thumbs.db', '.DS_Store']);

const MARKDOWN = /\.md$/i;

export async function walk(dir, base = dir) {
  const out = [];
  let entries;
  try {
    entries = await readdir(dir, { withFileTypes: true });
  } catch {
    return out;
  }
  for (const entry of entries) {
    if (entry.name.startsWith('.')) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) out.push(...(await walk(full, base)));
    else out.push(path.relative(base, full).split(path.sep).join('/'));
  }
  return out;
}

export function titleFromMarkdown(md, fallback) {
  const m = /^#\s+(.+?)\s*$/m.exec(md);
  return m ? m[1].replace(/[`*_]/g, '').trim() : fallback;
}

export function sectionOfDest(destRelPath) {
  const [first, ...rest] = destRelPath.split('/');
  return rest.length ? first : null;
}

export async function collisionsFor(repoRoot, destRelPath) {
  const base = path.basename(destRelPath).toLowerCase();
  const pages = (await walk(path.join(repoRoot, 'docs'))).filter((p) => MARKDOWN.test(p));
  return pages
    .filter((p) => path.basename(p).toLowerCase() === base && p !== destRelPath)
    .sort();
}

export async function plan({ repoRoot }) {
  const inboxDir = path.join(repoRoot, '_inbox');
  const sources = await walk(inboxDir);
  const rows = [];
  const stops = [];

  for (const source of sources.sort()) {
    if (!source.includes('/') && SKIP_AT_ROOT.has(path.basename(source))) continue;

    const dest = toDestination(source);
    if (dest.stop) {
      stops.push({ source, reason: dest.stop });
      continue;
    }

    const isMarkdown = MARKDOWN.test(source);
    const destRelPath = isMarkdown
      ? dest.destRelPath
      : assetDest(dest.section, path.basename(source));

    const body = isMarkdown ? await readFile(path.join(inboxDir, source), 'utf8') : '';
    const refs = isMarkdown ? extractRefs(body) : { images: [], links: [], embeds: [] };
    const classification = existsSync(path.join(repoRoot, 'docs', destRelPath))
      ? 'UPDATE'
      : 'NEW';

    // Only a NEW page can be an accidental duplicate; an UPDATE already IS the page.
    const collisions =
      isMarkdown && classification === 'NEW'
        ? await collisionsFor(repoRoot, destRelPath)
        : [];

    if (collisions.length) {
      stops.push({
        source,
        reason: `"${path.basename(destRelPath)}" already exists elsewhere in docs/ (${collisions.join(', ')}) - confirm whether this updates one of those pages before creating a duplicate.`,
      });
    }

    rows.push({
      source,
      dest: destRelPath,
      kind: isMarkdown ? 'page' : 'asset',
      classification,
      collisions,
      section: dest.section,
      structural: dest.structural,
      title: isMarkdown ? titleFromMarkdown(body, path.basename(destRelPath, '.md')) : null,
      images: refs.images.map((i) => i.target),
      links: refs.links.map((l) => l.target),
      embeds: refs.embeds.map((e) => e.file),
    });
  }

  return { rows, stops };
}
