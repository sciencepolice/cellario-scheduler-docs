// Path normalization for docs-intake.
//
// Inbox paths mirror the docs/ tree; every segment is kebab-cased on the way in.
// Two deliberate exceptions, both load-bearing:
//
//   1. Structural files at the docs root keep their capitalization (CONTRIBUTING.md
//      rule 2). Archbee's sync filesystem is case-sensitive and the .archbee.yaml
//      values must match the on-disk names exactly.
//   2. Filenames under a samples/ directory are NEVER renamed. C#/Python sample
//      filenames are referenced verbatim by use{file=...} embeds, by relative links
//      in prose, and by .lycheeignore entries. Directory segments are still kebabed.

export const RAW_BASE =
  'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';

/** Files that legitimately live at the docs root with capitalization intact. */
export const STRUCTURAL_ROOT_FILES = new Set(['Introduction.md', 'config.md']);

/** Files the skill owns; a dropped copy is an error, not content. */
export const MANAGED_ROOT_FILES = new Set(['Summary.md']);

export function kebabCase(segment) {
  const dot = segment.lastIndexOf('.');
  const hasExt = dot > 0;
  const stem = hasExt ? segment.slice(0, dot) : segment;
  const ext = hasExt ? segment.slice(dot).toLowerCase() : '';

  const kebab = stem
    .replace(/([a-z0-9])([A-Z])/g, '$1-$2') // GettingStarted -> Getting-Started
    .toLowerCase()
    .replace(/[\s_]+/g, '-')
    .replace(/[^a-z0-9.-]/g, '-')
    .replace(/-{2,}/g, '-')
    .replace(/^-+|-+$/g, '');

  return kebab + ext;
}

export function toDestination(inboxRelPath) {
  const segments = inboxRelPath.split('/').filter(Boolean);
  const filename = segments.pop();

  if (segments.length === 0) {
    if (MANAGED_ROOT_FILES.has(filename)) {
      return {
        stop: `"${filename}" is managed by the skill (stage 3 wires the nav) - remove it from the inbox.`,
      };
    }
    if (STRUCTURAL_ROOT_FILES.has(filename)) {
      return { destRelPath: filename, section: null, structural: true };
    }
    return {
      stop: `"${filename}" sits at _inbox/ root with no section folder - the drop path is the only section signal.`,
    };
  }

  // Checked before kebab-casing; "samples" is already lowercase in every real path.
  const inSamples = segments.includes('samples');
  const dirs = segments.map(kebabCase);

  return {
    destRelPath: [...dirs, inSamples ? filename : kebabCase(filename)].join('/'),
    section: dirs[0],
    structural: false,
  };
}

export function assetDest(section, filename) {
  return `assets/images/${section}/${kebabCase(filename)}`;
}

export function rawUrlFor(destRelPath) {
  return `${RAW_BASE}docs/${destRelPath}`;
}
