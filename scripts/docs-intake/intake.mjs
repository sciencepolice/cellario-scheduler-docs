#!/usr/bin/env node
// docs-intake CLI. Three subcommands, all driven by .claude/skills/docs-intake/SKILL.md:
//
//   plan          stage 1: enumerate and classify the inbox (never writes)
//   rewrite-refs  stage 2: normalize refs in one placed page
//   check         stage 4: lint, resolve refs, HEAD raw URLs, report orphans
//
// Exit codes: 0 clean, 1 validation failure, 2 ambiguity requiring a human, 64 usage.

import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { plan } from './lib/manifest.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const DEFAULT_REPO_ROOT = path.resolve(HERE, '..', '..');

export const EXIT = { OK: 0, VALIDATION: 1, AMBIGUOUS: 2, USAGE: 64 };

const USAGE = `Usage:
  intake.mjs plan [--json] [--repo-root <dir>]
  intake.mjs rewrite-refs <docs-relative-path> [--dry-run] [--repo-root <dir>]
  intake.mjs check [--fix] [--json] [--repo-root <dir>]
`;

export function parseArgs(argv) {
  const flags = { json: false, dryRun: false, fix: false, repoRoot: DEFAULT_REPO_ROOT };
  const positional = [];
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--json') flags.json = true;
    else if (arg === '--dry-run') flags.dryRun = true;
    else if (arg === '--fix') flags.fix = true;
    else if (arg === '--repo-root') {
      i += 1;
      flags.repoRoot = path.resolve(argv[i]);
    } else if (arg.startsWith('-')) {
      throw new Error(`Unknown flag: ${arg}`);
    } else positional.push(arg);
  }
  return { command: positional[0], positional: positional.slice(1), flags };
}

function printPlan({ rows, stops }) {
  if (rows.length) {
    console.log('| Source | Destination | Class | Title |');
    console.log('| --- | --- | --- | --- |');
    for (const r of rows) {
      console.log(`| ${r.source} | ${r.dest} | ${r.classification} | ${r.title ?? ''} |`);
    }
  } else {
    console.log('_inbox/ is empty - nothing to plan.');
  }
  if (stops.length) {
    console.log('\nSTOPS (resolve before proceeding):');
    for (const s of stops) console.log(`  - ${s.source}: ${s.reason}`);
  }
}

async function main(argv) {
  let parsed;
  try {
    parsed = parseArgs(argv);
  } catch (err) {
    console.error(err.message);
    console.error(USAGE);
    return EXIT.USAGE;
  }
  const { command, flags } = parsed;

  if (command === 'plan') {
    const result = await plan({ repoRoot: flags.repoRoot });
    if (flags.json) console.log(JSON.stringify(result, null, 2));
    else printPlan(result);
    return result.stops.length ? EXIT.AMBIGUOUS : EXIT.OK;
  }

  console.error(command ? `Unknown command: ${command}` : 'No command given.');
  console.error(USAGE);
  return EXIT.USAGE;
}

process.exitCode = await main(process.argv.slice(2));
