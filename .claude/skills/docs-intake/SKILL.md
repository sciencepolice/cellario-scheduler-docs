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

```bash
node scripts/docs-intake/intake.mjs wire-nav <dest>...
```

Pass every `NEW` page's destination path in one call. The command finds each page's group from
the most-specific directory prefix among existing `Summary.md` entries and appends
`- [Title](path)` to it. Exit `2` means a nav group is ambiguous — `api/api/` genuinely is,
because `examples-overview.md` sits there but is listed under **API Examples** — nothing is
written in that case, and the human chooses the group.

`UPDATE` pages need no nav change; don't pass them.

## Stage 4 — Validate (one write)

Run this exact sequence, in order. It is circular-looking only if you stop after step 1 — don't:

1. **`check` (first pass).**

   ```bash
   node scripts/docs-intake/intake.mjs check
   ```

   Compare against the repo's known baseline — `docs/api/index.md` is already an orphan, and
   the `.lycheeignore` TODO block suppresses links to files never copied. **New** problems are
   yours to fix; pre-existing ones are out of scope (see `internal/design-docs-intake-skill.md`
   §8).

   **Expect raw-URL 404s here for every brand-new asset.** A raw-`main` URL for a file this PR
   just added cannot resolve until the PR merges — that is normal on this first pass, not a
   failure. Do not stop for it and do not treat it as "check is clean."

   - Auto-fixable lint: re-run with `--fix`, then re-run `check`.
   - Any **other** unresolved problem (broken local link/embed, a real orphan, non-auto-fixable
     lint): stop. Do not open a knowingly-red PR.

2. **`lycheeignore`** — record the temporary suppressions for those expected 404s.

   ```bash
   node scripts/docs-intake/intake.mjs lycheeignore
   ```

   Exit `2` means a raw URL 404s with **no local file** backing it — the asset was never
   copied, and suppressing it would ship a broken image. Copy the asset first, or fix the ref;
   never suppress your way past this. List the lines it adds to delete after merge in the PR
   body.

3. **`check` (second pass).**

   ```bash
   node scripts/docs-intake/intake.mjs check
   ```

   This run must now be clean apart from the documented pre-existing baseline. If a raw-URL
   failure remains here, it is not the expected first-pass kind — treat it as a real problem
   and stop.

## Stage 5 — Ship

1. Empty `_inbox/` — delete everything under it **except `README.md`** — before committing.

```bash
git checkout -b docs/intake-YYYY-MM-DD   # substitute today's date
git add docs/ .lycheeignore
git commit -F <commit message file>
gh auth switch --hostname github.com --user sciencepolice
git push -u origin HEAD
gh pr create --base main --title "..." --body-file <body file>
gh auth switch --hostname github.com --user dstugan_hrbs
```

Pushing **requires** the `sciencepolice` account — the `dstugan_hrbs` EMU account can never
have access to this repo. Switch back to `dstugan_hrbs` afterward.

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
| An incoming image's destination already exists | Assets are keyed by basename; copying over it would silently overwrite a different page's live artwork. Rename the incoming file or confirm it's the same image — never overwrite. |

## Tests

```bash
node --test ".claude/skills/docs-intake/test/*.test.mjs"
```
