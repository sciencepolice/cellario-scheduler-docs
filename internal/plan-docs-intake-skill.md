# `docs-intake` Skill Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a repo-local Claude Code skill that turns source-repo markdown dropped into `_inbox/` into Archbee-ready pages — normalized paths, raw-GitHub image URLs, preserved `use{file=...}` embeds, `Summary.md` registration, local validation — then opens a PR.

**Architecture:** A zero-dependency Node CLI (`scripts/docs-intake/intake.mjs`) owns everything deterministic and exposes three subcommands (`plan`, `rewrite-refs`, `check`) over four focused pure-ish library modules. `SKILL.md` owns the procedure and the judgment calls the script deliberately refuses to make: matching an inbox file to its `docs/` counterpart, and merging incoming prose while preserving local Archbee plumbing. Every ambiguity is a non-zero exit, not a guess.

**Tech Stack:** Node 24 (built-in `node:test`, global `fetch`), no runtime dependencies. `npx markdownlint-cli2` is invoked as a subprocess — it is already a CI dependency, not a new one.

**Spec:** `internal/design-docs-intake-skill.md` (approved 2026-09-11).

## Global Constraints

- **Node >= 24.** Uses `node:test`, global `fetch`, and `String.matchAll`. `node --version` must report v24+.
- **Zero runtime dependencies.** No `package.json`, no `node_modules`. The only external process is `npx markdownlint-cli2`.
- **Raw asset base URL, verbatim:** `https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/`
- **Never rename a file under a `samples/` directory.** C#/Python sample filenames are referenced verbatim by `use{file=...}` embeds, by relative links in prose, and by `.lycheeignore` entries (`Run_Data_Next.cs`, `Log_Message.cs`, `Clean_Washer_With_Plate.cs`). Directory segments are still kebab-cased; the filename is not.
- **Never kebab-case a `.cs` or `.py` link target.** Only `.md` targets are normalized.
- **Structural root files keep their capitalization:** `Introduction.md`, `Summary.md`, `config.md`. Archbee's sync filesystem is case-sensitive (`CONTRIBUTING.md` rule 2).
- **`Summary.md` and `.lycheeignore` get exactly one writer per run**, at stage 3 and stage 4. No per-file step may write either.
- **Specs and plans live in `internal/`, never `docs/`.** `docs/` is Archbee's sync root and CI globs `docs/**/*.md`.
- **Nothing under `scripts/`, `.claude/`, or `internal/` is linted or link-checked** by CI (`validate.yml` globs `docs/**/*.md` and `*.md`, where `*.md` is root-level only).
- **Commit identity is repo-local:** `sciencepolice <47534695+sciencepolice@users.noreply.github.com>`. Pushing requires `gh auth switch --hostname github.com --user sciencepolice` first; the `dstugan_hrbs` EMU account can never have access.
- **Test command, used by every task:** `node --test ".claude/skills/docs-intake/test/*.test.mjs"`
  The **glob form is required**, quoted so Node expands it rather than the shell. The
  directory form (`node --test <dir>`) fails on this machine (Node v24.19.0 on Windows) —
  the runner tries to load the directory itself as a module and dies with
  `MODULE_NOT_FOUND`. Verified working in both Git Bash and PowerShell.

---

## File Structure

| File | Responsibility |
| --- | --- |
| `.gitignore` | **New.** Ignore `_inbox/*`, un-ignore `_inbox/README.md`. |
| `_inbox/README.md` | The drop contract: mirror the `docs/` tree. |
| `scripts/docs-intake/lib/paths.mjs` | Kebab-casing and inbox→docs destination computation. Pure. |
| `scripts/docs-intake/lib/refs.mjs` | Extract and rewrite image refs, link targets, `use{file=}` embeds. Pure. |
| `scripts/docs-intake/lib/summary.mjs` | Parse `Summary.md`; map a directory to its nav group; append an entry; report orphans. Pure. |
| `scripts/docs-intake/lib/manifest.mjs` | Walk `_inbox/`, classify NEW/UPDATE, collect refs. Filesystem. |
| `scripts/docs-intake/lib/validate.mjs` | Lint, on-disk ref resolution, raw-URL HEAD checks, orphan report, `.lycheeignore` proposals. Filesystem + network + subprocess. |
| `scripts/docs-intake/intake.mjs` | CLI: arg parsing, subcommand dispatch, output formatting, exit codes. |
| `.claude/skills/docs-intake/SKILL.md` | The six-stage procedure and the ambiguity stops. Procedure only — cites `CONTRIBUTING.md` for the rules. |
| `.claude/skills/docs-intake/test/*.test.mjs` | `node:test` suites, one per lib module plus a CLI suite. |
| `.claude/skills/docs-intake/test/fixtures/repo/` | A miniature repo (docs tree + inbox) so tests are hermetic. |
| `CONTRIBUTING.md` | Add an "Automated intake" pointer under Workflow. |

**Why a fixture repo.** `manifest.mjs` and `validate.mjs` take a `repoRoot` argument rather than assuming the real one, so tests run against `test/fixtures/repo/` and stay green as real content changes. The spec names `scripting/base-tutorials/hello-world.md` as the `UPDATE` exemplar; the fixture reproduces that page's *shape* (a raw-GitHub image plus a `use{file=}` embed), and Task 8 runs one end-to-end dry run against the real page.

---

### Task 1: Inbox scaffolding

**Files:**
- Create: `.gitignore`
- Create: `_inbox/README.md`

**Interfaces:**
- Consumes: nothing.
- Produces: the `_inbox/` drop location every later task assumes, and the guarantee that raw drops cannot be committed.

- [ ] **Step 1: Create `.gitignore`**

The repo has no `.gitignore` today. The negation line is load-bearing — `_inbox/*` alone would also ignore the README.

```gitignore
# Raw content drops for the docs-intake skill (.claude/skills/docs-intake).
# Files here are staged, never tracked: the skill normalizes them into docs/ and
# empties the folder as part of its run. The README is the one tracked exception.
_inbox/*
!_inbox/README.md

# OS / editor cruft
Thumbs.db
.DS_Store
```

- [ ] **Step 2: Create `_inbox/README.md`**

```markdown
# `_inbox/` — staging for the docs-intake skill

Drop source-repo markdown (and its images) here, **mirroring the `docs/` tree**. The drop
path is the only signal the skill uses to decide which section a file belongs to — it does
not infer the section from content.

```text
_inbox/user-guide/getting-started.md   ->  docs/user-guide/getting-started.md
_inbox/api/api/events.md               ->  docs/api/api/events.md
_inbox/scripting/samples/demo.py       ->  docs/scripting/samples/demo.py
```

Then ask Claude Code to run the intake. It will normalize paths, rewrite image references,
merge updates into existing pages, register new pages in `docs/Summary.md`, validate
locally, and open a PR.

Notes:

- Everything here except this README is git-ignored, so a raw drop can never be committed.
- A file dropped at `_inbox/` root with no section folder is an error, not a guess. The
  exceptions are `Introduction.md` and `config.md`, which genuinely live at the docs root.
- `Summary.md` is managed by the skill. Don't drop one.
- Filenames under a `samples/` directory are kept verbatim — `use{file=...}` embeds and
  prose links reference them by their exact source names.
- The skill empties this folder when it commits, so a finished run leaves it clean.
```

