# `docs-intake` Addendum: Post-Review Blocker Fixes

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this addendum task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Date:** 2026-09-11
**Parent plan:** `internal-plan-docs-intake-skill.md` → `internal/plan-docs-intake-skill.md`
**Spec:** `internal/design-docs-intake-skill.md`

**Goal:** Close the blockers found by the final whole-branch review and by the first real
77-file inbox drop, so the skill's full pipeline actually executes and cannot silently
duplicate or overwrite published content.

**Why:** The eight original tasks each built faithfully to their brief and each passed review.
But the review found that **spec stages 3 and 4 never execute** — `groupForDirectory`,
`appendEntry`, and `proposeLycheeIgnore` are tested and have no production caller — and the
first real drop then demonstrated that **spec §6's "two plausible counterparts" stop was never
implemented at all**. That drop mirrored `_inbox/api/…` against a repo keeping those pages at
`docs/api/api/…`; all 24 affected files classified `NEW` and `plan` reported **0 stops**.
Applying it would have published 24 duplicates beside the live pages.

## Global Constraints

All constraints from the parent plan's Global Constraints section still bind, in particular:

- Node >= 24; zero runtime dependencies; no `package.json`.
- Raw asset base URL, verbatim: `https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/`
- Never rename a file under a `samples/` directory; never kebab-case a `.cs`/`.py` link target.
- Structural root files keep capitalization: `Introduction.md`, `Summary.md`, `config.md`.
- `Summary.md` and `.lycheeignore` get exactly one writer per run.
- Exit codes: `0` clean, `1` validation failure, `2` ambiguity needing a human, `64` usage error.
- **Test command:** `node --test ".claude/skills/docs-intake/test/*.test.mjs"` — the quoted
  glob is required; the directory form dies with `MODULE_NOT_FOUND` on Node 24 for Windows.
- Nothing under `docs/`, `_inbox/`, or `.lycheeignore` may be modified by implementation work.
  Tests operate on temp copies of `.claude/skills/docs-intake/test/fixtures/repo`.

## Findings Addressed

| Finding | Severity | Task |
| --- | --- | --- |
| Spec §6's "two plausible counterparts" stop never implemented — 24 would-be duplicates, 0 stops | **Critical** | 9 |
| `groupForDirectory`, `appendEntry`, `proposeLycheeIgnore` have no production caller — stages 3/4 are dead code | **Critical** | 10 |
| `README.md` skipped at every depth then discarded — a dropped section landing page is lost | Important | 9 |
| No try/catch around `main`; an I/O error exits `1`, colliding with "validation failure" | Important | 9 |
| Structural root files classify in `plan` but `rewrite-refs` rejects them with exit `2` | Important | 9 |
| Non-markdown rows show a page destination instead of `assets/images/…` | Minor | 9 |
| `.lycheeignore` suppression proposed without checking the asset exists locally — masks a missing image | Important | 10 |
| Spec §8's "incoming file satisfies an existing suppression" report unimplemented | Important | 10 |
| Asset destinations key on basename only — a dropped `overview.png` silently overwrites a live image | Important | 11 |
| `SKILL.md` stage 5 runs `git rm -r --cached _inbox`, staging deletion of the tracked `_inbox/README.md` | Important | 11 |
| `SKILL.md` stage 5 tells the agent to empty `_inbox/` in prose *after* the commit block | Important | 11 |
| `SKILL.md` uses bash-only syntax in a PowerShell-primary environment | Minor | 11 |

---

### Task 9: `plan` safety — counterpart collisions, README scope, exit codes, asset rows

**Files:**
- Modify: `scripts/docs-intake/lib/manifest.mjs`
- Modify: `scripts/docs-intake/intake.mjs`
- Test: `.claude/skills/docs-intake/test/manifest.test.mjs` (extend)

**Interfaces:**
- Consumes: `walk`, `toDestination`, `titleFromMarkdown`, `extractRefs`, `assetDest`.
- Produces:
  - `SKIP_AT_ROOT: Set<string>` replacing the depth-blind `SKIP_FILES`
  - `collisionsFor(repoRoot, destRelPath) -> Promise<string[]>` — existing `docs/` pages sharing the destination's basename, excluding the destination itself
  - `plan()` rows gain `kind: 'page' | 'asset'` and `collisions: string[]`
  - `plan` adds one stop per colliding row, so it exits `2`

- [ ] **Step 1: Write the failing tests**

The existing test file imports only `walk`, `titleFromMarkdown`, `plan`, `path`,
`fileURLToPath`, and `execFileSync`. Add to its imports:

```js
import { mkdtemp, mkdir, writeFile, cp } from 'node:fs/promises';
import { tmpdir } from 'node:os';
```

Append these tests:

