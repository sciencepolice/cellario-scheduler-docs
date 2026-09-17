# Design: `docs-intake` Skill — Inbox to Archbee-Ready Pages

**Status:** Approved design — ready for implementation planning
**Owner:** Daniel Stugan
**Date:** 2026-09-11
**Scope:** A repo-local Claude Code skill plus a zero-dependency Node helper, both committed to `cellario-scheduler-docs`

> **Internal document.** Lives outside `docs/` deliberately — Archbee's sync root is `./docs`,
> so nothing here reaches the published customer site. Do not move it under `docs/`.

---

## 1. The Problem

Content arrives in this repo by manual copy from source repos. Getting it published correctly
means satisfying a set of structure rules that are strict and **fail silently** — a mistake
doesn't error, it drops Archbee back to mirroring folders alphabetically, or leaves a page
that exists on disk and simply never appears in the portal nav. `CONTRIBUTING.md` documents
the rules; nothing enforces them.

The work is the same every time: kebab-case the paths, place files in the right section,
rewrite image references to raw-GitHub URLs, preserve `use{file=...}` embeds, register every
new page in `Summary.md`, and keep CI's lint and link checks green. It is mechanical enough
to be tedious and subtle enough to get wrong — `docs/api/index.md` is already an orphan on
disk today, unreferenced by `Summary.md`.

**This skill automates the mechanical passes and stops at every genuine judgment call.**

---

## 2. Scope

**In scope.** Source-repo markdown dropped into a staging folder, mostly as updates to pages
that already exist in `docs/`, occasionally as new pages. The skill normalizes, places,
merges, wires the nav, validates locally, and opens a PR.

**Out of scope, deliberately.** Content authoring or rewriting; Archbee-side operations
(purge, re-sync, publish); and the two known repo landmines in section 8, which are flagged
to a human rather than silently fixed.

**Review model.** All review happens on the GitHub PR. The skill does not gate on interactive
approval of content; it gates only on ambiguity it cannot resolve (section 6).

---

## 3. Architecture

A single skill, executed sequentially. Deterministic work goes to a script; two judgment
calls stay with the model. Rejected alternatives are recorded in section 9.

| Path | Purpose |
| --- | --- |
| `.claude/skills/docs-intake/SKILL.md` | The workflow: preflight → plan → place → wire nav → validate → ship. Procedure only. |
| `scripts/docs-intake/intake.mjs` | Node 24, zero dependencies. Subcommands `plan`, `rewrite-refs`, `check`; global `--dry-run`. |
| `.claude/skills/docs-intake/test/fixtures/` | Fixture inbox used to exercise the script before a real drop (section 7). |
| `_inbox/README.md` | How to drop files: mirror the `docs/` tree. |
| `.gitignore` | **New file** — the repo has none today. Ignores `_inbox/*` so raw drops can never be committed. |
| `CONTRIBUTING.md` | Add a short "Automated intake" pointer under Workflow. |

### 3.1 Single source of truth for the rules

The structure rules stay in `CONTRIBUTING.md` ("How Archbee builds the site"). `SKILL.md`
**cites that section and does not restate it.** Two copies of silently-failing rules would
drift, and the drifted copy would be the one someone follows.

### 3.2 Division of labor

**The script owns everything deterministic:** enumerating the batch, kebab-casing each path
segment, computing destinations, detecting whether a `docs/` counterpart exists, rewriting
image references to
`https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/<section>/<name>`,
resolving relative links and `use{file=}` paths against the filesystem, running
`npx markdownlint-cli2` with the repo config, and `curl -I` on raw URLs.

**The model owns the two calls a script cannot make:** matching an inbox file to its `docs/`
counterpart when the filename is not an exact hit, and the merge in Stage 2 of the pipeline (section 5) — separating
incoming prose from local Archbee plumbing that must survive.

### 3.3 One writer per shared file

