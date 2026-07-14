# cellario-scheduler-docs

The single source [Archbee](https://www.archbee.com/) syncs from to build the **Cellario
Scheduler developer site**. This repo does **not** generate content — it *organizes* the
artifacts (`.md`, `.cs`, `.py`, images) produced by other repos into the site's structure
and holds the Archbee sync configuration.

## How it works

- Content lives under [`docs/`](docs/). [`docs/Summary.md`](docs/Summary.md) defines the
  left-nav tree.
- [`archbee.yaml`](archbee.yaml) tells Archbee where the docs root, nav file, and assets
  folder are.
- Sync is **one-way: GitHub → Archbee**. Every push to `main` triggers a sync; nobody
  edits in Archbee directly.
- The repo is **public**; images are GitHub-hosted and referenced by raw URL.

## Structure

Three top-level sections, each with child folders:

| Section    | Path               | Contents                                     |
| ---------- | ------------------ | -------------------------------------------- |
| User Guide | `docs/user-guide/` | End-user documentation                       |
| API        | `docs/api/`        | API reference + C# samples (`samples/`)      |
| Scripting  | `docs/scripting/`  | Scripting docs + Python samples (`samples/`) |

Images: `docs/assets/images/<section>/`.

## Adding content

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for the copy/PR workflow, image URL convention,
and how to embed `.cs`/`.py` source into a page.