```js
test('README.md is skipped only at inbox root, not inside a section', async () => {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-readme-'));
  await cp(FIXTURE, dir, { recursive: true });
  await mkdir(path.join(dir, '_inbox', 'api'), { recursive: true });
  await writeFile(path.join(dir, '_inbox', 'api', 'README.md'), '# API\n\nLanding page.\n', 'utf8');

  const { rows } = await plan({ repoRoot: dir });
  const dests = rows.map((r) => r.dest);
  assert.ok(dests.includes('api/readme.md'), `section README kept, got ${JSON.stringify(dests)}`);
  assert.ok(!dests.includes('readme.md'), 'root README still skipped');
});

test('plan stops on a NEW file whose basename already exists elsewhere in docs/', async () => {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-collide-'));
  await cp(FIXTURE, dir, { recursive: true });
  // The fixture already has docs/scripting/base-tutorials/hello-world.md.
  // Dropping the same basename at a different depth must not pass as a silent NEW.
  await mkdir(path.join(dir, '_inbox', 'scripting'), { recursive: true });
  await writeFile(path.join(dir, '_inbox', 'scripting', 'Hello World.md'), '# Hello World\n', 'utf8');

  const { rows, stops } = await plan({ repoRoot: dir });
  const row = rows.find((r) => r.dest === 'scripting/hello-world.md');
  assert.equal(row.classification, 'NEW');
  assert.deepEqual(row.collisions, ['scripting/base-tutorials/hello-world.md']);
  assert.ok(
    stops.some((s) => /already exists elsewhere/.test(s.reason)),
    `expected a collision stop, got ${JSON.stringify(stops)}`,
  );
});

test('an UPDATE is not reported as colliding with itself', async () => {
  const { rows, stops } = await plan({ repoRoot: FIXTURE });
  const row = rows.find((r) => r.dest === 'scripting/base-tutorials/hello-world.md');
  assert.equal(row.classification, 'UPDATE');
  assert.deepEqual(row.collisions, []);
  assert.deepEqual(stops, []);
});

test('plan marks non-markdown rows as assets with an assets/ destination', async () => {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-asset-'));
  await cp(FIXTURE, dir, { recursive: true });
  await mkdir(path.join(dir, '_inbox', 'user-guide', 'images'), { recursive: true });
  await writeFile(path.join(dir, '_inbox', 'user-guide', 'images', 'Run Dialog.PNG'), 'x', 'utf8');

  const { rows } = await plan({ repoRoot: dir });
  const row = rows.find((r) => r.source.endsWith('Run Dialog.PNG'));
  assert.equal(row.kind, 'asset');
  assert.equal(row.dest, 'assets/images/user-guide/run-dialog.png');
});

test('a missing file surfaces as a usage error, not a validation failure', () => {
  try {
    execFileSync(process.execPath, [CLI, 'rewrite-refs', 'nope/missing.md', '--repo-root', FIXTURE], {
      encoding: 'utf8',
      stdio: 'pipe',
    });
    assert.fail('expected a non-zero exit');
  } catch (err) {
    assert.equal(err.status, 64, `expected 64, got ${err.status}`);
  }
});
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `node --test .claude/skills/docs-intake/test/manifest.test.mjs`

Expected: FAIL — `api/readme.md` absent, `row.collisions` undefined, `row.kind` undefined, and the missing-file case exits `1` rather than `64`.

- [ ] **Step 3: Rescope the skip list and widen `walk` in `lib/manifest.mjs`**

Replace the `SKIP_FILES` constant with:

```js
// Skipped only at the inbox ROOT. A README.md inside a section is real content:
// the repo convention is one index.md landing page per section, and discarding a
// dropped landing page loses a real page. This bit on the first 77-file drop -
// docs/api/index.md is an orphan that the dropped _inbox/api/README.md would fix.
export const SKIP_AT_ROOT = new Set(['README.md', 'Thumbs.db', '.DS_Store']);

const MARKDOWN = /\.md$/i;
```

Add `assetDest` to the `paths.mjs` import. Then make `walk` return every non-dotfile,
deferring the skip decision to `plan`, which knows depth:

```js
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
```

**Note:** `lib/validate.mjs` also calls `walk` over `docs/`, where it filters to `.md` itself.
Widening `walk` therefore does not change `validate`'s behavior — but confirm that by running
the validate tests, not by assuming.

- [ ] **Step 4: Add the collision detector**

```js
export async function collisionsFor(repoRoot, destRelPath) {
  const base = path.basename(destRelPath).toLowerCase();
  const pages = (await walk(path.join(repoRoot, 'docs'))).filter((p) => MARKDOWN.test(p));
  return pages
    .filter((p) => path.basename(p).toLowerCase() === base && p !== destRelPath)
    .sort();
}
```

- [ ] **Step 5: Rewrite the `plan` loop body**

```js
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
```

- [ ] **Step 6: Show `kind` in the plan table and harden exit codes in `intake.mjs`**

```js
function printPlan({ rows, stops }) {
  if (rows.length) {
    console.log('| Source | Destination | Kind | Class | Title |');
    console.log('| --- | --- | --- | --- | --- |');
    for (const r of rows) {
      console.log(`| ${r.source} | ${r.dest} | ${r.kind} | ${r.classification} | ${r.title ?? ''} |`);
    }
  } else {
    console.log('_inbox/ is empty - nothing to plan.');
  }
  if (stops.length) {
    console.log('\nSTOPS (resolve before proceeding):');
    for (const s of stops) console.log(`  - ${s.source}: ${s.reason}`);
  }
}
```

Replace the file's final `process.exitCode = await main(...)` line with:

```js
try {
  process.exitCode = await main(process.argv.slice(2));
} catch (err) {
  // An I/O or programming error must not masquerade as EXIT.VALIDATION (1),
  // which the contract reserves for "the content failed validation".
  console.error(`docs-intake: ${err?.message ?? err}`);
  process.exitCode = EXIT.USAGE;
}
```

- [ ] **Step 7: Give structural root files a working stage-2 path**

In the `rewrite-refs` branch, `Introduction.md` and `config.md` currently exit `2`. A docs-root
page has no section (so no asset folder), but its `.md` links still need kebab-casing. Replace
the `if (!section)` rejection so `section` being null is tolerated:

```js
    const section = sectionOfDest(rel);
    const abs = path.join(flags.repoRoot, 'docs', rel);
    const original = await readFile(abs, 'utf8');