`Summary.md` and `.lycheeignore` are touched exactly once per run, at stage 3 and stage 4,
after all per-file work has completed. No per-file step may write either. This constraint is
what rules out the parallel-agent design (section 9).

### 3.4 Local tooling assumptions

Verified present: `node` v24, `npx`, `gh` 2.97. **Absent: `docker` and the `lychee` binary**,
so CI's real link checker cannot run locally. `check` implements a stand-in — on-disk
resolution plus `curl -I` on raw URLs — which covers the failure modes that actually occur
(new images, renamed pages) without claiming parity with CI.

---

## 4. Intake Contract

Files are dropped into `_inbox/`, **mirroring the `docs/` tree**. The drop path is the only
section signal; the skill does not infer section from content.

```text
_inbox/user-guide/getting-started.md   →  docs/user-guide/getting-started.md
_inbox/api/api/events.md               →  docs/api/api/events.md
_inbox/scripting/samples/demo.py       →  docs/scripting/samples/demo.py
```

`_inbox/*` is git-ignored, so a raw drop cannot be committed by accident. The skill empties
`_inbox/` as part of stage 5, making a completed run visibly idempotent.

A file at `_inbox/` root with no section folder is an error, not a guess (section 6).

---

## 5. Pipeline

### Stage 0 — Preflight

Verify the current branch is `main`, the working tree is clean, and `_inbox/` is non-empty;
`git pull` first. Any failure stops the run before a single file moves.

### Stage 1 — Plan

`intake.mjs plan` emits a manifest, one row per inbox file:

- source path and computed destination (each path segment kebab-cased)
- classification: `NEW` (no counterpart) or `UPDATE` (counterpart exists)
- referenced assets and referenced links

The model reviews the manifest for anomalies before anything is written.

### Stage 2 — Place, per file

**`NEW`** — copy to destination, run `rewrite-refs`, queue a `Summary.md` append for stage 3.

**`UPDATE`** — incoming prose becomes the new body. Carried over from the existing page:

1. the local-path → raw-GitHub-URL image mapping,
2. any `use{file=...}` embed blocks,
3. kebab-cased internal link targets.

Incoming content is authoritative for prose, so **deletions do land** — an upstream cut is a
legitimate update. But a truncated source file and a deliberate cut are indistinguishable in a
diff, so **any deletion over ~10 lines is called out in the PR body** for review.

**New images** — copied to `docs/assets/images/<section>/`, kebab-cased, referenced by raw URL.

### Stage 3 — Wire nav (single write)

Parse the existing `Summary.md` and map each destination directory to its nav group by
**most-specific directory prefix**:

| Directory prefix | Group |
| --- | --- |
| `user-guide/` | `## User Guide` |
| `api/api/examples/` | `## API Examples` |
| `api/client-sdk/` | `## Client SDK` |
| `scripting/scripting-api/` | `## Scripting API` |
| `scripting/` | `## Scripting` |
| `api/api/` | **ambiguous — stop and ask** (see below) |

New entries **append to the end of their group**. Ordering within a group is not inferred.

**Ambiguity stop:** if a directory's existing entries span more than one group, stop and ask.
`api/api/` genuinely does — `examples-overview.md` sits in that directory but is listed under
**API Examples** while its siblings are under **API Reference**. Guessing here would misfile
the page in the portal nav.

`UPDATE` files need no nav change; they are already listed.

### Stage 4 — Validate (single write to `.lycheeignore`)

`intake.mjs check` runs:

- `npx markdownlint-cli2` against `.markdownlint-cli2.yaml` (which already ignores
  `docs/Summary.md` and `docs/config.md`)
- on-disk resolution of every relative link and every `use{file=}` path — CI does **not**
  check `use{file=}`, per `CONTRIBUTING.md`, so this is strictly additional coverage
- `curl -I` on raw-GitHub URLs
- **orphan report:** pages present on disk but absent from `Summary.md` — the #1 silent
  failure mode in this repo

