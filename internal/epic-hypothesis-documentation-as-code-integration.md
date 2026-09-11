# Epic Hypothesis: Documentation As Code Integration for Cellario Scheduler

**Status:** Draft for discussion — not a commitment
**Owner:** Daniel Stugan
**Date:** 2026-08-07
**Vehicle:** `cellario-scheduler-docs` → Archbee-published Cellario Scheduler developer site

> **Internal document.** Lives outside `docs/` deliberately — Archbee's sync root is `./docs`,
> so nothing here reaches the published customer site. Do not move it under `docs/`.

> **Read this as a bet, not a plan.** Everything below is falsifiable on purpose. If the
> experiments come back negative, the right outcome is to kill or repoint this epic — not to
> build it anyway. Targets marked **[TBD-baseline]** are deliberately unset: we have no
> instrumented baseline today, and inventing numbers would make this doc worse than useless.

---

## Target Persona

**Lab automation engineer at a customer site.** Works at a pharma or biotech running one or
more Cellario systems. Owns the integration between Cellario and the surrounding
environment — LIMS, in-house sample-tracking tools, custom dashboards, event-driven
workflows. Writes C# and Python competently but is not a full-time software developer; this
is one of several hats. Has no access to HighRes internals, no source, and no Slack channel
with our engineers. Their alternative to reading docs is opening a support case or waiting
for a field engineer.

*(Proto-persona — assembled from the integration scenarios our own API docs name: LIMS
integration, automated workflows, system monitoring, custom applications. Not yet validated
by interviews.)*

---

## If/Then Hypothesis

**If we** publish the complete integration surface — REST API reference, the C# Client SDK,
and the C#/Python scripting API — as one searchable developer site with runnable, embedded
examples

**for** lab automation engineers at customer sites wiring Cellario into LIMS, in-house
tools, and custom workflows

**Then we will** enable them to build and ship a working first integration *without opening
a support case or consuming HighRes field-engineering time*

---

## Why We Believe This

Grounded observations, not opinion:

- **The integration surface is already broad and already documented.** REST API on `:8444`
  with a shipped OpenAPI spec, a NuGet-published .NET 8 Client SDK (`Cellario.Client`,
  NSwag-generated from the same spec, so it stays in sync), and a scripting API spanning
  protocols/orders, systems/inventory, messaging, and resource allocation. The raw material
  for self-service exists.
- **The content we've built is overwhelmingly developer-facing.** Of the pages in the repo
  today, essentially all substantive content is API, Client SDK, or scripting. The user
  guide is still a starter stub. *The site as actually built is already a developer-experience
  bet — this doc just makes that explicit and testable rather than accidental.*
- **The site ships with a free instrumentation channel.** Archbee's Ask-AI assistant logs
  every question asked against the corpus. That gives us a continuous, zero-build read on
  what people came looking for and didn't find — the closest thing to a discovery interview
  that runs itself.

---

## Assumptions We Are Making (all currently untested)

| # | Assumption | Risk if wrong |
|---|------------|---------------|
| A1 | Customer automation engineers *want* to self-serve, rather than preferring HighRes do the integration for them | The whole epic is pointed at a job nobody is hiring us for |
| A2 | The blocker is knowledge, not capability — the API/SDK can actually do what they need | We document our way into the same support cases |
| A3 | Integration questions are a material share of support load | We optimize a rounding error |
| A4 | The audience will find and reach the site | Great docs, no readers |
| A5 | Published content is accurate against the shipping 4.5 build | Docs actively mislead and *increase* support load |

**A1 and A3 are the load-bearing ones.** If either fails, this epic should not proceed.

---

## Tiny Acts of Discovery Experiments

Ordered. The first is mandatory — without it, nothing else is measurable.

1. **Establish the baseline (weeks 1–4).** Tag inbound support cases into
   integration / API / scripting vs. operational. Four weeks of tagging, no tooling build,
   just a tag taxonomy and discipline. *Tests A3 directly — and if integration cases turn
   out to be negligible, we stop here.*

2. **Mine Ask-AI (weeks 1–4, parallel).** Harvest every question asked of the site assistant
   plus zero-result searches. Rank by frequency. Output is a ranked gap list that costs us
   nothing to produce. *Tests A2 and A4.*

3. **First-integration task test (week 5).** Recruit 5 automation engineers — customer-side
   if we can get them, otherwise internal people who have never touched the API. Give them
   *only* the public site. Task: authenticate, submit an order, read back its status. Time
   it, record every stall point, offer no help. *The single highest-signal experiment; tests
   A2 and A5.*

4. **Field-engineer link-first counterfactual (weeks 5–8).** For the next tranche of inbound
   integration requests, field engineers respond first with a link to the relevant page
   instead of doing the work. Track how many resolve on the link alone vs. escalate.
   *Concierge test for "could the doc have replaced the human?" — tests A1 and A2.*

5. **Clean-room quick start (week 5, half a day).** On a fresh machine, follow the Client SDK
   quick start verbatim: NuGet reference → authenticate → first call. Does a stranger reach a
   successful authenticated call without prior knowledge? *Cheap smoke test of A5.*

Total cost: roughly one person's part-time attention for eight weeks. No engineering build.

---

## Validation Measures

**We know our hypothesis is valid if, within 8 weeks of the site going live**
(4 weeks establishing baseline + 4 weeks measured), **we observe:**

*Quantitative*

- **≥ 4 of 5** task-test participants complete submit-order-and-read-status using only the
  public site, with zero human assistance
- Integration-tagged support cases decline against the 4-week baseline — **target
  [TBD-baseline]**, set once experiment 1 reports, before the measured window opens
- **≥ 50%** of field-engineer link-first responses close without escalation

*Qualitative*

- The Ask-AI top-20 questions shift in character — from "how do I do this at all" toward
  narrower edge cases and specifics
- Task-test participants can say where they'd look next, unprompted, even when stuck

**Setting the support-case target after the baseline lands, not now, is deliberate.** A
number picked today would be theater.

---

## What Would Falsify This

Named in advance so we can't rationalize past them:

- **3 or more of 5 task-test participants fail or need help** → the gap is not documentation.
  Look at API ergonomics, auth, and error messages instead.
- **Support mix doesn't move, but Ask-AI shows people reached the right pages** → the blocker
  is product, not docs. This epic is pointed at the wrong thing; repoint at the API itself.
- **Nobody visits the site** → distribution and discoverability problem, not a content
  problem. Different epic entirely.
- **Integration cases are a trivial share of the baseline** → A3 is dead; the support-load
  half of the rationale collapses and this becomes a much smaller bet justified on adoption
  alone.

---

## Open Questions for the Group

1. Do we have *any* retrievable support history we could retro-tag, even roughly? It would
   collapse the four-week baseline wait to an afternoon.
2. Can we get 5 customer-side automation engineers for the task test, or do we settle for an
   internal proxy? Customer-side is meaningfully better signal and meaningfully harder to get.
3. Who owns the support-case tag taxonomy, and will it survive four weeks of discipline?
4. Is the docs content actually verified against the shipping 4.5 build — and if not, does A5
   need its own remediation pass before we measure anything?
5. Does the user guide stay a stub? This bet says the developer surface is where the value
   is; if the group disagrees, that's a different epic and should be argued now.

---

## If Validated

Decompose into user stories covering: the ranked Ask-AI gap list, the specific stall points
from the task test, end-to-end integration walkthroughs for the named scenarios (LIMS,
event-driven workflows, monitoring), and whatever the clean-room run breaks on.

**If invalidated:** say so out loud, record why, and repoint. An invalidated hypothesis that
saved a quarter of misdirected work is a good outcome, not a failure.