```

and make only the image pass conditional:

```js
    const images = section
      ? rewriteImageRefs(before, { section })
      : { text: before, rewritten: [], assets: [] };
```

- [ ] **Step 8: Run the full suite**

Run: `node --test ".claude/skills/docs-intake/test/*.test.mjs"`

Expected: PASS — 40 tests, 0 failures (35 existing + 5 new).

- [ ] **Step 9: Commit**

```bash
git add scripts/docs-intake/lib/manifest.mjs scripts/docs-intake/intake.mjs \
  .claude/skills/docs-intake/test/manifest.test.mjs
git commit -m "Stop on counterpart collisions and keep section READMEs"
```

---

### Task 10: Wire stages 3 and 4 — `wire-nav` and `lycheeignore` subcommands

**Files:**
- Modify: `scripts/docs-intake/intake.mjs`
- Modify: `scripts/docs-intake/lib/validate.mjs`
- Test: `.claude/skills/docs-intake/test/wire-nav.test.mjs` (new)
- Test: `.claude/skills/docs-intake/test/validate.test.mjs` (extend)

**Interfaces:**
- Consumes: `parseSummary`, `groupForDirectory`, `appendEntry` from `lib/summary.mjs`; `proposeLycheeIgnore`, `parseLycheeIgnore`, `collectRawUrls` from `lib/validate.mjs`; `titleFromMarkdown` from `lib/manifest.mjs`.
- Produces:
  - `assetExistsLocally(repoRoot, rawUrl) -> boolean` in `lib/validate.mjs`
  - `suppressionsSatisfied(repoRoot) -> Promise<{ pattern: string, nowResolves: string[] }[]>` in `lib/validate.mjs`
  - CLI `wire-nav <docs-relative-path>... [--title <t>] [--dry-run] [--repo-root <dir>]` — appends each page to the end of its resolved nav group in `docs/Summary.md`, in **one** write; exits `2` on an ambiguous group without writing anything
  - CLI `lycheeignore [--dry-run] [--repo-root <dir>]` — appends a commented temporary block for raw URLs that 404 **and** whose file exists locally; exits `2` and writes nothing if any 404 URL has no local file

**Why this task exists:** spec §3.2 and §5 assign nav-group resolution to the script, and spec
§6 names the ambiguous-group case as a stop that prevents misfiling a page in the portal nav.
Today `SKILL.md` instead tells the agent to hand-edit `Summary.md`, so that stop never runs and
the three functions built for it are dead code. `check`'s orphan report catches a *missing*
entry but not a *wrong-group* one.

- [ ] **Step 1: Write the failing `wire-nav` tests**

Create `.claude/skills/docs-intake/test/wire-nav.test.mjs`:

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
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-nav-'));
  await cp(FIXTURE, dir, { recursive: true });
  return dir;
}

test('wire-nav appends to the end of the resolved group', async () => {
  const repo = await scratchRepo();
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  await writeFile(
    path.join(repo, 'docs', 'user-guide', 'getting-started.md'),
    '# Getting Started\n\nBody.\n',
    'utf8',
  );

  const out = execFileSync(
    process.execPath,
    [CLI, 'wire-nav', 'user-guide/getting-started.md', '--repo-root', repo],
    { encoding: 'utf8' },
  );

  const summary = await readFile(path.join(repo, 'docs', 'Summary.md'), 'utf8');
  const lines = summary.split(/\r?\n/);
  const at = lines.indexOf('- [Getting Started](user-guide/getting-started.md)');
  assert.ok(at > lines.indexOf('- [Overview](user-guide/index.md)'), 'after the group’s last entry');
  assert.ok(at < lines.indexOf('## Scripting'), 'before the next group');
  assert.ok(out.includes('User Guide'), out);
});

test('wire-nav derives the title from the page H1 and honours --title', async () => {
  const repo = await scratchRepo();
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  await writeFile(path.join(repo, 'docs', 'user-guide', 'a.md'), '# From The H1\n', 'utf8');
  execFileSync(process.execPath, [CLI, 'wire-nav', 'user-guide/a.md', '--repo-root', repo], {
    encoding: 'utf8',
  });
  let summary = await readFile(path.join(repo, 'docs', 'Summary.md'), 'utf8');
  assert.ok(summary.includes('- [From The H1](user-guide/a.md)'), summary);

  await writeFile(path.join(repo, 'docs', 'user-guide', 'b.md'), '# Ignored\n', 'utf8');
  execFileSync(
    process.execPath,
    [CLI, 'wire-nav', 'user-guide/b.md', '--title', 'Explicit Name', '--repo-root', repo],
    { encoding: 'utf8' },
  );
  summary = await readFile(path.join(repo, 'docs', 'Summary.md'), 'utf8');
  assert.ok(summary.includes('- [Explicit Name](user-guide/b.md)'), summary);
});

test('wire-nav writes Summary.md exactly once for several pages', async () => {
  const repo = await scratchRepo();
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  for (const n of ['one', 'two', 'three']) {
    await writeFile(path.join(repo, 'docs', 'user-guide', `${n}.md`), `# Page ${n}\n`, 'utf8');
  }
  execFileSync(
    process.execPath,
    [
      CLI, 'wire-nav',
      'user-guide/one.md', 'user-guide/two.md', 'user-guide/three.md',
      '--repo-root', repo,
    ],
    { encoding: 'utf8' },
  );

  const summary = await readFile(path.join(repo, 'docs', 'Summary.md'), 'utf8');
  for (const n of ['one', 'two', 'three']) {
    assert.equal(
      summary.split(`(user-guide/${n}.md)`).length - 1,
      1,
      `${n} appears exactly once`,
    );
  }
  const lines = summary.split(/\r?\n/);
  assert.ok(lines.indexOf('- [Page three](user-guide/three.md)') < lines.indexOf('## Scripting'));
});

