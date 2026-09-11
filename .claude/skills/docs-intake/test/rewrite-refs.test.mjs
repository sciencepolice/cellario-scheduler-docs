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
