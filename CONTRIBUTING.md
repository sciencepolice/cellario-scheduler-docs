# Contributing to cellario-scheduler-docs

This repo feeds the Cellario Scheduler developer site via Archbee. Content arrives by
**manual copy / PR** — you (or a maintainer) copy artifacts from the source repos into the
right section here and open a pull request. Merging to `main` publishes to Archbee.

## Workflow

1. Branch off `main`.
2. Drop your `.md` (and any `.cs`/`.py` samples, images) into the correct section under
   `docs/`:
   - `docs/user-guide/<child-folder>/`
   - `docs/api/<child-folder>/` — C# samples go in `docs/api/samples/`
   - `docs/scripting/<child-folder>/` — Python samples go in `docs/scripting/samples/`
3. **Add the page to [`docs/summary.md`](docs/summary.md)** so it shows up in the nav.
   A page that isn't linked there won't appear in the site's left navigation.
4. Open a PR. CI (`.github/workflows/validate.yml`) lints markdown and checks that links
   and image URLs resolve. Fix anything it flags. (Embedded `use{file=...}` paths are not
   auto-checked — double-check those by hand.)
5. Get review (see `.github/CODEOWNERS`) and merge. Archbee syncs on push to `main`.

## Images (GitHub-hosted)

Images live in `docs/assets/images/<section>/` and are referenced by **raw GitHub URL** so
Archbee points back to GitHub for hosting:

```markdown
![Run dialog](https://raw.githubusercontent.com/HRB-SW/cellario-scheduler-docs/main/docs/assets/images/user-guide/run-dialog.png)
```

Notes:
- Raw URLs pin to the `main` branch — an image change goes live when it merges to `main`.
- Keep filenames lowercase-kebab-case. Put each image under its section's subfolder.

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
> in `archbee.yaml` — deferred until there's demand for browsable source.

## Conventions

- One `index.md` per section as its landing page.
- File and folder names: lowercase-kebab-case.
- Don't edit content in Archbee — it's overwritten on the next sync. GitHub is the source
  of truth.