test('wire-nav exits 2 on an ambiguous group and writes nothing', async () => {
  const repo = await scratchRepo();
  // Add two groups that both own the same directory, reproducing the real api/api case.
  const summaryPath = path.join(repo, 'docs', 'Summary.md');
  const original = await readFile(summaryPath, 'utf8');
  await writeFile(
    summaryPath,
    `${original}\n## Alpha\n\n- [A](shared/a.md)\n\n## Beta\n\n- [B](shared/b.md)\n`,
    'utf8',
  );
  await mkdir(path.join(repo, 'docs', 'shared'), { recursive: true });
  await writeFile(path.join(repo, 'docs', 'shared', 'c.md'), '# C\n', 'utf8');
  const before = await readFile(summaryPath, 'utf8');

  try {
    execFileSync(process.execPath, [CLI, 'wire-nav', 'shared/c.md', '--repo-root', repo], {
      encoding: 'utf8',
      stdio: 'pipe',
    });
    assert.fail('expected exit 2');
  } catch (err) {
    assert.equal(err.status, 2);
    assert.match(`${err.stdout ?? ''}${err.stderr ?? ''}`, /ambiguous/i);
  }

  assert.equal(await readFile(summaryPath, 'utf8'), before, 'Summary.md untouched');
});

test('wire-nav --dry-run leaves Summary.md byte-identical', async () => {
  const repo = await scratchRepo();
  const summaryPath = path.join(repo, 'docs', 'Summary.md');
  const before = await readFile(summaryPath, 'utf8');
  await mkdir(path.join(repo, 'docs', 'user-guide'), { recursive: true });
  await writeFile(path.join(repo, 'docs', 'user-guide', 'dry.md'), '# Dry\n', 'utf8');

  execFileSync(
    process.execPath,
    [CLI, 'wire-nav', 'user-guide/dry.md', '--dry-run', '--repo-root', repo],
    { encoding: 'utf8' },
  );

  assert.equal(await readFile(summaryPath, 'utf8'), before);
});

test('wire-nav is idempotent: an already-listed page is reported, not duplicated', async () => {
  const repo = await scratchRepo();
  const out = execFileSync(
    process.execPath,
    [CLI, 'wire-nav', 'user-guide/index.md', '--repo-root', repo],
    { encoding: 'utf8' },
  );
  const summary = await readFile(path.join(repo, 'docs', 'Summary.md'), 'utf8');
  assert.equal(summary.split('(user-guide/index.md)').length - 1, 1);
  assert.match(out, /already listed/i);
});
```

- [ ] **Step 2: Run them to verify they fail**

Run: `node --test .claude/skills/docs-intake/test/wire-nav.test.mjs`

Expected: FAIL — `Unknown command: wire-nav`, exit `64`.

- [ ] **Step 3: Add `wire-nav` to `intake.mjs`**

Add to the imports:

```js
import { parseSummary, groupForDirectory, appendEntry } from './lib/summary.mjs';
import { plan, sectionOfDest, titleFromMarkdown } from './lib/manifest.mjs';
```

(The `manifest.mjs` import line already exists — extend it rather than adding a second one.)

Add `title: null` to the flags initializer and a `--title` branch to `parseArgs`, before the
`arg.startsWith('-')` catch-all:

```js
    else if (arg === '--title') {
      i += 1;
      flags.title = argv[i];
    }