- [ ] **Step 3: Verify the ignore rules behave**

Run:

```bash
mkdir -p _inbox/user-guide && printf '# Scratch\n' > _inbox/user-guide/scratch.md
git check-ignore -v _inbox/user-guide/scratch.md
git check-ignore -v _inbox/README.md; echo "exit=$?"
```

Expected: the first `check-ignore` prints a `.gitignore:4:_inbox/*` match; the second prints nothing and reports `exit=1` (not ignored). Then clean up: `rm _inbox/user-guide/scratch.md`.

- [ ] **Step 4: Commit**

```bash
git add .gitignore _inbox/README.md
git commit -m "Add _inbox staging folder and repo .gitignore"
```

---

### Task 2: `lib/paths.mjs` — path normalization

**Files:**
- Create: `scripts/docs-intake/lib/paths.mjs`
- Test: `.claude/skills/docs-intake/test/paths.test.mjs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `RAW_BASE: string`
  - `STRUCTURAL_ROOT_FILES: Set<string>`, `MANAGED_ROOT_FILES: Set<string>`
  - `kebabCase(segment: string) -> string`
  - `toDestination(inboxRelPath: string) -> { destRelPath, section, structural } | { stop: string }`
  - `assetDest(section: string, filename: string) -> string`
  - `rawUrlFor(destRelPath: string) -> string`

- [ ] **Step 1: Write the failing test**

Create `.claude/skills/docs-intake/test/paths.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  kebabCase,
  toDestination,
  assetDest,
  rawUrlFor,
} from '../../../../scripts/docs-intake/lib/paths.mjs';

test('kebabCase normalizes the shapes content actually arrives in', () => {
  assert.equal(kebabCase('Title Case With Spaces'), 'title-case-with-spaces');
  assert.equal(kebabCase('Run_Data_Next.cs'), 'run-data-next.cs');
  assert.equal(kebabCase('GettingStarted.md'), 'getting-started.md');
  assert.equal(kebabCase('Already-Kebab.MD'), 'already-kebab.md');
  assert.equal(kebabCase('01-overview-and-quick-start.md'), '01-overview-and-quick-start.md');
  assert.equal(kebabCase('Weird  --  Name!.md'), 'weird-name.md');
});

test('toDestination kebab-cases every segment and reports the section', () => {
  assert.deepEqual(toDestination('user-guide/Getting Started.md'), {
    destRelPath: 'user-guide/getting-started.md',
    section: 'user-guide',
    structural: false,
  });
  assert.deepEqual(toDestination('API Reference/Sub Folder/A Page.md'), {
    destRelPath: 'api-reference/sub-folder/a-page.md',
    section: 'api-reference',
    structural: false,
  });
});

test('toDestination never renames a file under samples/', () => {
  const got = toDestination('scripting/samples/CSharp Scripts/demo/Run_Data_Next.cs');
  assert.equal(got.destRelPath, 'scripting/samples/csharp-scripts/demo/Run_Data_Next.cs');
  assert.equal(got.section, 'scripting');
});

test('toDestination stops on a root drop with no section folder', () => {
  const got = toDestination('notes.md');
  assert.match(got.stop, /no section folder/);
  assert.equal(got.destRelPath, undefined);
});

test('toDestination allows the structural root exceptions but not Summary.md', () => {
  assert.deepEqual(toDestination('Introduction.md'), {
    destRelPath: 'Introduction.md',
    section: null,
    structural: true,
  });
  assert.deepEqual(toDestination('config.md'), {
    destRelPath: 'config.md',
    section: null,
    structural: true,
  });
  assert.match(toDestination('Summary.md').stop, /managed by the skill/);
});

