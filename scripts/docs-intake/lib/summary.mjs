// docs/Summary.md is the GitBook-style nav tree: a single H1, "## Group" headings
// as categories, and "- [Title](path)" list items as pages. Order and grouping come
// from this file alone (CONTRIBUTING.md rule 3), and a page absent from it never
// appears on the portal - which is why orphans() exists.

const HEADING_RE = /^##\s+(.+?)\s*$/;
const ITEM_RE = /^(\s*)-\s+\[([^\]]*)\]\(([^)]+)\)\s*$/;

const dirOf = (p) => {
  const i = p.lastIndexOf('/');
  return i === -1 ? '' : p.slice(0, i);
};

export function parseSummary(text) {
  const lines = text.split(/\r?\n/);
  const groups = [];
  const preamble = [];
  let current = null;

  lines.forEach((line, i) => {
    const heading = HEADING_RE.exec(line);
    if (heading) {
      current = {
        title: heading[1],
        headingLine: i,
        lastEntryLine: i,
        entries: [],
      };
      groups.push(current);
      return;
    }
    const item = ITEM_RE.exec(line);
    if (!item) return;
    const entry = { indent: item[1].length, title: item[2], path: item[3], line: i };
    if (current) {
      current.entries.push(entry);
      current.lastEntryLine = i;
    } else {
      preamble.push(entry);
    }
  });

  return { lines, preamble, groups };
}

export function groupForDirectory(parsed, destRelPath) {
  const targetDir = dirOf(destRelPath);
  let bestLen = -1;
  let matches = [];

  for (const group of parsed.groups) {
    for (const entry of group.entries) {
      const d = dirOf(entry.path);
      const isPrefix = d !== '' && (d === targetDir || targetDir.startsWith(`${d}/`));
      if (!isPrefix) continue;
      if (d.length > bestLen) {
        bestLen = d.length;
        matches = [];
      }
      if (d.length === bestLen) matches.push({ group: group.title, dir: d });
    }
  }

  const candidates = [...new Set(matches.map((m) => m.group))];
  if (candidates.length === 1) return { group: candidates[0], ambiguous: false };
  return {
    group: null,
    ambiguous: true,
    candidates,
    dir: matches.length ? matches[0].dir : null,
  };
}

export function appendEntry(parsed, { group, title, path }) {
  const target = parsed.groups.find((g) => g.title === group);
  if (!target) throw new Error(`No such group in Summary.md: "${group}"`);
  const out = [...parsed.lines];
  out.splice(target.lastEntryLine + 1, 0, `- [${title}](${path})`);
  return out.join('\n');
}

export function orphans(parsed, docsPages, { structural = [], shadowDocs = [] } = {}) {
  const listed = new Set();
  for (const entry of parsed.preamble) listed.add(entry.path);
  for (const group of parsed.groups) {
    for (const entry of group.entries) listed.add(entry.path);
  }
  const exempt = new Set([...structural, ...shadowDocs]);
  return docsPages.filter((p) => !listed.has(p) && !exempt.has(p));
}