```

Insert this branch after `rewrite-refs`:

```js
  if (command === 'wire-nav') {
    const rels = parsed.positional;
    if (!rels.length) {
      console.error('wire-nav needs at least one docs-relative path.');
      console.error(USAGE);
      return EXIT.USAGE;
    }
    if (flags.title && rels.length > 1) {
      console.error('--title applies to a single page; pass pages one at a time to name them.');
      return EXIT.USAGE;
    }

    const summaryPath = path.join(flags.repoRoot, 'docs', 'Summary.md');
    let text = await readFile(summaryPath, 'utf8');

    // Resolve every page FIRST: an ambiguous group must abort the whole run before
    // any write, so Summary.md is never left half-wired.
    const planned = [];
    for (const rel of rels) {
      const parsedSummary = parseSummary(text);
      const listed = parsedSummary.groups.some((g) => g.entries.some((e) => e.path === rel))
        || parsedSummary.preamble.some((e) => e.path === rel);
      if (listed) {
        console.log(`  already listed, skipping: ${rel}`);
        continue;
      }

      const { group, ambiguous, candidates } = groupForDirectory(parsedSummary, rel);
      if (ambiguous) {
        const detail = candidates?.length
          ? `candidates: ${candidates.join(', ')}`
          : 'no group owns that directory';
        console.error(
          `ambiguous nav group for "${rel}" (${detail}) - a human must choose; nothing was written.`,
        );
        return EXIT.AMBIGUOUS;
      }

      const body = await readFile(path.join(flags.repoRoot, 'docs', rel), 'utf8');
      const title = flags.title ?? titleFromMarkdown(body, path.basename(rel, '.md'));
      planned.push({ rel, group, title });
      // Apply to the in-memory text so the next iteration sees it and line numbers stay valid.
      text = appendEntry(parseSummary(text), { group, title, path: rel });
    }

    if (!flags.dryRun && planned.length) await writeFile(summaryPath, text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}Summary.md`);
    for (const p of planned) console.log(`  ${p.group}: - [${p.title}](${p.rel})`);
    if (!planned.length) console.log('  no changes');
    return EXIT.OK;
  }
```

Update `USAGE` to include:

```
  intake.mjs wire-nav <docs-relative-path>... [--title <t>] [--dry-run] [--repo-root <dir>]
  intake.mjs lycheeignore [--dry-run] [--repo-root <dir>]
```

- [ ] **Step 4: Run the `wire-nav` tests to verify they pass**

Run: `node --test .claude/skills/docs-intake/test/wire-nav.test.mjs`

Expected: PASS — 6 tests, 0 failures.

- [ ] **Step 5: Write the failing `lycheeignore` tests**

Append to `.claude/skills/docs-intake/test/validate.test.mjs` (add `mkdtemp`, `mkdir`,
`writeFile`, `readFile`, `cp` from `node:fs/promises` and `tmpdir` from `node:os` to its
imports, plus `execFileSync` from `node:child_process`):

```js
test('assetExistsLocally distinguishes a real asset from a dead reference', async () => {
  const { assetExistsLocally } = await import('../../../../scripts/docs-intake/lib/validate.mjs');
  const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';
  assert.equal(assetExistsLocally(FIXTURE, `${RAW}docs/Summary.md`), true);
  assert.equal(assetExistsLocally(FIXTURE, `${RAW}docs/assets/images/nope/missing.png`), false);
  assert.equal(assetExistsLocally(FIXTURE, 'https://example.com/x.png'), false);
});

test('suppressionsSatisfied reports a suppression whose file now exists', async () => {
  const { suppressionsSatisfied } = await import('../../../../scripts/docs-intake/lib/validate.mjs');
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-supp-'));
  await cp(FIXTURE, dir, { recursive: true });
  await writeFile(path.join(dir, '.lycheeignore'), '# c\nhello-world\\.md\nnever-landed\\.md\n', 'utf8');

  const satisfied = await suppressionsSatisfied(dir);
  const patterns = satisfied.map((s) => s.pattern);
  assert.ok(patterns.includes('hello-world\\.md'), JSON.stringify(satisfied));
  assert.ok(!patterns.includes('never-landed\\.md'), 'a still-missing file is not reported');
});

test('lycheeignore refuses to suppress a raw URL with no local file', async () => {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-ign-'));
  await cp(FIXTURE, dir, { recursive: true });
  const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';
  await mkdir(path.join(dir, 'docs', 'user-guide'), { recursive: true });
  await writeFile(
    path.join(dir, 'docs', 'user-guide', 'ghost.md'),
    `# Ghost\n\n![Missing](${RAW}docs/assets/images/user-guide/ghost.png)\n`,
    'utf8',
  );
  await writeFile(path.join(dir, '.lycheeignore'), '# seed\n', 'utf8');
  const before = await readFile(path.join(dir, '.lycheeignore'), 'utf8');

  try {
    execFileSync(process.execPath, [CLI, 'lycheeignore', '--repo-root', dir], {
      encoding: 'utf8',
      stdio: 'pipe',
    });
    assert.fail('expected exit 2');
  } catch (err) {
    assert.equal(err.status, 2);
    assert.match(`${err.stdout ?? ''}${err.stderr ?? ''}`, /no local file/i);
  }

  assert.equal(await readFile(path.join(dir, '.lycheeignore'), 'utf8'), before, 'untouched');
});
```

`CLI` must be defined in this file as it is in the others:
`const CLI = path.resolve(HERE, '../../../../scripts/docs-intake/intake.mjs');`

- [ ] **Step 6: Implement in `lib/validate.mjs`**

```js
/**
 * True when a raw-main URL points at a file that exists in this working tree.
 * A 404 on a URL whose file IS present means "not merged yet" (safe to suppress);
 * a 404 on a URL with NO local file means the asset was never copied - suppressing
 * that hides a broken image from CI and it ships broken to the portal.
 */
export function assetExistsLocally(repoRoot, rawUrl) {
  if (!rawUrl.startsWith(RAW_PREFIX)) return false;
  const rel = decodeURIComponent(rawUrl.slice(RAW_PREFIX.length));
  return existsSync(path.join(repoRoot, rel));
}

