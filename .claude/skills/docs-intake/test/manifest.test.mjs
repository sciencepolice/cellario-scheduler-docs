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