Each brand-new asset gets a **commented temporary `.lycheeignore` entry**, appended to the
existing TODO-style block, because a raw-`main` URL 404s until the PR merges. The entries to
delete after merge are listed in the PR body.

Mechanically-fixable lint is auto-fixed and re-checked. Anything else is reported and **blocks
the PR** rather than shipping a red build.

### Stage 5 — Ship

Branch `docs/intake-<YYYY-MM-DD>`, commit with a per-file summary, empty `_inbox/`, push, then
`gh pr create`. The PR body carries a table of every file with its classification and
destination, every judgment call made, every large deletion flagged in stage 2, and the
`.lycheeignore` lines to remove after merge.

If `gh` is unauthenticated, the skill commits and pushes, then prints the `gh pr create`
command to run.

---

## 6. Ambiguity Stops

Every one of these halts the run and asks. None is resolved by guessing.

| Condition | Why it stops |
| --- | --- |
| Inbox file at root, no section folder | Drop path is the only section signal. |
| Two plausible `docs/` counterparts for one file | Merging into the wrong page corrupts two pages at once. |
| Target directory's group is ambiguous | Misfiles the page in the portal nav (see `api/api/`). |
| Dirty tree or not on `main` | Mixes unrelated work into the intake PR. |
| Non-auto-fixable lint or an unresolvable link | Prevents a knowingly-red PR. |

---

## 7. Testing

Fixture-based, because the subject is transformation of real content.
`.claude/skills/docs-intake/test/fixtures/` holds a small inbox:

- one **`UPDATE`** against a real page carrying both a raw-GitHub image and a `use{file=}`
  embed — `scripting/base-tutorials/hello-world.md` is the intended target
- one **`NEW`** page in an unambiguous group

`--dry-run` prints the manifest, resulting diffs, and proposed `Summary.md` and
`.lycheeignore` edits **without writing anything**. It runs before the first real drop and
asserts the four things most likely to break:

1. kebab-casing of every path segment
2. image-reference rewriting to the raw-GitHub form
3. the `Summary.md` insertion point (correct group, appended at end)
4. the `.lycheeignore` entry for a new asset

---

## 8. Known Landmines — Flagged, Not Fixed

Both are real and both are out of scope for this skill.

**`archbee.json` contradicts `.archbee.yaml`.** It declares lowercase `summary.md`,
`publishSpace: false`, and an empty `shadowDocs`. Per the rule that cost us the nav once
already, a non-dotfile config **is not read**, so it is inert today — but it is a trap for the
next person who edits the "wrong" file and sees nothing happen. Deserves its own cleanup PR.

**The `.lycheeignore` TODO block** suppresses files referenced by content but never copied
(`operations-examples.md`, `deployment/https-configuration.md`, `Run_Data_Next.cs`,
`Log_Message.cs`, `Clean_Washer_With_Plate.cs`, and the `Cellario.Client` NuGet URL). A future
drop may satisfy some. `check` reports when an incoming file matches a suppression, but does
**not** remove the line — the call to drop a suppression belongs to a human.

---

## 9. Rejected Alternatives

**One subagent per file (parallel fan-out).** Faster on a large batch, and the shape originally
asked about. Rejected because every file wants to touch `Summary.md` and `.lycheeignore`;
concurrent writers on shared files clobber each other, and the nav wiring has to be serialized
afterward regardless — so the coordination cost buys nothing for the common case of a handful
of updated pages. Revisit only if batches routinely exceed ~15 files, and then for per-file
merge work only, with stages 3 and 4 still serialized.

**Pure script, no model judgment.** Fully deterministic and instant. Rejected because it cannot
perform the Stage 2 merge: deciding which hunks are prose updates and which lines are local
Archbee plumbing to preserve is not expressible as a regex.

**Straight overwrite of the counterpart, then transform.** Simpler and deterministic. Rejected
because any docs-side edit that is not a mechanical transform — a hand-fixed link, an added
note — would be silently lost, surfacing only if a reviewer happened to catch it.