/** Suppressions in .lycheeignore whose referenced file now exists under docs/. */
export async function suppressionsSatisfied(repoRoot) {
  const ignorePath = path.join(repoRoot, '.lycheeignore');
  if (!existsSync(ignorePath)) return [];
  const raw = await readFile(ignorePath, 'utf8');
  const patterns = raw
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter((l) => l && !l.startsWith('#'));

  const pages = (await walk(path.join(repoRoot, 'docs'))).map((p) => `docs/${p}`);
  const out = [];
  for (const pattern of patterns) {
    let re;
    try {
      re = new RegExp(pattern);
    } catch {
      continue;
    }
    const nowResolves = pages.filter((p) => re.test(p));
    if (nowResolves.length) out.push({ pattern, nowResolves });
  }
  return out;
}
```

Add near the top, deriving the prefix from the shared constant rather than retyping the URL:

```js
import { RAW_BASE } from './paths.mjs';
const RAW_PREFIX = RAW_BASE;
```

- [ ] **Step 7: Add the `lycheeignore` branch to `intake.mjs`**

```js
  if (command === 'lycheeignore') {
    const urls = await collectRawUrls(flags.repoRoot);
    const failures = await headCheck(urls);

    const suppressible = [];
    const unsafe = [];
    for (const f of failures) {
      (assetExistsLocally(flags.repoRoot, f.url) ? suppressible : unsafe).push(f.url);
    }

    if (unsafe.length) {
      console.error('Refusing to suppress: these raw URLs 404 and have no local file -');
      console.error('the asset was never copied, so suppressing would ship a broken image.');
      for (const u of unsafe) console.error(`  ${u}`);
      return EXIT.AMBIGUOUS;
    }

    const ignorePath = path.join(flags.repoRoot, '.lycheeignore');
    const before = existsSync(ignorePath) ? await readFile(ignorePath, 'utf8') : '';
    const { text, added } = proposeLycheeIgnore(before, suppressible, {
      note: 'this intake PR',
    });

    if (!flags.dryRun && added.length) await writeFile(ignorePath, text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}.lycheeignore`);
    for (const u of added) console.log(`  temporary suppression: ${u}`);
    if (!added.length) console.log('  no new assets need suppressing');

    const satisfied = await suppressionsSatisfied(flags.repoRoot);
    if (satisfied.length) {
      console.log('\nExisting suppressions that now resolve (consider deleting the line):');
      for (const s of satisfied) console.log(`  ${s.pattern} -> ${s.nowResolves.join(', ')}`);
    }
    return EXIT.OK;
  }
```

Add `assetExistsLocally`, `suppressionsSatisfied`, `proposeLycheeIgnore`, `collectRawUrls`, and
`headCheck` to the `validate.mjs` import, and `existsSync` from `node:fs`.

- [ ] **Step 8: Report satisfied suppressions from `check` too**

Spec §8 requires `check` to report when an incoming file satisfies an existing suppression. In
the `check` branch, add `suppressionsSatisfied` to the gathered results and print them — as
**information, not a failure**: do not let it change the exit code.

```js
    const satisfied = await suppressionsSatisfied(flags.repoRoot);
```

Add `satisfied` to the JSON `result` object, and in the human output:

```js
      console.log(`suppressions that now resolve: ${satisfied.length}`);
      for (const s of satisfied) console.log(`  ${s.pattern} -> ${s.nowResolves.join(', ')}`);
```

- [ ] **Step 9: Run the full suite**

Run: `node --test ".claude/skills/docs-intake/test/*.test.mjs"`

Expected: PASS — 49 tests, 0 failures (40 after Task 9 + 6 wire-nav + 3 lycheeignore).

- [ ] **Step 10: Verify no dead exports remain**

Run:

```bash
for s in groupForDirectory appendEntry proposeLycheeIgnore assetExistsLocally suppressionsSatisfied; do
  echo "--- $s ---"
  grep -rn "$s" scripts/ | grep -v 'lib/summary.mjs:\|lib/validate.mjs:' || echo "  NO PRODUCTION CALLER"
done
```

Expected: every symbol has at least one caller in `scripts/docs-intake/intake.mjs`. If any
still prints `NO PRODUCTION CALLER`, the Critical finding is not closed — fix before committing.

- [ ] **Step 11: Commit**

```bash
git add scripts/docs-intake/intake.mjs scripts/docs-intake/lib/validate.mjs \
  .claude/skills/docs-intake/test/wire-nav.test.mjs \
  .claude/skills/docs-intake/test/validate.test.mjs
git commit -m "Wire stages 3 and 4 into the CLI as wire-nav and lycheeignore"
```

---

### Task 11: Asset collision guard and `SKILL.md` corrections

**Files:**
- Modify: `scripts/docs-intake/lib/refs.mjs`
- Modify: `scripts/docs-intake/intake.mjs`
- Modify: `.claude/skills/docs-intake/SKILL.md`
- Test: `.claude/skills/docs-intake/test/refs.test.mjs` (extend)

**Interfaces:**
- Produces: `rewriteImageRefs(markdown, { section, existingAssets })` — `existingAssets` is an optional `Set<string>` of asset destination paths already present in `docs/`; any incoming image whose computed destination is in that set is reported as a **collision** instead of being silently pointed at it.
- `rewriteImageRefs` return value gains `collisions: { sourceBasename, destRelPath }[]`.

**Why:** assets are keyed by basename alone, so a dropped `overview.png` computes the same
destination as a live published `overview.png` used by a different page. The agent's copy step
then overwrites real customer-facing artwork, and `check` cannot see it because the URL still
resolves. This is the only path the final review found to *silent* corruption of published
content.

- [ ] **Step 1: Write the failing test**

Append to `.claude/skills/docs-intake/test/refs.test.mjs`:

```js
test('rewriteImageRefs flags an asset whose destination already exists', () => {
  const md = '![Overview](./images/Overview.png)\n\n![Fresh](./images/brand-new.png)';
  const existingAssets = new Set(['assets/images/user-guide/overview.png']);
  const { rewritten, assets, collisions } = rewriteImageRefs(md, {
    section: 'user-guide',
    existingAssets,
  });

  assert.deepEqual(collisions, [
    { sourceBasename: 'Overview.png', destRelPath: 'assets/images/user-guide/overview.png' },
  ]);
  assert.equal(assets.length, 1, 'only the non-colliding asset is queued for copying');
  assert.equal(assets[0].sourceBasename, 'brand-new.png');
  assert.equal(rewritten.length, 2, 'both URLs are still rewritten');
});

test('rewriteImageRefs without existingAssets behaves exactly as before', () => {
  const md = '![A](./images/a.png)';
  const { assets, collisions } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.equal(assets.length, 1);
  assert.deepEqual(collisions, []);
});
```

- [ ] **Step 2: Run it to verify it fails**

Run: `node --test .claude/skills/docs-intake/test/refs.test.mjs`

Expected: FAIL — `collisions` is `undefined`.

- [ ] **Step 3: Implement in `lib/refs.mjs`**

```js
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
```

- [ ] **Step 4: Pass the existing assets in from the CLI**

In `intake.mjs`'s `rewrite-refs` branch, build the set from disk before rewriting, and print
collisions prominently:

```js
    const assetsDir = path.join(flags.repoRoot, 'docs', 'assets');
    const existingAssets = new Set(
      (await walk(assetsDir)).map((p) => `assets/${p}`),
    );
```

Add `walk` to the `manifest.mjs` import. Pass `existingAssets` into `rewriteImageRefs`, and
after the `copy asset` lines:

```js
    for (const c of images.collisions) {
      console.log(`  ASSET COLLISION - do NOT copy over the live file: ${c.sourceBasename} -> docs/${c.destRelPath}`);
      console.log('    rename the incoming file, or confirm it is the same image, before copying.');
    }
```

- [ ] **Step 5: Correct `SKILL.md`**

Four changes. Do not restate the Archbee structure rules — the file must keep citing
`CONTRIBUTING.md`.

1. **Delete the destructive line** from stage 5:

   ```
   git rm -r --cached _inbox 2>/dev/null || true
   ```

   It stages deletion of the tracked `_inbox/README.md` (reproduced in a scratch repo), and it
   is unnecessary because `_inbox/*` is already git-ignored, so nothing under it can be staged.

2. **Move the "empty `_inbox/`" instruction ahead of the commit**, as a numbered step before
   the command block rather than prose after it — an agent following the block literally
   commits first. Say explicitly: delete everything under `_inbox/` **except `README.md`**.

3. **Replace stages 3 and 4 with the new subcommands.** Stage 3 becomes:

   ```bash
   node scripts/docs-intake/intake.mjs wire-nav <dest>...
   ```

   noting that exit `2` means an ambiguous nav group, that nothing is written in that case, and
   that the human chooses the group. Stage 4 gains, after `check`:

   ```bash
   node scripts/docs-intake/intake.mjs lycheeignore
   ```

   noting that exit `2` means a raw URL 404s with no local file — the asset was never copied,
   and suppressing it would ship a broken image.

4. **Make stage 5's commands shell-neutral.** The environment is PowerShell-primary; `$(date
   +%Y-%m-%d)` and `2>/dev/null` are bash-only. Use a literal branch name placeholder
   (`docs/intake-YYYY-MM-DD`, with the instruction to substitute today's date) and drop the
   redirection.

   Also keep the existing note that pushing requires `gh auth switch --hostname github.com
   --user sciencepolice`, since the `dstugan_hrbs` EMU account can never have access — and add
   that the account should be switched back afterward.

Add an `ASSET COLLISION` row to the ambiguity-stop table: *an incoming image's destination
already exists — renaming or confirming is a human call, never an overwrite.*

- [ ] **Step 6: Run the full suite**

Run: `node --test ".claude/skills/docs-intake/test/*.test.mjs"`

Expected: PASS — 51 tests, 0 failures (49 after Task 10 + 2 new).

- [ ] **Step 7: Confirm nothing real was touched**

Run: `git status --porcelain docs/ _inbox/ .lycheeignore`

Expected: empty. The real `_inbox/` still holds the 77-file drop plus `README.md`; this task
must not process it.

- [ ] **Step 8: Commit**

```bash
git add scripts/docs-intake/lib/refs.mjs scripts/docs-intake/intake.mjs \
  .claude/skills/docs-intake/SKILL.md .claude/skills/docs-intake/test/refs.test.mjs
