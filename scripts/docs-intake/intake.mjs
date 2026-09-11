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
import { readFile, writeFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { rewriteImageRefs, kebabLinkTargets, plumbingReport } from './lib/refs.mjs';
import { plan, sectionOfDest, titleFromMarkdown, walk } from './lib/manifest.mjs';
import { parseSummary, groupForDirectory, appendEntry } from './lib/summary.mjs';
import {
  runLint,
  checkLocalRefs,
  collectRawUrls,
  headCheck,
  checkOrphans,
  assetExistsLocally,
  suppressionsSatisfied,
  proposeLycheeIgnore,
} from './lib/validate.mjs';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const DEFAULT_REPO_ROOT = path.resolve(HERE, '..', '..');

export const EXIT = { OK: 0, VALIDATION: 1, AMBIGUOUS: 2, USAGE: 64 };

const USAGE = `Usage:
  intake.mjs plan [--json] [--repo-root <dir>]
  intake.mjs rewrite-refs <docs-relative-path> [--from-inbox <inbox-relative-path>] [--dry-run] [--repo-root <dir>]
  intake.mjs check [--fix] [--json] [--repo-root <dir>]
  intake.mjs wire-nav <docs-relative-path>... [--title <t>] [--dry-run] [--repo-root <dir>]
  intake.mjs lycheeignore [--dry-run] [--repo-root <dir>]
`;

export function parseArgs(argv) {
  const flags = {
    json: false,
    dryRun: false,
    fix: false,
    repoRoot: DEFAULT_REPO_ROOT,
    fromInbox: null,
    title: null,
  };
  const positional = [];
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--json') flags.json = true;
    else if (arg === '--dry-run') flags.dryRun = true;
    else if (arg === '--fix') flags.fix = true;
    else if (arg === '--repo-root') {
      i += 1;
      flags.repoRoot = path.resolve(argv[i]);
    } else if (arg === '--from-inbox') {
      i += 1;
      flags.fromInbox = argv[i];
    } else if (arg === '--title') {
      i += 1;
      flags.title = argv[i];
    } else if (arg.startsWith('-')) {
      throw new Error(`Unknown flag: ${arg}`);
    } else positional.push(arg);
  }
  return { command: positional[0], positional: positional.slice(1), flags };
}

