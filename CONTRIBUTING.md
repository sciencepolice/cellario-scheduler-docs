# Contributing to cellario-scheduler-docs

This repo feeds the Cellario Scheduler developer site via Archbee. Content arrives by
**manual copy / PR** — you (or a maintainer) copy artifacts from the source repos into the
right section here and open a pull request. Merging to `main` publishes to Archbee.

## How Archbee builds the site (structure rules)

Archbee reconstructs the published site from a small set of config + structure files. These
rules are strict and fail *silently* (a mistake drops Archbee back to mirroring the raw
folder tree alphabetically), so treat them as load-bearing. This layout mirrors Archbee's
canonical example repo, [`github.com/dragosbulugean/slate`](https://github.com/dragosbulugean/slate).

**1. The config file must be `.archbee.yaml` — a dotfile at the repo root.**
A file named `archbee.yaml` (no leading dot) is *not read at all*: `root`, `structure`,
`shadowdocs`, etc. are silently ignored and the nav falls back to folder-mirror mode. Our
config:

```yaml
root: ./docs                 # relative, with ./ (a leading-slash /docs may not resolve)
structure:
  readme: Introduction.md    # the space HOME page
  summary: Summary.md        # the nav tree (see rule 3)
  assets: assets             # image folder
publishspace: true           # auto-publish on each sync
shadowdocs:                  # see rule 4
  - api/api/scripting-api-reference.md
```

**2. The `readme` + `summary` pair is required, and both are capitalized.**
`Introduction.md` (home page) and `Summary.md` (nav) live at the `docs/` root. These two
structural files are deliberate exceptions to the lowercase-kebab-case rule — match the
canonical example's capitalization exactly, and make sure the `.archbee.yaml` values match
the on-disk filenames (the sync runs on a case-sensitive filesystem).

**3. `Summary.md` must be a GitBook-style bulleted tree — not bare links.**

```markdown
# Table of contents

- [Introduction](Introduction.md)

## API Reference

- [Overview](api/api/overview.md)
- [Authentication](api/api/authentication.md)
  - [Sub-page](api/api/authentication/detail.md)   <!-- nest with 2-space indent -->
```

- `# Table of contents` — single H1 at the top.
- `## Group` — each heading becomes a top-level nav **category**, in file order.
- `- [Title](path)` — **list items** are pages; paths are relative to `docs/`.
- Indented list items (2 spaces) become **child pages** of the item above.
- **Bare links without the `-` bullet are not parsed** — that alone makes Archbee ignore
  the whole summary and mirror folders instead.

**Ordering & grouping come entirely from `Summary.md` order** — never alphabetical, never
the folder layout — *but only when rules 1–3 hold*. If the config isn't loaded or the
summary can't be parsed, Archbee mirrors the physical folders sorted A–Z. That fallback is
the #1 symptom of a broken structure file.

**4. Shadow docs** (`shadowdocs:` list, paths relative to `root`) are synced and indexed
for the Ask-AI assistant but **hidden from the published portal nav** — use for
AI-oriented references. Do not also list a shadow doc in `Summary.md`.

**5. There is no `ignore`/`exclude` key.** To keep a file in the repo but out of the site,
simply don't reference it (and don't let a shadowdocs folder glob sweep it in). List every
page you want published in `Summary.md`.

**6. Syncing.** Sync is one-way (GitHub → Archbee) on every push to `main`. Structure
changes take effect on the next sync; when the tree was previously built wrong, do a
**purge + re-sync** in Archbee to rebuild it from scratch.

## Workflow

1. Branch off `main`.
2. Drop your `.md` (and any `.cs`/`.py` samples, images) into the correct section under
   `docs/`:
   - `docs/user-guide/<child-folder>/`
   - `docs/api/<child-folder>/` — C# samples go in `docs/api/samples/`
   - `docs/scripting/<child-folder>/` — Python samples go in `docs/scripting/samples/`
3. **Add the page to [`docs/Summary.md`](docs/Summary.md)** so it shows up in the nav.
   A page that isn't linked there won't appear in the site's left navigation.
4. Open a PR. CI (`.github/workflows/validate.yml`) lints markdown and checks that links
   and image URLs resolve. Fix anything it flags. (Embedded `use{file=...}` paths are not
   auto-checked — double-check those by hand.)
5. Get review (see `.github/CODEOWNERS`) and merge. Archbee syncs on push to `main`.

### Automated intake

For a batch of files copied from a source repo, drop them into `_inbox/` mirroring the `docs/`
tree and ask Claude Code to run the intake. The `docs-intake` skill
(`.claude/skills/docs-intake/SKILL.md`) applies the structure rules above, wires new pages
into `Summary.md`, validates locally, and opens the PR. It stops and asks whenever a section
or nav group is ambiguous rather than guessing. Design notes:
`internal/design-docs-intake-skill.md`.

## Images (GitHub-hosted)

Images live in `docs/assets/images/<section>/` and are referenced by **raw GitHub URL** so
Archbee points back to GitHub for hosting:

```markdown
![Run dialog](https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/user-guide/run-dialog.png)
```

Notes:

- Raw URLs pin to the `main` branch — an image change goes live when it merges to `main`.
- Keep filenames lowercase-kebab-case. Put each image under its section's subfolder.
- The same raw-GitHub-URL pattern is used for **non-image downloadable assets** (e.g. the
  OpenAPI spec under `docs/assets/api/`). Gotcha: a raw-`main` URL 404s until the file is
  actually on `main`, so the CI link check fails on the introducing PR. Add a temporary
  entry to [`.lycheeignore`](.lycheeignore) for the new file and remove it after merge.

## Embedding `.cs` / `.py` source

Keep the source file in the section's `samples/` folder, then embed a range into a page
with Archbee's `use{file=...}` syntax (paths are relative to `docs/`):

```markdown
use{file="scripting/samples/demo.py#l1-l20" syntax="python"}
use{file="api/samples/JobClient.cs#l5-l40" syntax="csharp"}
```

Declarations can also be collected in [`docs/config.md`](docs/config.md). This keeps the
shown code in sync with the actual source file on every sync.

> To instead publish a whole file as its own page, ask a maintainer to enable `shadowdocs`
> in `.archbee.yaml` — deferred until there's demand for browsable source.

## Conventions

- One `index.md` per section as its landing page.
- File and folder names: lowercase-kebab-case — except the structural files `Introduction.md`
  and `Summary.md` at the `docs/` root (see the structure rules above).
- Don't edit content in Archbee — it's overwritten on the next sync. GitHub is the source
  of truth.
