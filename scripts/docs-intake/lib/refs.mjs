// Extraction and rewriting of the three reference kinds that matter to Archbee:
// image refs (must become raw-main GitHub URLs), relative .md links (must be
// kebab-cased to match on-disk names), and use{file=...} embeds (must survive an
// update untouched).

import { RAW_BASE, kebabCase, assetDest } from './paths.mjs';

// The target group is lazy and space-tolerant ([^)]*? not [^)\s]+): source-repo
// markdown routinely carries unencoded spaces ("./images/Run Dialog.png"), and those
// are precisely the refs that must be rewritten. Laziness makes the optional title
// group win, so ("./a.png \"A title\"") splits correctly instead of swallowing the title.
const IMAGE_RE = /!\[([^\]]*)\]\(([^)]*?)(\s+"[^"]*")?\)/g;
const LINK_RE = /(?<!!)\[([^\]]*)\]\(([^)]*?)(\s+"[^"]*")?\)/g;
const EMBED_RE = /use\{file="([^"]+)"(?:\s+syntax="([^"]+)")?\}/g;

export function isExternal(target) {
  return /^(?:https?:|mailto:|#|\/\/)/.test(target);
}

export function extractRefs(markdown) {
  return {
    images: [...markdown.matchAll(IMAGE_RE)].map((m) => ({
      raw: m[0],
      alt: m[1],
      target: m[2],
    })),
    links: [...markdown.matchAll(LINK_RE)].map((m) => ({
      raw: m[0],
      text: m[1],
      target: m[2],
    })),
    embeds: [...markdown.matchAll(EMBED_RE)].map((m) => ({
      raw: m[0],
      file: m[1],
      syntax: m[2] ?? null,
    })),
  };
}

export function rewriteImageRefs(markdown, { section, existingAssets = null }) {
  const rewritten = [];
  const assets = [];
  const collisions = [];

  const text = markdown.replace(IMAGE_RE, (raw, alt, target, title) => {
    if (isExternal(target)) return raw;
    const sourceBasename = target.split('/').pop().split('?')[0];
    const destRelPath = assetDest(section, sourceBasename);
    const url = `${RAW_BASE}docs/${destRelPath}`;
    rewritten.push({ from: target, to: url });

    // Assets are keyed by basename, so a generic name (overview.png, settings.png)
    // can compute the destination of a DIFFERENT page's published image. Copying
    // over it would silently replace live artwork and check() cannot see it, because
    // the URL still resolves. Report instead of queueing the copy.
    if (existingAssets?.has(destRelPath)) collisions.push({ sourceBasename, destRelPath });
    else assets.push({ sourceBasename, destRelPath });

    return `![${alt}](${url}${title ?? ''})`;
  });

  return { text, rewritten, assets, collisions };
}

export function kebabLinkTargets(markdown) {
  const rewritten = [];

  const text = markdown.replace(LINK_RE, (raw, label, target, title) => {
    if (isExternal(target)) return raw;
    const hash = target.indexOf('#');
    const pathPart = hash === -1 ? target : target.slice(0, hash);
    const anchor = hash === -1 ? '' : target.slice(hash);
    // Only .md is normalized: .cs/.py source filenames are referenced verbatim.
    if (!/\.md$/i.test(pathPart)) return raw;
    if (pathPart.split('/').includes('samples')) return raw;

    const next =
      pathPart
        .split('/')
        .map((s) => (s === '.' || s === '..' || s === '' ? s : kebabCase(s)))
        .join('/') + anchor;

    if (next === target) return raw;
    rewritten.push({ from: target, to: next });
    return `[${label}](${next}${title ?? ''})`;
  });

  return { text, rewritten };
}

export function plumbingReport(incoming, existing) {
  const ex = extractRefs(existing);

  // basename -> the raw URL the published page already uses.
  const byBasename = new Map();
  for (const img of ex.images) {
    if (!isExternal(img.target)) continue;
    byBasename.set(img.target.split('/').pop().toLowerCase(), img.target);
  }

  const preservedImages = [];
  const text = incoming.replace(IMAGE_RE, (raw, alt, target, title) => {
    if (isExternal(target)) return raw;
    const key = kebabCase(target.split('/').pop().split('?')[0]).toLowerCase();
    const url = byBasename.get(key);
    if (!url) return raw;
    preservedImages.push({ from: target, to: url });
    return `![${alt}](${url}${title ?? ''})`;
  });

  // Reported, never re-inserted: re-placing an embed in reorganized prose is a
  // judgment call the model owns (spec section 3.2).
  const incomingEmbeds = new Set(extractRefs(incoming).embeds.map((e) => e.raw));
  const droppedEmbeds = ex.embeds.filter((e) => !incomingEmbeds.has(e.raw));

  return { text, preservedImages, droppedEmbeds };
}
