import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  isExternal,
  extractRefs,
  rewriteImageRefs,
  kebabLinkTargets,
  plumbingReport,
} from '../../../../scripts/docs-intake/lib/refs.mjs';

const RAW = 'https://raw.githubusercontent.com/sciencepolice/cellario-scheduler-docs/main/';

test('isExternal recognizes what must not be rewritten', () => {
  assert.equal(isExternal('https://example.com/a.png'), true);
  assert.equal(isExternal('mailto:a@b.com'), true);
  assert.equal(isExternal('#anchor'), true);
  assert.equal(isExternal('./images/a.png'), false);
});

test('extractRefs separates images, links and embeds', () => {
  const md = [
    '![Run dialog](./images/Run Dialog.png)',
    '[Authentication](Authentication.md)',
    '[NuGet](https://www.nuget.org/packages/Cellario.Client/)',
    'use{file="scripting/samples/demo.py#l1-l20" syntax="python"}',
  ].join('\n\n');
  const { images, links, embeds } = extractRefs(md);
  assert.equal(images.length, 1);
  assert.equal(images[0].target, './images/Run Dialog.png');
  assert.deepEqual(links.map((l) => l.target), ['Authentication.md', 'https://www.nuget.org/packages/Cellario.Client/']);
  assert.equal(embeds.length, 1);
  assert.equal(embeds[0].file, 'scripting/samples/demo.py#l1-l20');
  assert.equal(embeds[0].syntax, 'python');
});

test('rewriteImageRefs points local images at raw-main and lists the assets to copy', () => {
  const md = '![Run dialog](./images/Run Dialog.png)\n\n![Remote](https://example.com/x.png)';
  const { text, rewritten, assets } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.ok(text.includes(`![Run dialog](${RAW}docs/assets/images/user-guide/run-dialog.png)`));
  assert.ok(text.includes('![Remote](https://example.com/x.png)'), 'external images untouched');
  assert.equal(rewritten.length, 1);
  assert.deepEqual(assets, [
    { sourceBasename: 'Run Dialog.png', destRelPath: 'assets/images/user-guide/run-dialog.png' },
  ]);
});

test('rewriteImageRefs preserves a markdown title attribute', () => {
  const md = '![Alt](./a.png "A title")';
  const { text } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.ok(text.endsWith('"A title")'), text);
});

test('kebabLinkTargets never kebab-cases a link to a structural root file', () => {
  const md = [
    '[Home](../Introduction.md)',
    '[Config](./config.md)',
    '[Nav](Summary.md)',
    '[Auth](Authentication.md)',
  ].join('\n\n');
  const { text, rewritten } = kebabLinkTargets(md);
  assert.ok(text.includes('[Home](../Introduction.md)'), 'Introduction.md left verbatim');
  assert.ok(text.includes('[Config](./config.md)'), 'config.md left verbatim');
  assert.ok(text.includes('[Nav](Summary.md)'), 'Summary.md left verbatim');
  assert.ok(text.includes('[Auth](authentication.md)'), 'non-structural targets still kebab-cased');
  assert.equal(rewritten.length, 1);
});

test('kebabLinkTargets normalizes .md links only', () => {
  const md = [
    '[Auth](Authentication.md)',
    '[Sample](Run_Data_Next.cs)',
    '[Anchored](Some Page.md#a-section)',
    '[External](https://example.com/A Page.md)',
  ].join('\n\n');
  const { text, rewritten } = kebabLinkTargets(md);
  assert.ok(text.includes('[Auth](authentication.md)'));
  assert.ok(text.includes('[Sample](Run_Data_Next.cs)'), '.cs targets are verbatim');
  assert.ok(text.includes('[Anchored](some-page.md#a-section)'));
  assert.ok(text.includes('[External](https://example.com/A Page.md)'));
  assert.equal(rewritten.length, 2);
});

test('rewriteImageRefs flags an asset whose destination already exists', () => {
  const md = '![Overview](./images/Overview.png)\n\n![Fresh](./images/brand-new.png)';
  const existingAssets = new Set(['assets/images/user-guide/overview.png']);
  const { rewritten, assets, collisions } = rewriteImageRefs(md, {
    section: 'user-guide',
    existingAssets,
  });

  assert.deepEqual(collisions, [
    { sourceBasename: 'Overview.png', destRelPath: 'assets/images/user-guide/overview.png' },
  ]);
  assert.equal(assets.length, 1, 'only the non-colliding asset is queued for copying');
  assert.equal(assets[0].sourceBasename, 'brand-new.png');
  assert.equal(rewritten.length, 2, 'both URLs are still rewritten');
});

test('rewriteImageRefs without existingAssets behaves exactly as before', () => {
  const md = '![A](./images/a.png)';
  const { assets, collisions } = rewriteImageRefs(md, { section: 'user-guide' });
  assert.equal(assets.length, 1);
  assert.deepEqual(collisions, []);
});

test('plumbingReport carries raw image URLs over and reports dropped embeds', () => {
  const existing = [
    '# Hello World',
    `![Script editor](${RAW}docs/assets/images/scripting/script-editor.png)`,
    'use{file="scripting/samples/hello.py#l1-l12" syntax="python"}',
  ].join('\n\n');
  const incoming = [
    '# Hello World',
    'New opening paragraph.',
    '![Script editor](./images/Script_Editor.png)',
  ].join('\n\n');

  const { text, preservedImages, droppedEmbeds } = plumbingReport(incoming, existing);
  assert.ok(text.includes(`${RAW}docs/assets/images/scripting/script-editor.png`));
  assert.ok(text.includes('New opening paragraph.'), 'incoming prose survives');
  assert.equal(preservedImages.length, 1);
  assert.equal(droppedEmbeds.length, 1);
  assert.equal(droppedEmbeds[0].file, 'scripting/samples/hello.py#l1-l12');
});