git commit -m "Guard asset collisions and correct the SKILL.md procedure"
```

---

### Task 12: Round-2 review fixes

Four findings from the round-2 whole-branch review plus one found in real use. All small.

**Files:**
- Modify: `scripts/docs-intake/lib/refs.mjs`
- Modify: `scripts/docs-intake/lib/manifest.mjs`
- Modify: `.claude/skills/docs-intake/SKILL.md`
- Test: `.claude/skills/docs-intake/test/refs.test.mjs`, `.claude/skills/docs-intake/test/manifest.test.mjs`

- [ ] **Fix 1 — `index.md` must not trigger a collision stop** *(found in real use)*

`collisionsFor` matches on basename, but `index.md` is a **deliberately repeated** filename:
`CONTRIBUTING.md` states "one `index.md` per section as its landing page", and `docs/` already
holds three (`api/`, `scripting/`, `user-guide/`). A real drop of a new section landing page
therefore stopped with `"index.md" already exists elsewhere in docs/ (api/index.md,
scripting/index.md, user-guide/index.md)` — a permanent false positive on every future section.

In `lib/manifest.mjs`, exempt the convention filename:

```js
// index.md is the per-section landing-page convention (CONTRIBUTING.md), so it is
// SUPPOSED to recur. Basename collision carries no signal for it.
const COLLISION_EXEMPT = new Set(['index.md']);

