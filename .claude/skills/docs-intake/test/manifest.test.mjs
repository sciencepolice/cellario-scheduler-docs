import { test } from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import { mkdtemp, mkdir, writeFile, cp } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import {
  walk,
  titleFromMarkdown,
  plan,
} from '../../../../scripts/docs-intake/lib/manifest.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE = path.join(HERE, 'fixtures', 'repo');
const CLI = path.resolve(HERE, '../../../../scripts/docs-intake/intake.mjs');

test('walk returns posix-style relative paths for every non-dotfile', async () => {
  // walk() no longer skips README.md itself - that decision is depth-sensitive
  // (root only) and belongs to plan(), which knows depth. See SKIP_AT_ROOT.
  const found = await walk(path.join(FIXTURE, '_inbox'));
  assert.deepEqual(found.sort(), [
    'README.md',
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