function printPlan({ rows, stops }) {
  if (rows.length) {
    console.log('| Source | Destination | Kind | Class | Title |');
    console.log('| --- | --- | --- | --- | --- |');
    for (const r of rows) {
      console.log(`| ${r.source} | ${r.dest} | ${r.kind} | ${r.classification} | ${r.title ?? ''} |`);
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

  if (command === 'rewrite-refs') {
    const rel = parsed.positional[0];
    if (!rel) {
      console.error('rewrite-refs needs a docs-relative path.');
      console.error(USAGE);
      return EXIT.USAGE;
    }
    const section = sectionOfDest(rel);
    const abs = path.join(flags.repoRoot, 'docs', rel);
    const original = await readFile(abs, 'utf8');

    // With --from-inbox this is an UPDATE: incoming prose becomes the body, and the
    // plumbing the published page already carries is preserved onto it.
    let before = original;
    let droppedEmbeds = [];
    let preservedImages = [];
    if (flags.fromInbox) {
      const incoming = await readFile(path.join(flags.repoRoot, '_inbox', flags.fromInbox), 'utf8');
      const merged = plumbingReport(incoming, original);
      before = merged.text;
      droppedEmbeds = merged.droppedEmbeds;
      preservedImages = merged.preservedImages;
    }

    const assetsDir = path.join(flags.repoRoot, 'docs', 'assets');
    const existingAssets = new Set(
      (await walk(assetsDir)).map((p) => `assets/${p}`),
    );

    const images = section
      ? rewriteImageRefs(before, { section, existingAssets })
      : { text: before, rewritten: [], assets: [], collisions: [] };
    const links = kebabLinkTargets(images.text);

    if (!flags.dryRun && links.text !== original) await writeFile(abs, links.text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}${rel}`);
    for (const r of preservedImages) console.log(`  preserved image: ${r.from} -> ${r.to}`);
    for (const r of images.rewritten) console.log(`  image: ${r.from} -> ${r.to}`);
    for (const r of links.rewritten) console.log(`  link:  ${r.from} -> ${r.to}`);
    for (const a of images.assets) {
      console.log(`  copy asset: ${a.sourceBasename} -> docs/${a.destRelPath}`);
    }
    for (const c of images.collisions) {
      console.log(`  ASSET COLLISION - do NOT copy over the live file: ${c.sourceBasename} -> docs/${c.destRelPath}`);
      console.log('    rename the incoming file, or confirm it is the same image, before copying.');
    }
    for (const e of droppedEmbeds) {
      console.log(`  re-place embed (yours to position): ${e.file}`);
    }
    if (
      !preservedImages.length &&
      !images.rewritten.length &&
      !links.rewritten.length &&
      !droppedEmbeds.length
    ) {
      console.log('  no changes');
    }
    return EXIT.OK;
  }

  if (command === 'wire-nav') {
    const rels = parsed.positional;
    if (!rels.length) {
      console.error('wire-nav needs at least one docs-relative path.');
      console.error(USAGE);
      return EXIT.USAGE;
    }
    if (flags.title && rels.length > 1) {
      console.error('--title applies to a single page; pass pages one at a time to name them.');
      return EXIT.USAGE;
    }

    const summaryPath = path.join(flags.repoRoot, 'docs', 'Summary.md');
    let text = await readFile(summaryPath, 'utf8');

    // Resolve every page FIRST: an ambiguous group must abort the whole run before
    // any write, so Summary.md is never left half-wired.
    const planned = [];
    for (const rel of rels) {
      const parsedSummary = parseSummary(text);
      const listed = parsedSummary.groups.some((g) => g.entries.some((e) => e.path === rel))
        || parsedSummary.preamble.some((e) => e.path === rel);
      if (listed) {
        console.log(`  already listed, skipping: ${rel}`);
        continue;
      }

      const { group, ambiguous, candidates } = groupForDirectory(parsedSummary, rel);
      if (ambiguous) {
        const detail = candidates?.length
          ? `candidates: ${candidates.join(', ')}`
          : 'no group owns that directory';
        console.error(
          `ambiguous nav group for "${rel}" (${detail}) - a human must choose; nothing was written.`,
        );
        return EXIT.AMBIGUOUS;
      }

      const body = await readFile(path.join(flags.repoRoot, 'docs', rel), 'utf8');
      const title = flags.title ?? titleFromMarkdown(body, path.basename(rel, '.md'));
      planned.push({ rel, group, title });
      // Apply to the in-memory text so the next iteration sees it and line numbers stay valid.
      text = appendEntry(parseSummary(text), { group, title, path: rel });
    }

    if (!flags.dryRun && planned.length) await writeFile(summaryPath, text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}Summary.md`);
    for (const p of planned) console.log(`  ${p.group}: - [${p.title}](${p.rel})`);
    if (!planned.length) console.log('  no changes');
    return EXIT.OK;
  }

  if (command === 'lycheeignore') {
    const urls = await collectRawUrls(flags.repoRoot);
    const failures = await headCheck(urls);

    const suppressible = [];
    const unsafe = [];
    for (const f of failures) {
      (assetExistsLocally(flags.repoRoot, f.url) ? suppressible : unsafe).push(f.url);
    }

    if (unsafe.length) {
      console.error('Refusing to suppress: these raw URLs 404 and have no local file -');
      console.error('the asset was never copied, so suppressing would ship a broken image.');
      for (const u of unsafe) console.error(`  ${u}`);
      return EXIT.AMBIGUOUS;
    }

    const ignorePath = path.join(flags.repoRoot, '.lycheeignore');
    const before = existsSync(ignorePath) ? await readFile(ignorePath, 'utf8') : '';
    const { text, added } = proposeLycheeIgnore(before, suppressible, {
      note: 'this intake PR',
    });

    if (!flags.dryRun && added.length) await writeFile(ignorePath, text, 'utf8');

    console.log(`${flags.dryRun ? '[dry-run] ' : ''}.lycheeignore`);
    for (const u of added) console.log(`  temporary suppression: ${u}`);
    if (!added.length) console.log('  no new assets need suppressing');

    const satisfied = await suppressionsSatisfied(flags.repoRoot);
    if (satisfied.length) {
      console.log('\nExisting suppressions that now resolve (consider deleting the line):');
      for (const s of satisfied) console.log(`  ${s.pattern} -> ${s.nowResolves.join(', ')}`);
    }
    return EXIT.OK;
  }

  if (command === 'check') {
    const lint = runLint(flags.repoRoot, { fix: flags.fix });
    const brokenRefs = await checkLocalRefs(flags.repoRoot);
    const rawUrls = await collectRawUrls(flags.repoRoot);
    const rawFailures = await headCheck(rawUrls);
    const orphanPages = await checkOrphans(flags.repoRoot);
    const satisfied = await suppressionsSatisfied(flags.repoRoot);

    const result = {
      lint: { ok: lint.ok, output: lint.output },
      brokenRefs,
      rawUrlsChecked: rawUrls.length,
      rawFailures,
      orphans: orphanPages,
      satisfied,
    };

    if (flags.json) {
      console.log(JSON.stringify(result, null, 2));
    } else {
      console.log(`lint: ${lint.ok ? 'PASS' : 'FAIL'}`);
      if (!lint.ok) console.log(lint.output);
      console.log(`broken links/embeds: ${brokenRefs.length}`);
      for (const b of brokenRefs) console.log(`  ${b.page} [${b.kind}] -> ${b.target}`);
      console.log(`raw URLs checked: ${rawUrls.length}, failures: ${rawFailures.length}`);
      for (const f of rawFailures) console.log(`  ${f.status} ${f.url}`);
      console.log(`orphans (on disk, absent from Summary.md): ${orphanPages.length}`);
      for (const o of orphanPages) console.log(`  ${o}`);
      console.log(`suppressions that now resolve: ${satisfied.length}`);
      for (const s of satisfied) console.log(`  ${s.pattern} -> ${s.nowResolves.join(', ')}`);
    }

    const clean =
      lint.ok && !brokenRefs.length && !rawFailures.length && !orphanPages.length;
    return clean ? EXIT.OK : EXIT.VALIDATION;
  }

  console.error(command ? `Unknown command: ${command}` : 'No command given.');
  console.error(USAGE);
  return EXIT.USAGE;
}

try {
  process.exitCode = await main(process.argv.slice(2));
} catch (err) {
  // An I/O or programming error must not masquerade as EXIT.VALIDATION (1),
  // which the contract reserves for "the content failed validation".
  console.error(`docs-intake: ${err?.message ?? err}`);
  process.exitCode = EXIT.USAGE;
}