export async function collisionsFor(repoRoot, destRelPath) {
  const base = path.basename(destRelPath).toLowerCase();
  if (COLLISION_EXEMPT.has(base)) return [];
  const pages = (await walk(path.join(repoRoot, 'docs'))).filter((p) => MARKDOWN.test(p));
  return pages
    .filter((p) => path.basename(p).toLowerCase() === base && p !== destRelPath)
    .sort();
}
```

Test: a dropped `_inbox/api/client-sdk/index.md` against a fixture holding other `index.md`
files yields `collisions: []` and no stop, while a non-exempt basename still collides.

- [ ] **Fix 2 — stop when a dropped asset would overwrite a published one** *(review finding B)*

`plan` computes collisions only for `isMarkdown && NEW`, so a dropped image whose destination
already exists renders as `asset | UPDATE` with exit `0` and no stop — and a generic name like
`Overview.png` then replaces a different page's published artwork. `check` stays green because
the URL still resolves. `SKILL.md`'s own stop table already says this must stop.

In `plan`, after computing `classification`, add:

```js
    if (!isMarkdown && classification === 'UPDATE') {
      stops.push({
        source,
        reason: `"${path.basename(destRelPath)}" already exists at docs/${destRelPath} - a dropped asset must not overwrite published artwork; rename it, or confirm it is the same image.`,
      });
    }
```

Test: a fixture with `docs/assets/images/user-guide/overview.png` present plus a dropped
`_inbox/user-guide/images/Overview.png` produces a stop and CLI exit `2`.

- [ ] **Fix 3 — never kebab-case a link to a structural root file** *(review finding C)*

Verified by the reviewer: `[Home](../Introduction.md)` becomes `../introduction.md`, and
`check` reports **0** broken links because `existsSync` is case-insensitive on Windows.
Archbee's sync filesystem is case-sensitive, so the portal gets a silently broken link to its
home page. `toDestination` and `validate.mjs`'s `STRUCTURAL` both exempt these files;
`kebabLinkTargets` does not. No instances exist in `docs/` today — this is latent.

In `lib/refs.mjs`, import `STRUCTURAL_ROOT_FILES` alongside the existing `paths.mjs` imports
and add `Summary.md` to the exemption locally (the constant deliberately excludes it because a
dropped `Summary.md` is a stop, but a *link* to it must still not be kebab-cased):

```js
const STRUCTURAL_LINK_TARGETS = new Set([...STRUCTURAL_ROOT_FILES, 'Summary.md']);
```

and in `kebabLinkTargets`, before kebab-casing, return `raw` when the target's basename is in
that set. Test: `[Home](../Introduction.md)`, `[Config](./config.md)` and `[Nav](Summary.md)`
are all left verbatim, while `[Auth](Authentication.md)` still becomes `authentication.md`.

- [ ] **Fix 4 — correct `SKILL.md` stage 4's circular order** *(review finding A)*

Stage 4 tells the agent to stop if anything is unresolved, then to run `lycheeignore` "once
`check` is clean". But `check`'s clean predicate includes `rawFailures`, and a new raw-`main`
URL **always** 404s before merge — precisely what `lycheeignore` suppresses. An agent following
the text literally either aborts at "do not open a knowingly-red PR" or ships unsuppressed.

Rewrite stage 4 as an explicit ordered sequence: run `check` (expect raw-URL 404s for any
newly added asset — that is normal on the first pass, not a failure), then `lycheeignore` to
record the temporary suppressions, then `check` again, which must now be clean apart from the
documented pre-existing baseline. State plainly that a first-pass 404 on a brand-new asset is
expected and that only a 404 whose file is missing locally is a real problem — `lycheeignore`
already refuses that case with exit `2`.

- [ ] **Verify and commit**

Run `node --test ".claude/skills/docs-intake/test/*.test.mjs"` — expect 55 pass / 0 fail.
Confirm `git status --porcelain docs/ .lycheeignore` is empty. Commit all five files together.

---

## Still Deferred After This Addendum

- **`lib/refs.mjs` context-blindness** (angle-bracket link targets; refs inside comments, code
  spans, and fences). Deferred by the repo owner pending review of the Archbee previews — see
  the parent plan's "Known Gaps — Deferred by Decision".
- **The pre-existing `check` baseline** on the real repo: `docs/api/index.md` orphaned, and the
  `.lycheeignore` TODO block covering files referenced but never copied. Out of scope per spec
  §8. Note that Task 10's `suppressionsSatisfied` will now *report* when a drop satisfies one
  of those suppressions, which is the reporting half of spec §8; removing the line stays a
  human decision.
- **Minor items from the final review** not addressed here: `DEP0190` warning from
  `spawnSync(..., {shell:true})`; `check` listing duplicate refs per occurrence (26 lines, 13
  unique); `parseSummary`'s unread `indent` field; `intake.mjs` exporting `EXIT`/`parseArgs`
  that can never be imported because `main()` runs at top level; `rawUrlFor` being test-only.
