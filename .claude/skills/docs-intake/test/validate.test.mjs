import { test } from 'node:test';
import assert from 'node:assert/strict';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
import { mkdtemp, mkdir, writeFile, readFile, cp } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import {
  parseShadowDocs,
  parseLycheeIgnore,
  checkLocalRefs,
  collectRawUrls,
  headCheck,
  checkOrphans,
  proposeLycheeIgnore,
} from '../../../../scripts/docs-intake/lib/validate.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE = path.join(HERE, 'fixtures', 'repo');
const CLI = path.resolve(HERE, '../../../../scripts/docs-intake/intake.mjs');

test('parseShadowDocs reads the list without a YAML dependency', () => {
  const yaml = [
    'root: ./docs',
    'structure:',
    '  readme: Introduction.md',
    'shadowdocs:',
    '  - api/api/scripting-api-reference.md',
    '  - api/api/other.md',
    'publishspace: true',
  ].join('\n');
  assert.deepEqual(parseShadowDocs(yaml), [
    'api/api/scripting-api-reference.md',
    'api/api/other.md',
  ]);
});

test('parseLycheeIgnore skips comments and blanks', () => {
  const patterns = parseLycheeIgnore('# a comment\n\noperations-examples\\.md\n');
  assert.equal(patterns.length, 1);
  assert.ok(patterns[0].test('docs/x/operations-examples.md'));
});

test('checkLocalRefs finds a broken embed in the fixture repo', async () => {
  // The fixture hello-world.md embeds scripting/samples/hello.py, which does not exist.
  const broken = await checkLocalRefs(FIXTURE);
  assert.ok(
    broken.some((b) => b.kind === 'embed' && b.target.startsWith('scripting/samples/hello.py')),
    JSON.stringify(broken),
  );
});

test('collectRawUrls dedupes the raw-main URLs in docs/', async () => {
  const urls = await collectRawUrls(FIXTURE);
  assert.deepEqual(urls, [
    'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/scripting/script-editor.png',
  ]);
});

test('headCheck reports non-ok responses and never hits the network in tests', async () => {
  const seen = [];
  const fetchImpl = async (url) => {
    seen.push(url);
    return { ok: url.endsWith('good.png'), status: url.endsWith('good.png') ? 200 : 404 };
  };
  const failures = await headCheck(['https://x/good.png', 'https://x/bad.png'], {
    concurrency: 2,
    fetchImpl,
  });
  assert.equal(seen.length, 2);
  assert.deepEqual(failures, [{ url: 'https://x/bad.png', status: 404 }]);
});

test('checkOrphans reports nothing for the fully-wired fixture', async () => {
  assert.deepEqual(await checkOrphans(FIXTURE), []);
});

test('proposeLycheeIgnore appends an escaped, commented block', () => {
  const { text, added } = proposeLycheeIgnore('# existing\n', ['https://x/a.png'], {
    note: 'PR #12',
  });
  assert.ok(text.startsWith('# existing\n'));
  assert.ok(text.includes('TEMPORARY'));
  assert.ok(text.includes('PR #12'));
  assert.ok(text.includes('https://x/a\\.png'));
  assert.deepEqual(added, ['https://x/a.png']);
});

test('assetExistsLocally distinguishes a real asset from a dead reference', async () => {
  const { assetExistsLocally } = await import('../../../../scripts/docs-intake/lib/validate.mjs');
  const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';
  assert.equal(assetExistsLocally(FIXTURE, `${RAW}docs/Summary.md`), true);
  assert.equal(assetExistsLocally(FIXTURE, `${RAW}docs/assets/images/nope/missing.png`), false);
  assert.equal(assetExistsLocally(FIXTURE, 'https://example.com/x.png'), false);
});

test('suppressionsSatisfied reports a suppression whose file now exists', async () => {
  const { suppressionsSatisfied } = await import('../../../../scripts/docs-intake/lib/validate.mjs');
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-supp-'));
  await cp(FIXTURE, dir, { recursive: true });
  await writeFile(path.join(dir, '.lycheeignore'), '# c\nhello-world\\.md\nnever-landed\\.md\n', 'utf8');

  const satisfied = await suppressionsSatisfied(dir);
  const patterns = satisfied.map((s) => s.pattern);
  assert.ok(patterns.includes('hello-world\\.md'), JSON.stringify(satisfied));
  assert.ok(!patterns.includes('never-landed\\.md'), 'a still-missing file is not reported');
});

test('lycheeignore refuses to suppress a raw URL with no local file', async () => {
  const dir = await mkdtemp(path.join(tmpdir(), 'docs-intake-ign-'));
  await cp(FIXTURE, dir, { recursive: true });
  const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';
  await mkdir(path.join(dir, 'docs', 'user-guide'), { recursive: true });
  await writeFile(
    path.join(dir, 'docs', 'user-guide', 'ghost.md'),
    `# Ghost\n\n![Missing](${RAW}docs/assets/images/user-guide/ghost.png)\n`,
    'utf8',
  );
  await writeFile(path.join(dir, '.lycheeignore'), '# seed\n', 'utf8');
  const before = await readFile(path.join(dir, '.lycheeignore'), 'utf8');

  try {
    execFileSync(process.execPath, [CLI, 'lycheeignore', '--repo-root', dir], {
      encoding: 'utf8',
      stdio: 'pipe',
    });
    assert.fail('expected exit 2');
  } catch (err) {
    assert.equal(err.status, 2);
    assert.match(`${err.stdout ?? ''}${err.stderr ?? ''}`, /no local file/i);
  }

  assert.equal(await readFile(path.join(dir, '.lycheeignore'), 'utf8'), before, 'untouched');
});