test('asset helpers build the raw-main URL form', () => {
  assert.equal(assetDest('user-guide', 'Run Dialog.PNG'), 'assets/images/user-guide/run-dialog.png');
  assert.equal(
    rawUrlFor('assets/images/user-guide/run-dialog.png'),
    'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/user-guide/run-dialog.png',
  );
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/paths.test.mjs`

Expected: FAIL — `Cannot find module .../scripts/docs-intake/lib/paths.mjs`.

- [ ] **Step 3: Write the implementation**

Create `scripts/docs-intake/lib/paths.mjs`:

```js
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
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/paths.test.mjs`

Expected: PASS — 6 tests, 0 failures.

- [ ] **Step 5: Commit**

```bash
git add scripts/docs-intake/lib/paths.mjs .claude/skills/docs-intake/test/paths.test.mjs
git commit -m "Add docs-intake path normalization"
```

---

### Task 3: `lib/refs.mjs` — reference extraction and rewriting

**Files:**
- Create: `scripts/docs-intake/lib/refs.mjs`
- Test: `.claude/skills/docs-intake/test/refs.test.mjs`

**Interfaces:**
- Consumes: `RAW_BASE`, `kebabCase`, `assetDest` from `lib/paths.mjs`.
- Produces:
  - `isExternal(target: string) -> boolean`
  - `extractRefs(markdown: string) -> { images: {raw,alt,target}[], links: {raw,text,target}[], embeds: {raw,file,syntax}[] }`
  - `rewriteImageRefs(markdown: string, { section: string }) -> { text, rewritten: {from,to}[], assets: {sourceBasename,destRelPath}[] }`
  - `kebabLinkTargets(markdown: string) -> { text, rewritten: {from,to}[] }`
  - `plumbingReport(incoming: string, existing: string) -> { text, preservedImages: {from,to}[], droppedEmbeds: {raw,file,syntax}[] }`

`plumbingReport` is the deterministic half of the stage-2 merge. It re-points incoming local
image refs at the raw URLs the existing page already uses, and it **reports** embeds the
existing page has that the incoming file lacks — it does not re-insert them. Placing an embed
back at the right point in reorganized prose is the model's call, per spec §3.2.

- [ ] **Step 1: Write the failing test**

Create `.claude/skills/docs-intake/test/refs.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  isExternal,
  extractRefs,
  rewriteImageRefs,
  kebabLinkTargets,
  plumbingReport,
} from '../../../../scripts/docs-intake/lib/refs.mjs';

const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';

test('isExternal recognizes what must not be rewritten', () => {
  assert.equal(isExternal('https://example.com/a.png'), true);
  assert.equal(isExternal('mailto:a@b.com'), true);
  assert.equal(isExternal('#anchor'), true);
  assert.equal(isExternal('./images/a.png'), false);
});

test('extractRefs separates images, links and embeds', () => {
  const md = [
    '![Run dialog](./images/Run Dialog.png)',
    '[Authentication](Authentication.md)',
    '[NuGet](https://www.nuget.org/packages/Cellario.Client/)',
    'use{file="scripting/samples/demo.py#l1-l20" syntax="python"}',
  ].join('\n\n');
  const { images, links, embeds } = extractRefs(md);
  assert.equal(images.length, 1);
  assert.equal(images[0].target, './images/Run Dialog.png');
  assert.deepEqual(links.map((l) => l.target), ['Authentication.md', 'https://www.nuget.org/packages/Cellario.Client/']);
  assert.equal(embeds.length, 1);
  assert.equal(embeds[0].file, 'scripting/samples/demo.py#l1-l20');
  assert.equal(embeds[0].syntax, 'python');
});

test('rewriteImageRefs points local images at raw-main and lists the assets to copy', () => {
  const md = '![Run dialog](./images/Run Dialog.png)\n\n![Remote](https://example.com/x.png)';
  const { text, rewritten, assets } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.ok(text.includes(`![Run dialog](${RAW}docs/assets/images/user-guide/run-dialog.png)`));
  assert.ok(text.includes('![Remote](https://example.com/x.png)'), 'external images untouched');
  assert.equal(rewritten.length, 1);
  assert.deepEqual(assets, [
    { sourceBasename: 'Run Dialog.png', destRelPath: 'assets/images/user-guide/run-dialog.png' },
  ]);
});

test('rewriteImageRefs preserves a markdown title attribute', () => {
  const md = '![Alt](./a.png "A title")';
  const { text } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.ok(text.endsWith('"A title")'), text);
});

test('kebabLinkTargets normalizes .md links only', () => {
  const md = [
    '[Auth](Authentication.md)',
    '[Sample](Run_Data_Next.cs)',
    '[Anchored](Some Page.md#a-section)',
    '[External](https://example.com/A Page.md)',
  ].join('\n\n');
  const { text, rewritten } = kebabLinkTargets(md);
  assert.ok(text.includes('[Auth](authentication.md)'));
  assert.ok(text.includes('[Sample](Run_Data_Next.cs)'), '.cs targets are verbatim');
  assert.ok(text.includes('[Anchored](some-page.md#a-section)'));
  assert.ok(text.includes('[External](https://example.com/A Page.md)'));
  assert.equal(rewritten.length, 2);
});

test('plumbingReport carries raw image URLs over and reports dropped embeds', () => {
  const existing = [
    '# Hello World',
    `![Script editor](${RAW}docs/assets/images/scripting/script-editor.png)`,
    'use{file="scripting/samples/hello.py#l1-l12" syntax="python"}',
  ].join('\n\n');
  const incoming = [
    '# Hello World',
    'New opening paragraph.',
    '![Script editor](./images/Script_Editor.png)',
  ].join('\n\n');

  const { text, preservedImages, droppedEmbeds } = plumbingReport(incoming, existing);
  assert.ok(text.includes(`${RAW}docs/assets/images/scripting/script-editor.png`));
  assert.ok(text.includes('New opening paragraph.'), 'incoming prose survives');
  assert.equal(preservedImages.length, 1);
  assert.equal(droppedEmbeds.length, 1);
  assert.equal(droppedEmbeds[0].file, 'scripting/samples/hello.py#l1-l12');
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/refs.test.mjs`

Expected: FAIL — `Cannot find module .../lib/refs.mjs`.

- [ ] **Step 3: Write the implementation**

Create `scripts/docs-intake/lib/refs.mjs`:

```js
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

export function rewriteImageRefs(markdown, { section }) {
  const rewritten = [];
  const assets = [];

  const text = markdown.replace(IMAGE_RE, (raw, alt, target, title) => {
    if (isExternal(target)) return raw;
    const sourceBasename = target.split('/').pop().split('?')[0];
    const destRelPath = assetDest(section, sourceBasename);
    const url = `${RAW_BASE}docs/${destRelPath}`;
    rewritten.push({ from: target, to: url });
    assets.push({ sourceBasename, destRelPath });
    return `![${alt}](${url}${title ?? ''})`;
  });

  return { text, rewritten, assets };
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
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/refs.test.mjs`

Expected: PASS — 6 tests, 0 failures.

- [ ] **Step 5: Commit**

```bash
git add scripts/docs-intake/lib/refs.mjs .claude/skills/docs-intake/test/refs.test.mjs
git commit -m "Add docs-intake reference extraction and rewriting"
```

---

### Task 4: `lib/summary.mjs` — nav tree parsing and group resolution

**Files:**
- Create: `scripts/docs-intake/lib/summary.mjs`
- Test: `.claude/skills/docs-intake/test/summary.test.mjs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `parseSummary(text: string) -> { lines: string[], preamble: {title,path}[], groups: {title,headingLine,lastEntryLine,entries:{indent,title,path,line}[]}[] }`
  - `groupForDirectory(parsed, destRelPath: string) -> { group: string|null, ambiguous: boolean, candidates?: string[], dir?: string|null }`
  - `appendEntry(parsed, { group, title, path }) -> string` (returns new `Summary.md` text)
  - `orphans(parsed, docsPages: string[], { structural?: string[], shadowDocs?: string[] }) -> string[]`

`groupForDirectory` implements the spec's most-specific-directory-prefix rule. **Call
`parseSummary` again after each `appendEntry`** — line numbers shift.

- [ ] **Step 1: Write the failing test**

Create `.claude/skills/docs-intake/test/summary.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  parseSummary,
  groupForDirectory,
  appendEntry,
  orphans,
} from '../../../../scripts/docs-intake/lib/summary.mjs';

// Mirrors the real docs/Summary.md, trimmed: it keeps the api/api ambiguity
// (examples-overview.md lives in api/api but is listed under API Examples).
const SUMMARY = `# Table of contents

- [Introduction](Introduction.md)

## User Guide

- [Overview](user-guide/index.md)

## API Reference

- [Overview](api/api/overview.md)
- [Authentication](api/api/authentication.md)

## API Examples

- [Examples Overview](api/api/examples-overview.md)
- [Basic Operations](api/api/examples/basic-operations.md)

## Scripting

- [Overview](scripting/index.md)
- [Referencing Dependencies](scripting/advanced-topics/referencing-dependencies.md)

## Scripting API

- [API Overview](scripting/scripting-api/general/api-overview.md)
`;

test('parseSummary reads groups, entries and the pre-group preamble', () => {
  const parsed = parseSummary(SUMMARY);
  assert.deepEqual(parsed.groups.map((g) => g.title), [
    'User Guide', 'API Reference', 'API Examples', 'Scripting', 'Scripting API',
  ]);
  assert.deepEqual(parsed.preamble.map((e) => e.path), ['Introduction.md']);
  assert.equal(parsed.groups[0].entries.length, 1);
  assert.equal(parsed.groups[0].entries[0].path, 'user-guide/index.md');
});

test('groupForDirectory resolves by most-specific directory prefix', () => {
  const parsed = parseSummary(SUMMARY);
  assert.deepEqual(groupForDirectory(parsed, 'user-guide/getting-started.md'), {
    group: 'User Guide', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'api/api/examples/new-examples.md'), {
    group: 'API Examples', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'scripting/advanced-topics/new-topic.md'), {
    group: 'Scripting', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'scripting/scripting-api/general/new-api.md'), {
    group: 'Scripting API', ambiguous: false,
  });
});

test('groupForDirectory flags api/api as ambiguous, per the spec', () => {
  const parsed = parseSummary(SUMMARY);
  const got = groupForDirectory(parsed, 'api/api/new-page.md');
  assert.equal(got.ambiguous, true);
  assert.equal(got.group, null);
  assert.deepEqual(got.candidates.sort(), ['API Examples', 'API Reference']);
});

test('groupForDirectory flags an unknown directory as ambiguous', () => {
  const parsed = parseSummary(SUMMARY);
  const got = groupForDirectory(parsed, 'brand-new-section/page.md');
  assert.equal(got.ambiguous, true);
  assert.deepEqual(got.candidates, []);
});

test('appendEntry appends to the end of its group only', () => {
  const parsed = parseSummary(SUMMARY);
  const next = appendEntry(parsed, {
    group: 'User Guide',
    title: 'Getting Started',
    path: 'user-guide/getting-started.md',
  });
  const lines = next.split('\n');
  const at = lines.indexOf('- [Getting Started](user-guide/getting-started.md)');
  assert.ok(at > lines.indexOf('- [Overview](user-guide/index.md)'));
  assert.ok(at < lines.indexOf('## API Reference'));
  assert.equal(parseSummary(next).groups[0].entries.length, 2);
});

test('appendEntry rejects an unknown group', () => {
  const parsed = parseSummary(SUMMARY);
  assert.throws(
    () => appendEntry(parsed, { group: 'Nope', title: 'X', path: 'x.md' }),
    /No such group/,
  );
});

test('orphans reports unlisted pages and exempts structural and shadow docs', () => {
  const parsed = parseSummary(SUMMARY);
  const pages = [
    'Introduction.md',
    'Summary.md',
    'config.md',
    'user-guide/index.md',
    'api/index.md',
    'api/api/scripting-api-reference.md',
  ];
  const got = orphans(parsed, pages, {
    structural: ['Introduction.md', 'Summary.md', 'config.md'],
    shadowDocs: ['api/api/scripting-api-reference.md'],
  });
  assert.deepEqual(got, ['api/index.md']);
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/summary.test.mjs`

Expected: FAIL — `Cannot find module .../lib/summary.mjs`.

- [ ] **Step 3: Write the implementation**

Create `scripts/docs-intake/lib/summary.mjs`:

```js
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
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/summary.test.mjs`

Expected: PASS — 7 tests, 0 failures.

- [ ] **Step 5: Commit**

```bash
git add scripts/docs-intake/lib/summary.mjs .claude/skills/docs-intake/test/summary.test.mjs
git commit -m "Add docs-intake Summary.md parsing and group resolution"
```

---

### Task 5: `lib/manifest.mjs` and the `plan` subcommand

**Files:**
- Create: `scripts/docs-intake/lib/manifest.mjs`
- Create: `scripts/docs-intake/intake.mjs`
- Create: `.claude/skills/docs-intake/test/fixtures/repo/` (files listed in Step 1)
- Test: `.claude/skills/docs-intake/test/manifest.test.mjs`

**Interfaces:**
- Consumes: `toDestination` from `lib/paths.mjs`; `extractRefs` from `lib/refs.mjs`.
- Produces:
  - `walk(dir: string, base?: string) -> Promise<string[]>` (posix-style relative paths; used again by `validate.mjs`)
  - `titleFromMarkdown(md: string, fallback: string) -> string`
  - `plan({ repoRoot }) -> Promise<{ rows: Row[], stops: {source,reason}[] }>` where
    `Row = { source, dest, classification: 'NEW'|'UPDATE', section, structural, title, images, links, embeds }`
  - CLI: `node scripts/docs-intake/intake.mjs plan [--json] [--repo-root <dir>]`, exit `0` clean / `2` if any stop

- [ ] **Step 1: Create the fixture repo**

Seven files. `docs/scripting/base-tutorials/hello-world.md` reproduces the shape of the real
page named in spec §7 — a raw-main image plus a `use{file=}` embed — so the `UPDATE` path is
exercised without depending on live content.

`.claude/skills/docs-intake/test/fixtures/repo/docs/Summary.md`:

```markdown
# Table of contents

- [Introduction](Introduction.md)

## User Guide

- [Overview](user-guide/index.md)

## Scripting

- [Hello World](scripting/base-tutorials/hello-world.md)
```

`.claude/skills/docs-intake/test/fixtures/repo/docs/Introduction.md`:

```markdown
# Introduction

Fixture home page.
```

`.claude/skills/docs-intake/test/fixtures/repo/docs/user-guide/index.md`:

```markdown
# User Guide

Fixture section landing page.
```

`.claude/skills/docs-intake/test/fixtures/repo/docs/scripting/base-tutorials/hello-world.md`:

```markdown
# Hello World

The original opening paragraph.

![Script editor](https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/script-editor.png)

use{file="scripting/samples/hello.py#l1-l12" syntax="python"}
```

`.claude/skills/docs-intake/test/fixtures/repo/_inbox/scripting/base-tutorials/Hello World.md`:

```markdown
# Hello World

A rewritten opening paragraph from the source repo.

![Script editor](./images/Script_Editor.png)
```

`.claude/skills/docs-intake/test/fixtures/repo/_inbox/README.md` (so the walk-skips-README
assertion has something to skip):

```markdown
# Fixture inbox

Present only so `walk()` is exercised against a README it must ignore.
```

`.claude/skills/docs-intake/test/fixtures/repo/_inbox/user-guide/Getting Started.md`:

```markdown
# Getting Started

Brand new page, no counterpart in docs/.

[Overview](Index.md)
```

- [ ] **Step 2: Write the failing test**

Create `.claude/skills/docs-intake/test/manifest.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import {
  walk,
  titleFromMarkdown,
  plan,
} from '../../../../scripts/docs-intake/lib/manifest.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE = path.join(HERE, 'fixtures', 'repo');
const CLI = path.resolve(HERE, '../../../../scripts/docs-intake/intake.mjs');

test('walk returns posix-style relative paths and skips READMEs', async () => {
  const found = await walk(path.join(FIXTURE, '_inbox'));
  assert.deepEqual(found.sort(), [
    'scripting/base-tutorials/Hello World.md',
    'user-guide/Getting Started.md',
  ]);
});

test('titleFromMarkdown prefers the H1', () => {
  assert.equal(titleFromMarkdown('# Getting Started\n\nBody', 'fallback'), 'Getting Started');
  assert.equal(titleFromMarkdown('No heading here', 'fallback'), 'fallback');
});

test('plan classifies UPDATE vs NEW and computes destinations', async () => {
  const { rows, stops } = await plan({ repoRoot: FIXTURE });
  assert.deepEqual(stops, []);

  const byDest = Object.fromEntries(rows.map((r) => [r.dest, r]));
  const update = byDest['scripting/base-tutorials/hello-world.md'];
  assert.equal(update.classification, 'UPDATE');
  assert.equal(update.section, 'scripting');
  assert.equal(update.title, 'Hello World');
  assert.deepEqual(update.images, ['./images/Script_Editor.png']);

  const created = byDest['user-guide/getting-started.md'];
  assert.equal(created.classification, 'NEW');
  assert.equal(created.title, 'Getting Started');
  assert.deepEqual(created.links, ['Index.md']);
});

test('plan --json exits 0 on a clean batch and prints the rows', () => {
  const out = execFileSync(process.execPath, [CLI, 'plan', '--json', '--repo-root', FIXTURE], {
    encoding: 'utf8',
  });
  const parsed = JSON.parse(out);
  assert.equal(parsed.rows.length, 2);
  assert.deepEqual(parsed.stops, []);
});
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/manifest.test.mjs`

Expected: FAIL — `Cannot find module .../lib/manifest.mjs`.

- [ ] **Step 4: Write `lib/manifest.mjs`**

```js
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
```

- [ ] **Step 5: Write `intake.mjs` with the `plan` subcommand**

```js
#!/usr/bin/env node
// docs-intake CLI. Three subcommands, all driven by .claude/skills/docs-intake/SKILL.md:
//
//   plan          stage 1: enumerate and classify the inbox (never writes)
//   rewrite-refs  stage 2: normalize refs in one placed page
//   check         stage 4: lint, resolve refs, HEAD raw URLs, report orphans
//
// Exit codes: 0 clean, 1 validation failure, 2 ambiguity requiring a human, 64 usage.

import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { plan } from './lib/manifest.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const DEFAULT_REPO_ROOT = path.resolve(HERE, '..', '..');

export const EXIT = { OK: 0, VALIDATION: 1, AMBIGUOUS: 2, USAGE: 64 };

const USAGE = `Usage:
  intake.mjs plan [--json] [--repo-root <dir>]
  intake.mjs rewrite-refs <docs-relative-path> [--dry-run] [--repo-root <dir>]
  intake.mjs check [--fix] [--json] [--repo-root <dir>]
`;

export function parseArgs(argv) {
  const flags = { json: false, dryRun: false, fix: false, repoRoot: DEFAULT_REPO_ROOT };
  const positional = [];
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--json') flags.json = true;
    else if (arg === '--dry-run') flags.dryRun = true;
    else if (arg === '--fix') flags.fix = true;
    else if (arg === '--repo-root') {
      i += 1;
      flags.repoRoot = path.resolve(argv[i]);
    } else if (arg.startsWith('-')) {
      throw new Error(`Unknown flag: ${arg}`);
    } else positional.push(arg);
  }
  return { command: positional[0], positional: positional.slice(1), flags };
}

function printPlan({ rows, stops }) {
  if (rows.length) {
    console.log('| Source | Destination | Class | Title |');
    console.log('| --- | --- | --- | --- |');
    for (const r of rows) {
      console.log(`| ${r.source} | ${r.dest} | ${r.classification} | ${r.title ?? ''} |`);
    }
  } else {
    console.log('_inbox/ is empty - nothing to plan.');
  }
  if (stops.length) {
    console.log('\nSTOPS (resolve before proceeding):');
    for (const s of stops) console.log(`  - ${s.source}: ${s.reason}`);
  }
}

async function main(argv) {
  let parsed;
  try {
    parsed = parseArgs(argv);
  } catch (err) {
    console.error(err.message);
    console.error(USAGE);
    return EXIT.USAGE;
  }
  const { command, flags } = parsed;

  if (command === 'plan') {
    const result = await plan({ repoRoot: flags.repoRoot });
    if (flags.json) console.log(JSON.stringify(result, null, 2));
    else printPlan(result);
    return result.stops.length ? EXIT.AMBIGUOUS : EXIT.OK;
  }

  console.error(command ? `Unknown command: ${command}` : 'No command given.');
  console.error(USAGE);
  return EXIT.USAGE;
}

process.exitCode = await main(process.argv.slice(2));
```

- [ ] **Step 6: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/manifest.test.mjs`

Expected: PASS — 4 tests, 0 failures.

- [ ] **Step 7: Verify the stop path by hand**

Run:

```bash
printf '# Stray\n' > "_inbox/stray.md"
node scripts/docs-intake/intake.mjs plan; echo "exit=$?"
rm "_inbox/stray.md"
```

Expected: the table is empty or lists nothing for `stray.md`, a `STOPS` section names it with "no section folder", and `exit=2`.

- [ ] **Step 8: Commit**

```bash
git add scripts/docs-intake/lib/manifest.mjs scripts/docs-intake/intake.mjs \
  .claude/skills/docs-intake/test/manifest.test.mjs \
  .claude/skills/docs-intake/test/fixtures
git commit -m "Add docs-intake plan subcommand and fixture repo"
```

---

### Task 6: The `rewrite-refs` subcommand

**Files:**
- Modify: `scripts/docs-intake/intake.mjs` (add the `rewrite-refs` branch and its imports)
- Modify: `scripts/docs-intake/lib/manifest.mjs` (add `sectionOfDest`)
- Test: `.claude/skills/docs-intake/test/rewrite-refs.test.mjs`

**Interfaces:**
- Consumes: `rewriteImageRefs`, `kebabLinkTargets`, `plumbingReport` from `lib/refs.mjs`; `EXIT`, `parseArgs` from `intake.mjs`.
- Produces:
  - `sectionOfDest(destRelPath: string) -> string|null` (exported from `lib/manifest.mjs`)
  - CLI `node scripts/docs-intake/intake.mjs rewrite-refs <docs-relative-path> [--from-inbox <inbox-relative-path>] [--dry-run]`, which rewrites the file in place (unless `--dry-run`) and prints what changed, the assets to copy, and any embeds the model must re-place.

**`--from-inbox` is what makes an `UPDATE` mechanical.** Given it, the command reads the inbox
file as the new body, runs `plumbingReport` against the existing `docs/` page to re-point
images at the raw URLs already published, then applies the normal rewrite. Embeds the existing
page had that the incoming file lacks are **printed, never re-inserted** — placing one back in
reorganized prose stays the model's call (spec §3.2). Without the flag the command normalizes
a file already in place, which is the `NEW` path.

- [ ] **Step 1: Write the failing test**

Create `.claude/skills/docs-intake/test/rewrite-refs.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import { mkdtemp, mkdir, writeFile, readFile, cp } from 'node:fs/promises';
import { tmpdir } from 'node:os';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE = path.join(HERE, 'fixtures', 'repo');
const CLI = path.resolve(HERE, '../../../../scripts/docs-intake/intake.mjs');

async function scratchRepo() {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-'));
  await cp(FIXTURE, dir, { recursive: true });
  return dir;
}

test('rewrite-refs normalizes images and .md links in place', async () => {
  const repo = await scratchRepo();
  const rel = 'user-guide/getting-started.md';
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  await writeFile(
    path.join(repo, 'docs', rel),
    '# Getting Started\n\n![Run dialog](./images/Run Dialog.png)\n\n[Overview](Index.md)\n',
    'utf8',
  );

  const out = execFileSync(
    process.execPath,
    [CLI, 'rewrite-refs', rel, '--repo-root', repo],
    { encoding: 'utf8' },
  );

  const body = await readFile(path.join(repo, 'docs', rel), 'utf8');
  assert.ok(body.includes(
    'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/user-guide/run-dialog.png',
  ));
  assert.ok(body.includes('[Overview](index.md)'));
  assert.ok(out.includes('assets/images/user-guide/run-dialog.png'), out);
});

test('rewrite-refs --dry-run leaves the file untouched', async () => {
  const repo = await scratchRepo();
  const rel = 'user-guide/dry.md';
  const original = '# Dry\n\n![A](./images/A.png)\n';
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  await writeFile(path.join(repo, 'docs', rel), original, 'utf8');

  execFileSync(process.execPath, [CLI, 'rewrite-refs', rel, '--dry-run', '--repo-root', repo], {
    encoding: 'utf8',
  });

  assert.equal(await readFile(path.join(repo, 'docs', rel), 'utf8'), original);
});

test('rewrite-refs --from-inbox preserves published image URLs and reports dropped embeds', async () => {
  const repo = await scratchRepo();
  const rel = 'scripting/base-tutorials/hello-world.md';

  const out = execFileSync(
    process.execPath,
    [
      CLI, 'rewrite-refs', rel,
      '--from-inbox', 'scripting/base-tutorials/Hello World.md',
      '--repo-root', repo,
    ],
    { encoding: 'utf8' },
  );

  const body = await readFile(path.join(repo, 'docs', rel), 'utf8');
  assert.ok(body.includes('A rewritten opening paragraph'), 'incoming prose landed');
  assert.ok(
    body.includes(
      'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/script-editor.png',
    ),
    'published image URL preserved, not re-derived',
  );
  assert.ok(!body.includes('./images/Script_Editor.png'), 'local path gone');
  assert.ok(out.includes('re-place embed'), out);
  assert.ok(out.includes('scripting/samples/hello.py#l1-l12'), out);
});

test('rewrite-refs exits 64 without a path', () => {
  assert.throws(
    () => execFileSync(process.execPath, [CLI, 'rewrite-refs'], { encoding: 'utf8', stdio: 'pipe' }),
    (err) => err.status === 64,
  );
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/rewrite-refs.test.mjs`

Expected: FAIL — `Unknown command: rewrite-refs`, exit 64.

- [ ] **Step 3: Add the subcommand to `intake.mjs`**

Add to the imports at the top:

```js
import { readFile, writeFile } from 'node:fs/promises';
import { rewriteImageRefs, kebabLinkTargets, plumbingReport } from './lib/refs.mjs';
import { plan, sectionOfDest } from './lib/manifest.mjs';
```

(The existing `import { plan } from './lib/manifest.mjs';` line is replaced by the combined
import above — do not leave two import statements for the same module.)

Extend `parseArgs` to accept the new flag. In the flags initializer add `fromInbox: null`, and
add this branch alongside the other `else if` clauses, **before** the `arg.startsWith('-')`
catch-all:

```js
    else if (arg === '--from-inbox') {
      i += 1;
      flags.fromInbox = argv[i];
    }
```

And update `USAGE` to:

```js
const USAGE = `Usage:
  intake.mjs plan [--json] [--repo-root <dir>]
  intake.mjs rewrite-refs <docs-relative-path> [--from-inbox <inbox-relative-path>] [--dry-run] [--repo-root <dir>]
  intake.mjs check [--fix] [--json] [--repo-root <dir>]
`;
```

Add `sectionOfDest` to `lib/manifest.mjs` (it belongs with the other path-derived helpers):

```js
export function sectionOfDest(destRelPath) {
  const [first, ...rest] = destRelPath.split('/');
  return rest.length ? first : null;
}
```

Insert this branch in `main`, immediately after the `plan` branch:

```js
  if (command === 'rewrite-refs') {
    const rel = parsed.positional[0];
    if (!rel) {
      console.error('rewrite-refs needs a docs-relative path.');
      console.error(USAGE);
      return EXIT.USAGE;
    }
    const section = sectionOfDest(rel);
    if (!section) {
      console.error(`"${rel}" is a docs-root file with no section - nothing to rewrite.`);
      return EXIT.AMBIGUOUS;
    }

    const abs = path.join(flags.repoRoot, 'docs', rel);
    const original = await readFile(abs, 'utf8');

    // With --from-inbox this is an UPDATE: incoming prose becomes the body, and the
    // plumbing the published page already carries is preserved onto it.
    let before = original;
    let droppedEmbeds = [];
    let preservedImages = [];
    if (flags.fromInbox) {
      const incoming = await readFile(path.join(flags.repoRoot, '_inbox', flags.fromInbox), 'utf8');
      const merged = plumbingReport(incoming, original);
      before = merged.text;
      droppedEmbeds = merged.droppedEmbeds;
      preservedImages = merged.preservedImages;
    }

    const images = rewriteImageRefs(before, { section });
    const links = kebabLinkTargets(images.text);

    if (!flags.dryRun && links.text !== original) await writeFile(abs, links.text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}${rel}`);
    for (const r of preservedImages) console.log(`  preserved image: ${r.from} -> ${r.to}`);
    for (const r of images.rewritten) console.log(`  image: ${r.from} -> ${r.to}`);
    for (const r of links.rewritten) console.log(`  link:  ${r.from} -> ${r.to}`);
    for (const a of images.assets) {
      console.log(`  copy asset: ${a.sourceBasename} -> docs/${a.destRelPath}`);
    }
    for (const e of droppedEmbeds) {
      console.log(`  re-place embed (yours to position): ${e.file}`);
    }
    if (
      !preservedImages.length &&
      !images.rewritten.length &&
      !links.rewritten.length &&
      !droppedEmbeds.length
    ) {
      console.log('  no changes');
    }
    return EXIT.OK;
  }
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/rewrite-refs.test.mjs`

Expected: PASS — 3 tests, 0 failures.

- [ ] **Step 5: Commit**

```bash
git add scripts/docs-intake/intake.mjs scripts/docs-intake/lib/manifest.mjs \
  .claude/skills/docs-intake/test/rewrite-refs.test.mjs
git commit -m "Add docs-intake rewrite-refs subcommand"
```

---

### Task 7: `lib/validate.mjs` and the `check` subcommand

**Files:**
- Create: `scripts/docs-intake/lib/validate.mjs`
- Modify: `scripts/docs-intake/intake.mjs` (add the `check` branch and its import)
- Test: `.claude/skills/docs-intake/test/validate.test.mjs`

**Interfaces:**
- Consumes: `walk` from `lib/manifest.mjs`; `extractRefs`, `isExternal` from `lib/refs.mjs`; `parseSummary`, `orphans` from `lib/summary.mjs`.
- Produces:
  - `STRUCTURAL: string[]`
  - `parseShadowDocs(yamlText: string) -> string[]`
  - `parseLycheeIgnore(text: string) -> RegExp[]`
  - `runLint(repoRoot, { fix }) -> { ok: boolean, output: string }`
  - `checkLocalRefs(repoRoot) -> Promise<{page,kind,target}[]>`
  - `collectRawUrls(repoRoot) -> Promise<string[]>`
  - `headCheck(urls: string[], { concurrency, fetchImpl }) -> Promise<{url,status}[]>`
  - `checkOrphans(repoRoot) -> Promise<string[]>`
  - `proposeLycheeIgnore(text: string, urls: string[], { note }) -> { text, added }`
  - CLI `node scripts/docs-intake/intake.mjs check [--fix] [--json]`, exit `0` clean / `1` on any failure

`headCheck` takes an injectable `fetchImpl` so tests never touch the network. The CLI passes
the global `fetch`.

- [ ] **Step 1: Write the failing test**

Create `.claude/skills/docs-intake/test/validate.test.mjs`:

```js
import { test } from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
  parseShadowDocs,
  parseLycheeIgnore,
  checkLocalRefs,
  collectRawUrls,
  headCheck,
  checkOrphans,
  proposeLycheeIgnore,
} from '../../../../scripts/docs-intake/lib/validate.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE = path.join(HERE, 'fixtures', 'repo');

test('parseShadowDocs reads the list without a YAML dependency', () => {
  const yaml = [
    'root: ./docs',
    'structure:',
    '  readme: Introduction.md',
    'shadowdocs:',
    '  - api/api/scripting-api-reference.md',
    '  - api/api/other.md',
    'publishspace: true',
  ].join('\n');
  assert.deepEqual(parseShadowDocs(yaml), [
    'api/api/scripting-api-reference.md',
    'api/api/other.md',
  ]);
});

test('parseLycheeIgnore skips comments and blanks', () => {
  const patterns = parseLycheeIgnore('# a comment\n\noperations-examples\\.md\n');
  assert.equal(patterns.length, 1);
  assert.ok(patterns[0].test('docs/x/operations-examples.md'));
});

test('checkLocalRefs finds a broken embed in the fixture repo', async () => {
  // The fixture hello-world.md embeds scripting/samples/hello.py, which does not exist.
  const broken = await checkLocalRefs(FIXTURE);
  assert.ok(
    broken.some((b) => b.kind === 'embed' && b.target.startsWith('scripting/samples/hello.py')),
    JSON.stringify(broken),
  );
});

test('collectRawUrls dedupes the raw-main URLs in docs/', async () => {
  const urls = await collectRawUrls(FIXTURE);
  assert.deepEqual(urls, [
    'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/script-editor.png',
  ]);
});

test('headCheck reports non-ok responses and never hits the network in tests', async () => {
  const seen = [];
  const fetchImpl = async (url) => {
    seen.push(url);
    return { ok: url.endsWith('good.png'), status: url.endsWith('good.png') ? 200 : 404 };
  };
  const failures = await headCheck(['https://x/good.png', 'https://x/bad.png'], {
    concurrency: 2,
    fetchImpl,
  });
  assert.equal(seen.length, 2);
  assert.deepEqual(failures, [{ url: 'https://x/bad.png', status: 404 }]);
});

test('checkOrphans reports nothing for the fully-wired fixture', async () => {
  assert.deepEqual(await checkOrphans(FIXTURE), []);
});

test('proposeLycheeIgnore appends an escaped, commented block', () => {
  const { text, added } = proposeLycheeIgnore('# existing\n', ['https://x/a.png'], {
    note: 'PR #12',
  });
  assert.ok(text.startsWith('# existing\n'));
  assert.ok(text.includes('TEMPORARY'));
  assert.ok(text.includes('PR #12'));
  assert.ok(text.includes('https://x/a\\.png'));
  assert.deepEqual(added, ['https://x/a.png']);
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/validate.test.mjs`

Expected: FAIL — `Cannot find module .../lib/validate.mjs`.

- [ ] **Step 3: Write `lib/validate.mjs`**

```js
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
```

- [ ] **Step 4: Add the `check` branch to `intake.mjs`**

Add to the imports:

```js
import {
  runLint,
  checkLocalRefs,
  collectRawUrls,
  headCheck,
  checkOrphans,
} from './lib/validate.mjs';
```

Insert this branch after the `rewrite-refs` branch:

```js
  if (command === 'check') {
    const lint = runLint(flags.repoRoot, { fix: flags.fix });
    const brokenRefs = await checkLocalRefs(flags.repoRoot);
    const rawUrls = await collectRawUrls(flags.repoRoot);
    const rawFailures = await headCheck(rawUrls);
    const orphanPages = await checkOrphans(flags.repoRoot);

    const result = {
      lint: { ok: lint.ok, output: lint.output },
      brokenRefs,
      rawUrlsChecked: rawUrls.length,
      rawFailures,
      orphans: orphanPages,
    };

    if (flags.json) {
      console.log(JSON.stringify(result, null, 2));
    } else {
      console.log(`lint: ${lint.ok ? 'PASS' : 'FAIL'}`);
      if (!lint.ok) console.log(lint.output);
      console.log(`broken links/embeds: ${brokenRefs.length}`);
      for (const b of brokenRefs) console.log(`  ${b.page} [${b.kind}] -> ${b.target}`);
      console.log(`raw URLs checked: ${rawUrls.length}, failures: ${rawFailures.length}`);
      for (const f of rawFailures) console.log(`  ${f.status} ${f.url}`);
      console.log(`orphans (on disk, absent from Summary.md): ${orphanPages.length}`);
      for (const o of orphanPages) console.log(`  ${o}`);
    }

    const clean =
      lint.ok && !brokenRefs.length && !rawFailures.length && !orphanPages.length;
    return clean ? EXIT.OK : EXIT.VALIDATION;
  }
```

- [ ] **Step 5: Run the test to verify it passes**

Run: `node --test .claude/skills/docs-intake/test/validate.test.mjs`

Expected: PASS — 7 tests, 0 failures.

- [ ] **Step 6: Run `check` against the real repo to see its current state**

Run: `node scripts/docs-intake/intake.mjs check; echo "exit=$?"`

Expected: `exit=1`, because the repo has known pre-existing issues — `docs/api/index.md` is an orphan, and the `.lycheeignore` TODO block covers links to files that were never copied. **Record the output; it is the baseline.** An intake run is judged against this baseline, not against zero. Do not "fix" these here — they are explicitly out of scope per spec §8.

- [ ] **Step 7: Commit**

```bash
git add scripts/docs-intake/lib/validate.mjs scripts/docs-intake/intake.mjs \
  .claude/skills/docs-intake/test/validate.test.mjs
git commit -m "Add docs-intake check subcommand"
```

---

### Task 8: `SKILL.md`, the CONTRIBUTING pointer, and an end-to-end dry run

**Files:**
- Create: `.claude/skills/docs-intake/SKILL.md`
- Modify: `CONTRIBUTING.md` (add "Automated intake" after the Workflow section, around line 87)

**Interfaces:**
- Consumes: all three subcommands from Tasks 5–7.
- Produces: the invocable skill. Nothing depends on this task.

- [ ] **Step 1: Write `SKILL.md`**

```markdown
---
name: docs-intake
description: Use when new documentation files have been dropped into _inbox/ and need to be organized into the docs/ structure as Archbee-compatible markdown - normalizes paths, rewrites image refs to raw-GitHub URLs, merges updates into existing pages, registers new pages in Summary.md, validates locally, and opens a PR. Triggers on "process the inbox", "organize these docs", "I dropped files in".
---

# Docs Intake

Turn raw source-repo markdown in `_inbox/` into Archbee-ready pages in `docs/`.

**The structure rules live in [`CONTRIBUTING.md`](../../../CONTRIBUTING.md), section "How
Archbee builds the site". Read them before touching structure — they are strict and fail
silently.** This skill is the procedure, not the rules; it does not restate them.

## Non-negotiables

- `Summary.md` and `.lycheeignore` are written **once per run**, at stages 3 and 4. Never
  from a per-file step.
- **Never rename a file under `samples/`.** Sample filenames are referenced verbatim by
  `use{file=...}` embeds, prose links, and `.lycheeignore`.
- **Every ambiguity is a stop.** Ask the user. Do not guess a section or a nav group.
- Incoming prose is authoritative; local Archbee plumbing is preserved.

## Stage 0 — Preflight

- [ ] `git rev-parse --abbrev-ref HEAD` is `main`; `git status --porcelain` is empty.
- [ ] `git pull`
- [ ] `_inbox/` has files other than `README.md`. If not, stop and say so.

If the tree is dirty or you are not on `main`, stop — an intake PR must not carry unrelated
edits.

## Stage 1 — Plan

```bash
node scripts/docs-intake/intake.mjs plan
```

Exit `2` means there are stops. Resolve every one with the user before continuing. Review the
table: each row's destination and `NEW`/`UPDATE` classification. If a filename is close to but
not exactly an existing page (a rename upstream), that is a **counterpart-matching judgment
call** — ask which page it updates rather than creating a near-duplicate.

## Stage 2 — Place, per file

**`NEW`:**

1. Copy `_inbox/<source>` to `docs/<dest>`.
2. `node scripts/docs-intake/intake.mjs rewrite-refs <dest>`
3. Copy each asset the command lists into `docs/assets/images/<section>/`.
4. Note the page for stage 3.

**`UPDATE`:**

1. ```bash
   node scripts/docs-intake/intake.mjs rewrite-refs <dest> --from-inbox <source>
   ```

   Incoming prose becomes the body; the raw-GitHub image URLs the published page already
   carries are preserved onto it, and links are kebab-cased. Run with `--dry-run` first if
   the page is one you want to eyeball before it changes.
2. **Re-place every embed the command lists under `re-place embed`.** The existing page had it
   and the incoming file does not; putting it back at the right point in reorganized prose is
   your call, not the tooling's.
3. Read the resulting diff. **Flag any deletion over ~10 lines** for the PR body — a truncated
   source file and a deliberate cut are identical in a diff.
4. If the incoming file adds images the published page never had, copy them into
   `docs/assets/images/<section>/` as the command's `copy asset` lines direct.

## Stage 3 — Wire the nav (one write)

For each `NEW` page, append `- [Title](path)` to the end of its group in `docs/Summary.md`.
The group comes from the most-specific directory prefix among existing entries. **If the
group is ambiguous, stop and ask** — `api/api/` genuinely is, because `examples-overview.md`
sits there but is listed under **API Examples**.

`UPDATE` pages need no nav change.

## Stage 4 — Validate (one write)

```bash
node scripts/docs-intake/intake.mjs check
```

Compare against the repo's known baseline — `docs/api/index.md` is already an orphan, and the
`.lycheeignore` TODO block suppresses links to files never copied. **New** problems are yours
to fix; pre-existing ones are out of scope (see `internal/design-docs-intake-skill.md` §8).

- Auto-fixable lint: re-run with `--fix`, then `check` again.
- Anything else unresolved: stop. Do not open a knowingly-red PR.
- For each brand-new asset, append a commented temporary `.lycheeignore` entry — a raw-`main`
  URL 404s until the PR merges. List the lines to delete after merge in the PR body.

## Stage 5 — Ship

```bash
git checkout -b docs/intake-$(date +%Y-%m-%d)
git add docs/ .lycheeignore
git rm -r --cached _inbox 2>/dev/null || true
git commit -F <commit message file>
gh auth switch --hostname github.com --user sciencepolice
git push -u origin HEAD
gh pr create --base main --title "..." --body-file <body file>
gh auth switch --hostname github.com --user dstugan_hrbs
```

Empty `_inbox/` (leaving `README.md`) before committing. Pushing **requires** the
`sciencepolice` account — the `dstugan_hrbs` EMU account can never have access to this repo.
Switch back afterward.

The PR body must carry: a table of every file with classification and destination, every
judgment call made, every large deletion flagged in stage 2, and the `.lycheeignore` lines to
remove after merge.

## Ambiguity stops — ask, never guess

| Condition | Why |
| --- | --- |
| Inbox file at root, no section folder | The drop path is the only section signal. |
| Two plausible `docs/` counterparts | Merging into the wrong page corrupts two at once. |
| Target directory's nav group is ambiguous | Misfiles the page in the portal nav. |
| Dirty tree or not on `main` | Mixes unrelated work into the intake PR. |
| Non-auto-fixable lint or unresolvable link | Prevents a knowingly-red PR. |

## Tests

```bash
node --test ".claude/skills/docs-intake/test/*.test.mjs"
```
```

- [ ] **Step 2: Add the CONTRIBUTING pointer**

In `CONTRIBUTING.md`, immediately after the numbered Workflow list (after the "Get review …
merge" item, around line 87), insert:

```markdown
### Automated intake

For a batch of files copied from a source repo, drop them into `_inbox/` mirroring the `docs/`
tree and ask Claude Code to run the intake. The `docs-intake` skill
(`.claude/skills/docs-intake/SKILL.md`) applies the structure rules above, wires new pages
into `Summary.md`, validates locally, and opens the PR. It stops and asks whenever a section
or nav group is ambiguous rather than guessing. Design notes:
`internal/design-docs-intake-skill.md`.
```

- [ ] **Step 3: Run the whole suite**

Run: `node --test ".claude/skills/docs-intake/test/*.test.mjs"`

Expected: PASS — 34 tests across 6 files, 0 failures.

- [ ] **Step 4: End-to-end dry run against real content**

This is the spec §7 exemplar: `docs/scripting/base-tutorials/hello-world.md`.

```bash
mkdir -p "_inbox/scripting/base-tutorials"
cp docs/scripting/base-tutorials/hello-world.md "_inbox/scripting/base-tutorials/Hello World.md"
node scripts/docs-intake/intake.mjs plan
```

Expected: one row — source `scripting/base-tutorials/Hello World.md`, destination
`scripting/base-tutorials/hello-world.md`, classification **UPDATE**, exit `0`. That proves
kebab-casing and counterpart detection on real content.

Then confirm the dry run writes nothing:

```bash
node scripts/docs-intake/intake.mjs rewrite-refs scripting/base-tutorials/hello-world.md --dry-run
git status --porcelain docs/
rm -rf "_inbox/scripting"
```

Expected: the command prints `[dry-run]` and its findings; `git status` on `docs/` prints
nothing.

- [ ] **Step 5: Commit**

```bash
git add .claude/skills/docs-intake/SKILL.md CONTRIBUTING.md
git commit -m "Add docs-intake skill definition and CONTRIBUTING pointer"
```

---

## Refinements to the Spec

Decisions made here that the approved spec did not cover. Each is a deliberate addition, not a
drift — fold them back into `internal/design-docs-intake-skill.md` if they survive review.

1. **`samples/` filenames are never renamed.** The spec's blanket kebab-case rule would have
   rewritten `Run_Data_Next.cs` and broken `use{file=...}` embeds, prose links, and three
   existing `.lycheeignore` entries. Directory segments are still kebab-cased.
2. **Only `.md` link targets are kebab-cased.** `.cs`/`.py` targets are verbatim source names.
3. **`Introduction.md` and `config.md` are allowed at inbox root**, as the named exceptions to
   "a root drop is an error." A dropped `Summary.md` is a stop — the skill owns that file.
4. **`fetch` replaces `curl -I`.** Node 24 has global `fetch`; one less external process, and
   `headCheck` takes an injectable `fetchImpl` so tests stay offline.
5. **`check` has a known non-zero baseline** on the real repo (`docs/api/index.md` orphan, plus
   the `.lycheeignore` TODO block). Runs are judged against that baseline, not against zero.
6. **Tests run against a fixture repo**, not the live tree, so they stay green as content
   changes. One end-to-end dry run (Task 8, Step 4) exercises real content.
7. **Ref targets may contain spaces.** The image and link patterns use a lazy `[^)]*?` target
   group rather than `[^)\s]+`. Source-repo markdown routinely carries unencoded spaces
   (`./images/Run Dialog.png`), and those are exactly the refs that must be rewritten — a
   whitespace-terminated pattern would skip them silently while reporting success.
8. **`rewrite-refs --from-inbox` performs the `UPDATE` merge.** The spec assigned the whole
   merge to the model; in practice the image-URL half is fully deterministic, so the tooling
   does it and the model handles only what genuinely needs judgment — re-placing embeds and
   reviewing large deletions. Keeps the subcommand count at three.
