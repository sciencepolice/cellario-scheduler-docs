// Stage 1: enumerate the inbox and classify each file. No writes happen here -
// plan() is safe to run at any time and is what --dry-run reports.

import { readdir, readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { toDestination } from './paths.mjs';
import { extractRefs } from './refs.mjs';

const SKIP_FILES = new Set(['README.md', 'Thumbs.db', '.DS_Store']);

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
    if (entry.isDirectory()) {
      out.push(...(await walk(full, base)));
    } else if (!SKIP_FILES.has(entry.name)) {
      out.push(path.relative(base, full).split(path.sep).join('/'));
    }
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

export async function plan({ repoRoot }) {
  const inboxDir = path.join(repoRoot, '_inbox');
  const sources = await walk(inboxDir);
  const rows = [];
  const stops = [];

  for (const source of sources.sort()) {
    const dest = toDestination(source);
    if (dest.stop) {
      stops.push({ source, reason: dest.stop });
      continue;
    }

    const isMarkdown = source.toLowerCase().endsWith('.md');
    const body = isMarkdown ? await readFile(path.join(inboxDir, source), 'utf8') : '';
    const refs = isMarkdown
      ? extractRefs(body)
      : { images: [], links: [], embeds: [] };

    rows.push({
      source,
      dest: dest.destRelPath,
      classification: existsSync(path.join(repoRoot, 'docs', dest.destRelPath))
        ? 'UPDATE'
        : 'NEW',
      section: dest.section,
      structural: dest.structural,
      title: isMarkdown
        ? titleFromMarkdown(body, path.basename(dest.destRelPath, '.md'))
        : null,
      images: refs.images.map((i) => i.target),
      links: refs.links.map((l) => l.target),
      embeds: refs.embeds.map((e) => e.file),
    });
  }

  return { rows, stops };
}
