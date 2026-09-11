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
