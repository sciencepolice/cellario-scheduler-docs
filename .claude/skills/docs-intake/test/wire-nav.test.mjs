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
