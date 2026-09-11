import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  parseSummary,
  groupForDirectory,
  appendEntry,
  orphans,
} from '../../../../scripts/docs-intake/lib/summary.mjs';

// Mirrors the real docs/Summary.md, trimmed: it keeps the api/api ambiguity
// (examples-overview.md lives in api/api but is listed under API Examples).
const SUMMARY = `# Table of contents

- [Introduction](Introduction.md)

## User Guide

- [Overview](user-guide/index.md)

## API Reference

- [Overview](api/api/overview.md)
- [Authentication](api/api/authentication.md)

## API Examples

- [Examples Overview](api/api/examples-overview.md)
- [Basic Operations](api/api/examples/basic-operations.md)

## Scripting

- [Overview](scripting/index.md)
- [Referencing Dependencies](scripting/advanced-topics/referencing-dependencies.md)

## Scripting API

- [API Overview](scripting/scripting-api/general/api-overview.md)
`;

test('parseSummary reads groups, entries and the pre-group preamble', () => {
  const parsed = parseSummary(SUMMARY);
  assert.deepEqual(parsed.groups.map((g) => g.title), [
    'User Guide', 'API Reference', 'API Examples', 'Scripting', 'Scripting API',
  ]);
  assert.deepEqual(parsed.preamble.map((e) => e.path), ['Introduction.md']);
  assert.equal(parsed.groups[0].entries.length, 1);
  assert.equal(parsed.groups[0].entries[0].path, 'user-guide/index.md');
});

test('groupForDirectory resolves by most-specific directory prefix', () => {
  const parsed = parseSummary(SUMMARY);
  assert.deepEqual(groupForDirectory(parsed, 'user-guide/getting-started.md'), {
    group: 'User Guide', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'api/api/examples/new-examples.md'), {
    group: 'API Examples', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'scripting/advanced-topics/new-topic.md'), {
    group: 'Scripting', ambiguous: false,
  });
  assert.deepEqual(groupForDirectory(parsed, 'scripting/scripting-api/general/new-api.md'), {
    group: 'Scripting API', ambiguous: false,
  });
});

test('groupForDirectory flags api/api as ambiguous, per the spec', () => {
  const parsed = parseSummary(SUMMARY);
  const got = groupForDirectory(parsed, 'api/api/new-page.md');
  assert.equal(got.ambiguous, true);
  assert.equal(got.group, null);
  assert.deepEqual(got.candidates.sort(), ['API Examples', 'API Reference']);
});

test('groupForDirectory flags an unknown directory as ambiguous', () => {
  const parsed = parseSummary(SUMMARY);
  const got = groupForDirectory(parsed, 'brand-new-section/page.md');
  assert.equal(got.ambiguous, true);
  assert.deepEqual(got.candidates, []);
});

test('appendEntry appends to the end of its group only', () => {
  const parsed = parseSummary(SUMMARY);
  const next = appendEntry(parsed, {
    group: 'User Guide',
    title: 'Getting Started',
    path: 'user-guide/getting-started.md',
  });
  const lines = next.split('\n');
  const at = lines.indexOf('- [Getting Started](user-guide/getting-started.md)');
  assert.ok(at > lines.indexOf('- [Overview](user-guide/index.md)'));
  assert.ok(at < lines.indexOf('## API Reference'));
  assert.equal(parseSummary(next).groups[0].entries.length, 2);
});

test('appendEntry rejects an unknown group', () => {
  const parsed = parseSummary(SUMMARY);
  assert.throws(
    () => appendEntry(parsed, { group: 'Nope', title: 'X', path: 'x.md' }),
    /No such group/,
  );
});

test('orphans reports unlisted pages and exempts structural and shadow docs', () => {
  const parsed = parseSummary(SUMMARY);
  const pages = [
    'Introduction.md',
    'Summary.md',
    'config.md',
    'user-guide/index.md',
    'api/index.md',
    'api/api/scripting-api-reference.md',
  ];
  const got = orphans(parsed, pages, {
    structural: ['Introduction.md', 'Summary.md', 'config.md'],
    shadowDocs: ['api/api/scripting-api-reference.md'],
  });
  assert.deepEqual(got, ['api/index.md']);
});
