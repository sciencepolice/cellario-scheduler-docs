import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  kebabCase,
  toDestination,
  assetDest,
  rawUrlFor,
} from '../../../../scripts/docs-intake/lib/paths.mjs';

test('kebabCase normalizes the shapes content actually arrives in', () => {
  assert.equal(kebabCase('Title Case With Spaces'), 'title-case-with-spaces');
  assert.equal(kebabCase('Run_Data_Next.cs'), 'run-data-next.cs');
  assert.equal(kebabCase('GettingStarted.md'), 'getting-started.md');
  assert.equal(kebabCase('Already-Kebab.MD'), 'already-kebab.md');
  assert.equal(kebabCase('01-overview-and-quick-start.md'), '01-overview-and-quick-start.md');
  assert.equal(kebabCase('Weird  --  Name!.md'), 'weird-name.md');
});

test('toDestination kebab-cases every segment and reports the section', () => {
  assert.deepEqual(toDestination('user-guide/Getting Started.md'), {
    destRelPath: 'user-guide/getting-started.md',
    section: 'user-guide',
    structural: false,
  });
  assert.deepEqual(toDestination('API Reference/Sub Folder/A Page.md'), {
    destRelPath: 'api-reference/sub-folder/a-page.md',
    section: 'api-reference',
    structural: false,
  });
});

test('toDestination never renames a file under samples/', () => {
  const got = toDestination('scripting/samples/CSharp Scripts/demo/Run_Data_Next.cs');
  assert.equal(got.destRelPath, 'scripting/samples/csharp-scripts/demo/Run_Data_Next.cs');
  assert.equal(got.section, 'scripting');
});

test('toDestination handles capitalized Samples directories case-insensitively', () => {
  const got = toDestination('scripting/Samples/CSharp Scripts/demo/Run_Data_Next.cs');
  assert.equal(got.destRelPath, 'scripting/samples/csharp-scripts/demo/Run_Data_Next.cs');
  assert.equal(got.section, 'scripting');
});

test('toDestination stops on a root drop with no section folder', () => {
  const got = toDestination('notes.md');
  assert.match(got.stop, /no section folder/);
  assert.equal(got.destRelPath, undefined);
});

test('toDestination allows the structural root exceptions but not Summary.md', () => {
  assert.deepEqual(toDestination('Introduction.md'), {
    destRelPath: 'Introduction.md',
    section: null,
    structural: true,
  });
  assert.deepEqual(toDestination('config.md'), {
    destRelPath: 'config.md',
    section: null,
    structural: true,
  });
  assert.match(toDestination('Summary.md').stop, /managed by the skill/);
});

test('asset helpers build the raw-main URL form', () => {
  assert.equal(assetDest('user-guide', 'Run Dialog.PNG'), 'assets/images/user-guide/run-dialog.png');
  assert.equal(
    rawUrlFor('assets/images/user-guide/run-dialog.png'),
    'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/docs/assets/images/user-guide/run-dialog.png',
  );
});
