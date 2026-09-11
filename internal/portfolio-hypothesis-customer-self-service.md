# Portfolio Problem Statement & Hypothesis: Customer Self-Service

**Status:** Draft for discussion — not a commitment
**Owner:** Daniel Stugan
**Date:** 2026-08-17
**Scope:** Cellario OS, Cellario Scheduler, Cellario Solutions
**Purpose:** Strategy input — a direction to argue about, not an epic to schedule
**Two findings:** we've shipped things we never explained (sections 1–5), and the way we work keeps it
that way (section 2, "The fourth problem")

> **Internal document.** Lives outside `docs/` deliberately — Archbee's sync root is `./docs`, so nothing
> here reaches the published customer site. Do not move it under `docs/`.

> **Read this as a bet, not a plan.** Everything below is written so it can be proved wrong. Targets
> marked **[set later]** are deliberately blank: we've never measured any of this, and making up numbers
> would be worse than leaving them empty. Anything marked **[Unverified]** is something I believe but
> nobody has checked — those need someone to confirm or kill them before this document carries any weight.

> **Relationship to the Scheduler doc.** `epic-hypothesis-documentation-as-code-integration.md` is one
> example of what's described here — the Scheduler documentation, framed as a docs bet. This document
> widens the problem across all three products, and widens the possible causes beyond documentation. If
> the two ever disagree, this one wins and the Scheduler doc should be updated.

> **Repointed 2026-08-18 — this document now has a parent.**
> `problem-statement-documentation-ownership.md` reframes the problem one level up: **we have no plan for
> how we publish documentation, and nobody owns what customers experience when they go looking for it.**
> We publish through five channels — Confluence PDF exports, controlled PDFs by email, the Salesforce
> knowledge base, the developer site, and HTML help installed with the product — and each one grew up to
> serve a genuinely different audience. Nobody owns how they work together, so we've never said which
> audience each one is for, what belongs where, which one wins when two of them cover the same thing, or
> how a customer gets from one to another. That makes "completion" — the main measure in this document —
> undefined, because it measures one channel against itself rather than what a customer can actually
> reach. **Where the two documents disagree, the parent wins.** Its section 6 lists the disagreements; the
> parts of this document affected are §1, §2 (which gains an L0), and A7.
>
> *Two things the parent is careful not to say: it doesn't argue for merging the five channels into one,
> and it doesn't ask for more research. Its position is that everyone already knows the documentation is
> scattered, and what's actually missing is an owner.*

---

## 0. Summary and What This Document Asks For

**The fact.** Cellario OS, Scheduler, and Solutions all have REST APIs with OpenAPI specs behind them,
and all three ship a scripting engine — OS's and Solutions' were both forked from Scheduler's. All three
products can do roughly the same things. And the documentation for all three is incomplete in the same
way: Scheduler and OS are both half-built on Archbee, and nobody has confirmed what Solutions has.

**What follows from that.** This isn't a case of not having built the capability, and it isn't a case of
not wanting to document it. We built the capability. We built the toolchain that turns an OpenAPI spec
into a published developer site. We started documenting two products and finished neither. **That pattern
points at how we work rather than at what we can afford:** documentation sits outside the engineering
release cycle, so nothing ever stops when it's missing, and work that nothing forces reliably stalls just
short of done. Two products ending up half-finished independently isn't bad luck — it's exactly what
you'd expect from the way we've set things up.

**What this is not claiming.** That documentation is the answer. Two other things could be the real
problem: customers may not be able to use these interfaces without us even when we describe them, or they
may not be able to work out what went wrong when something breaks. The second matters more than it
looks — the scripting engines let customers change how the system schedules work, and we probably can't
tell a customer's broken script apart from a bug of our own. If that's true, helping more customers
self-serve *increases* our support load. Section 2 lays out all three; experiment 1 works out which one is
actually stopping people.

*And it isn't claiming the products should be more alike.* They're different products, with different
uses and different code. What we can reuse across all three is the toolchain and the process — not the
content.

**What this asks for — four decisions, roughly cheapest first:**

1. **Confirm or correct the table in section 5.** It's built from what I could verify in the repo plus
   what people have told me. If any of the specs is written by hand rather than generated from code, the
   cheap path this document depends on disappears.
2. **Make documentation part of the release process** — kept in version control, and shipped alongside the
   version of the software it describes. Today nothing stops when it's late, and work that nothing forces
   stops at around 60%, which is exactly where two products independently stopped. **This is the decision
   that makes the other three stick**, and it's structural — nobody below this group can make it.

   *It has two halves and they have to move together.* The process half is above. The other half is that
   **nobody is currently responsible** — it could be tech writing, engineering, a shared function, or
   something else, and this document doesn't try to pre-empt that call. But putting a check on the release
   without funding someone to clear it turns a documentation problem into a release problem, which is
   worse than what we have now. Section 2 lists the conditions that apply to whatever the group decides.
3. **Approve the measuring, not the building** (section 7). Ten weeks, roughly one person part-time, no
   engineering work — but it does need six weeks of consistent tagging across support, HRB-P-1, HRB-P-2,
   and HRB-P-3, and that's a management commitment rather than a product one. Without it, every number in
   section 8 stays hypothetical.
4. **Decide where we stand on the services trade-off** (A6). If HRB-P-1 and HRB-P-2 hours are billable,
   this direction trades revenue for reach. That isn't product's call, and it should be settled before we
   succeed at it rather than after.

**What would kill this.** If the labor census shows that work customers could have done themselves is
only a small slice of HRB-P-1, HRB-P-2, and HRB-P-3 hours, the business case collapses. Everything that
would prove this wrong is named up front in section 9.

---

## 1. Problem Statement

> **Revised 2026-08-18.** This section originally treated the problem as being only about knowledge and
> capability. Access now sits alongside them as an equal cause. See
> `problem-statement-documentation-ownership.md`.

**Technical staff at customer sites can't extend, integrate, configure, or diagnose our products on their
own — and where they could, they often can't find, reach, or trust the information that would let them.**
Nearly every non-trivial change they want to make goes through HighRes people: a support case, an
Applications Engineer (HRB-P-2), an Automation Engineer (HRB-P-1), an Automation Support Engineer
(HRB-P-3), or a services engagement.

**There are two causes here and they aren't the same problem.** Either the information doesn't exist —
the completion problem this document was written about — or it exists and is spread across five channels
that each grew up for a different audience, with nobody owning how they fit together, so the customer
can't work out which one is theirs or which copy matches the version they're running. **The second cause
is invisible in everything we currently measure**, because our measures count what we've written rather
than what a customer can reach. A topic can be fully documented and still be effectively missing at the
site.

*Why it matters to them:* their work waits on our response time instead of their own. A LIMS integration,
a protocol change, a new device, a system that's started misbehaving — each one becomes a request queued
against a vendor rather than a job they own. Their only alternative to understanding our system is waiting
for someone who does.

*Why it matters to us:* customers wait on HighRes people, which means the number of customers who can
succeed at the same time is limited by how many people we have. An HRB-P-1 hour spent on something a
customer could have done is an hour not spent deploying the next system; an HRB-P-2 hour spent the same
way is an hour not spent on protocol work only we can do. **[Unverified — this is the business claim
everything else rests on, and nobody has measured it.]**

*How it feels:* to the customer, like the system is a black box they rent access to — or worse, one whose
manual exists in five editions that disagree. To our engineers, like answering the same question in a
different accent, sometimes out of a different document than the colleague who answered it last week.

**The one-line version:** we sell systems whose APIs and scripting engines are only really usable by
people who work here — and we describe them in five places, none of which is the official one.

---

## 2. Three Questions — and the Process Behind Them

The Scheduler doc treats this as a documentation problem. Across all three products it isn't. There are
three separate reasons a customer ends up calling us instead of getting on with it, plus a fourth that
explains why the first three never get fixed. Keeping them apart matters, because they land on different
teams and only one of them is a writing job.

| | The question | What it looks like when the answer is no | Whose job the fix is |
|---|---|---|---|
| **L0** | **Can they find it, get to it, and tell whether it describes their version?** | Five channels, nothing saying which one wins, a different access rule for each — the one channel they can search doesn't say what version it covers, and the two that do can't be searched | Nobody's, and it spans several teams — see `problem-statement-documentation-ownership.md` |
| **L1** | **Is it written down?** | Nothing public describing it, or something public that's wrong | Docs / Product |
| **L2** | **Can they use it without us?** | Login nobody can figure out, no working example, breaking changes with no warning, names that mean something other than they appear to | Engineering |
| **L3** | **Can they tell what went wrong?** | A run fails and the only way to find out why is to call us | Engineering |

**L0 was added 2026-08-18 and it comes before everything else in the table.** L1–L3 all assume the
customer got to the document in the first place. If they can't find it, can't get past whatever login
guards it, or can't tell which version of the software it describes, how good it is never comes into play.
L0 is also the only row with nobody's name on it — the other three land on Docs/Product or Engineering,
while L0 spans Quality, Support, Engineering, and Product at once, which is why nothing in the way we work
has ever registered it as a problem. The parent document covers it properly.

Writing it down (L1) is the cheapest and most visible, which is exactly why it's the default answer. It's
also the only one of the three that can't fix a bad interface — good docs for an API nobody can figure out
or debug just produce better-informed support cases. **Experiment 1 exists to find out which of the three
is actually stopping people, product by product.**

And because the scripting engines let customers change *how the system schedules work*, L3 has to answer
something harder than "what broke": it has to answer **whose code broke it.**

Each product documents its own interfaces on its own terms — different products, different uses, different
code. **What we can reuse across all three is the toolchain and the process, not the content**, and that's
where the leverage is.

### The fourth problem: nobody's release is held up by missing docs

L1–L3 describe what the customer runs into. This describes why it stays that way — and it's about how we
work, not about what we've written.

**Nothing currently stops when documentation is late.** No release is held up, no check fails, no customer
commitment slips. Work that nothing forces gets to roughly 60% and stops when attention moves elsewhere.
**Two products independently landing at "half-baked on Archbee" isn't two coincidences — it's the
predictable result of an arrangement where late documentation holds nothing up.** Same outcome, two teams,
no coordination needed to produce it. That points at how we're organized rather than at what we spent.

**The proposal is narrow, and it says nothing about who does the work: keep documentation in version
control, and make shipping it part of the release — it goes out alongside the version it describes.**
Three things follow:

- **The release becomes the thing that forces it.** "Done" stops depending on goodwill and starts
  depending on a release checklist. That's the whole idea; everything else is detail.
- **Doc version matches product version.** This matters more in our market than in most. Customers run
  4.5 while we ship 4.7, and validated sites stay on old versions for years on purpose. A single
  always-current site quietly misdescribes most of what's actually installed out there. For GxP
  customers, version-matched docs are close to a requirement — the same customers A5 worries may not want
  self-service at all — which means this change pays for itself with them even if nothing else in this
  document holds up.
- **It extends something that already works instead of inventing something new.** The OpenAPI specs are
  generated from code, and the generated reference is the healthiest documentation we have precisely
  because it can't drift from what it describes. This puts the rest on similar footing.

**Nobody is responsible for this today, and that's the open decision.** It could sit with tech writing,
with engineering, with a shared function, or somewhere else. What the proposal does constrain is the
*shape* of whatever the group picks:

- Whoever owns it has to be **inside the release process**, not next to it. An owner who can be late
  without consequence recreates exactly what we have now under a new name.
- **What counts as "documented" has to be written down**, or the check can't be checked and will get
  skipped by default rather than by decision.
- Someone has to own **the written guides** specifically. Reference material can be generated; getting-
  started guides and integration walkthroughs can't, and they're the part a newcomer actually needs. This
  is the piece most likely to fall through whatever split gets chosen.

**Ways this fails:**

- **The check gets skipped when a release is under pressure**, once, and then always. The fix is to keep
  it small enough to survive: accuracy of whatever changed, plus a changelog entry — not polished prose. A
  check that holds up a release over writing quality will be gone within two quarters.
- **Three products, three interpretations of the standard.** One shared definition, or we've traded
  documentation drifting for the process drifting.
- **Versioned docs mean more to maintain.** Several live versions instead of one. **[Unverified: what
  Archbee actually supports for per-version documentation, and what it costs per release. This is the most
  likely place for the proposal to get expensive, and worth checking before it becomes a commitment.]**
- **Process without people is just a new way to fail.** If nobody is funded to do the work, adding a
  release check turns a documentation problem into a release problem. That's worse than where we are now,
  and it's the specific risk of adopting the process half of this while leaving the ownership half open
  indefinitely.

---

## 3. Who We're Talking About

From our existing persona library, not invented for this document. Two groups matter: the people who *use*
these interfaces, and the people who *check whether they exist* before we get the order.

**Who uses them**

| Persona | What they use | Why they're here |
|---|---|---|
| **COMPANY-P-12** — Advanced "DIY" Automation Engineer | Scripting, REST API, events | The clearest example, and the library already says so: their listed dislike is *"lack of open source information and APIs,"* and their listed like is *"identifying product shortcomings and solving problems on their own."* This document is largely about that one line. |
| **COMPANY-P-3** — Experienced Automation Engineer | Scripting, protocols, devices | High product knowledge, *"strong SW capabilities including running custom scripts and protocols."* Keeps the system running and handles errors during runs — so they hit this stuff under time pressure, not while exploring. |
| **SWP-P-2** — Protocol Author | Scripting engine, protocol design | Turns a scientific process into something the software can execute. Dislikes *"finding gaps in software that prevent translation of certain steps into the software model"* — which is the scripting case, already on record as a customer complaint. |
| **SWP-P-4** — API Consumer | REST API, events | Described in the library as the integration itself: pushes information in, pulls data out, for data capture, analysis, and inventory. |
| **SWP-P-3** — Integration Manager | REST API, events, SDK | Owns fitting us into everything around us. |
| **COMPANY-P-11** — Software Developer | REST API, SDK | Customer-side developer wiring Cellario to their ELN or LIMS. Knows .NET and Python; dislikes *"needing to involve third parties in software integrations"* — which, from their side of the table, means us. |

**Who checks whether these things exist**

| Persona | Why they matter here |
|---|---|
| **COMPANY-P-14** — Informatics Director | *"Highly interested in API and DB schemes,"* does CSV/GxP validation, prefers pre-validated products, and **sits in RfPs with the power to veto.** This is where A5 lives — and it means version-matched documentation helps us win deals, not just reduce support. |
| **COMPANY-P-13** — Software Architect | Judges our software and how well it integrates. Strong influence, not the decision maker. Scores us on whatever is public, and never files a support case. |
| **COMPANY-P-9** — IT Director | Login and SSO conformance, security questionnaires, network layering. Reads our docs specifically for how authentication works. |

**The scripting engine is the piece worth the group's attention**, and three personas share it —
COMPANY-P-12, COMPANY-P-3, and SWP-P-2. It's where a customer makes the system do something we never
shipped, and where a customer breaks scheduling in a way that lands in our queue looking like our bug.
For how much damage it can do, it's the least documented thing we have.

*The Python gap sits here.* Scheduler and OS support C# and Python; Solutions is C# only. COMPANY-P-11 and
COMPANY-P-10 both work in Python, so C#-only is a real barrier rather than a detail — the kind that
quietly hands the work back to HRB-P-2.

> **Something worth noticing about the persona library itself.** SWP-P-3 (Integration Manager) has nothing
> in it beyond a creation date, and SWP-P-4 (API Consumer) is a single sentence — and SWP-P-4 is written
> as an *integration* rather than as a person. **The two personas who own integration work are the
> thinnest entries we have.** It's the same pattern as the documentation: describing the developer
> audience has never been anybody's job, internally or externally. Worth fixing regardless of anything
> else here, and cheap to fix.

One more thing that feeds the completeness argument in section 5: **COMPANY-P-2** (Lab Technician /
Operator) lists among their likes *"user manuals that are up to date, comprehensive, and accurate."*
Complete documentation isn't only a developer concern, and that group is far larger than all six above put
together.

**Eight interfaces across three products (see section 5). None is fully documented; two products are
partway there.**

---

## 4. If/Then Hypothesis

**If we** treat each product's APIs and scripting engines as a deliverable in their own right — written
down to a standard we've defined, kept current by the release process, usable by someone who doesn't work
here, and debuggable when something breaks

**for** the technical people who already use them — COMPANY-P-12, COMPANY-P-3, SWP-P-2, SWP-P-3, SWP-P-4,
COMPANY-P-11 — and for the people who judge us on them before buying

**then we will** move integration, configuration, and first-line troubleshooting off HighRes people and
onto the customer — getting us out of the way of their work, and removing our headcount as the limit on
how many customers can succeed at the same time

---

## 5. Why We Believe This

### What each product actually exposes

| | REST API | OpenAPI spec | Event subscriber model | Scripting engine | Script languages | Docs on Archbee |
|---|---|---|---|---|---|---|
| **Scheduler** | ✅ (`:8444`) | ✅ | ✅ | ✅ *(the original)* | C#, Python | ⚠️ Partial — API/SDK/scripting built out, user guide a stub |
| **Cellario OS** | ✅ | ✅ | ✅ | ✅ *(forked from Scheduler)* | C#, Python | ⚠️ Partial |
| **Solutions** | ✅ | ✅ | ❓ not confirmed | ✅ *(forked from Scheduler)* | C# only | ❓ not confirmed |

**Three things come out of that table.**

**One: all three products can do roughly the same things.** This is one platform wearing three names, not
three products. Two narrow gaps exist — Solutions has no confirmed event model and no Python — and both
are specific enough to just decide rather than study.

**Two: the problem is finishing, not starting.** Scheduler and OS are *both* partly documented on Archbee.
That's a different problem from "one product is documented and the others aren't," and it points somewhere
less comfortable: **we've started this twice and finished it zero times.** So the constraint clearly isn't
willingness, budget, or tooling — we had all three, twice. What's missing is someone accountable for
*done*, which is decision 2 in section 0.

**Three: half-done may be the worst place to be.** Enough content to be found and to raise expectations;
not enough to rely on. Someone who gets let down by a documentation site once usually doesn't come back —
so partial coverage can burn the trust that complete coverage would have earned, while still generating
the support cases it was meant to prevent. **If that's right, "finish two products" beats "start a third,"
and the order of work in section 7 should reflect it.** This is a claim about how readers behave, not
something we've measured — experiment 5 is what would test it.

### What we can actually point to

- **Every product in scope already has what self-service needs.** The capability isn't the gap. Scheduler
  4.5 is the fully worked example: REST API on `:8444` with a shipped OpenAPI spec
  (`CellarioScheduler-4.5-API.json`), a .NET 8 client published on NuGet (`Cellario.Client`, generated by
  NSwag from that same spec, so the two can't drift apart), and a scripting API covering
  protocols/orders, systems/inventory, messaging, and resource allocation.
- **All three products use OpenAPI specs, which means the toolchain already exists.** Scheduler's chain —
  spec → generated client → generated reference → published site — isn't a Scheduler thing. It's
  something all three products can use, and it's been run once already. **Doing the second and third
  products costs a fraction of what the first one cost, and that cost is already spent.**

  Two products have Archbee documentation underway, so nobody decided against this — people decided *for*
  it, twice, and both stopped short. The same outcome in two teams independently is a property of how we
  work, not of either team. The answer is decision 2: put the work inside the release cycle so finishing
  is forced rather than hoped for.
- **The problem has the same shape on every product even though the content doesn't.** Each product
  exposes an API and a scripting engine to a technical customer who needs to understand it without our
  help. Three separate documentation jobs, one repeatable way of doing them.
- **What we chose to document says what we think matters.** In the docs repo, essentially everything
  substantial is API, Client SDK, or scripting; the user guide is still a starter stub. We've been betting
  on developers without ever saying so out loud. This document makes that bet explicit and testable.
- **We already have a live source of data about this, and it's free.** Archbee's Ask-AI assistant logs
  every question asked of the site — a running read on what people came looking for and didn't find, with
  nothing to build. *One caveat: against half-complete content the signal is muddy. A failed answer can't
  tell us whether we don't document something or document it badly, and an Ask-AI answering confidently
  from partial content is its own risk. Still the cheapest data available, and it costs nothing to start
  reading it.*
- **The gap is in explaining what we shipped, not in building more.** Not "build what we lack" but
  "finish explaining what we already ship." That's meaningfully cheaper than it looks from the outside,
  and it's why this document asks for measuring rather than for a program.
- **Customers writing their own scheduling logic is already supported, on all three products.** We've
  shipped the riskiest kind of extensibility — letting customers change how the system makes decisions —
  everywhere. Whether we've shipped the documentation, the error messages, and the ability to tell whose
  code failed is the open question. This is the one place where doing nothing isn't neutral: the
  capability is live either way.

---

## 6. Assumptions & Risks

| # | Assumption | Risk if wrong |
|---|---|---|
| A1 | Customers *want* to do this themselves rather than preferring HighRes do it | The whole direction is aimed at a job nobody wants us for |
| A2 | The problem is knowledge and access, not capability — the interfaces can actually do what customers need | We document our way into exactly the same support cases |
| A3 | Work customers could do themselves is a meaningful share of HRB-P-1, HRB-P-2, and HRB-P-3 hours | We optimize a rounding error |
| A4 | These interfaces are stable enough to document — describing them doesn't lock us into things we need to change | We end up stuck supporting interfaces we've outgrown |
| A5 | Self-service works with customers' validated/GxP change-control rules | We build for customers who aren't allowed to use it |
| A6 | Self-service doesn't eat into revenue we depend on (services, HRB-P-1 / HRB-P-2 time, support contracts) | We succeed at the experiment and lose money doing it |
| A7 | People find and can get to whatever we publish | Great interfaces, no users. **[Promoted 2026-08-18 — no longer treated as a minor assumption. `problem-statement-documentation-ownership.md` argues this is a primary cause and that it gates A3, A5, and A6, since something nobody can reach can't save a labor hour, win an evaluation, or replace a services engagement.]** |
| A8 | We can tell a customer's own scheduling code apart from our behavior when a run misbehaves | Every customer we enable becomes a source of support cases we can't sort out, and self-service *increases* support load |
| A9 | The toolchain and the process carry across products even though the content doesn't — spec-to-site tooling, the release check, and the definition of "documented" are reusable | Each product's docs are a one-off project, the cost never drops, and this becomes three programs rather than one thing done three times |
| A10 | The three OpenAPI specs are current and generated from shipping code, not written by hand | The toolchain copies an out-of-date description of each product, and we mass-produce wrong documentation |
| A11 | A mostly-complete set of docs is worth far more than two half-complete ones, because readers give up on a site that fails them once | We're funding a third half-built site, and "cover all three products" is the wrong goal |
| A12 | A documentation check on the release checklist will survive a release that's running late, whoever is responsible for clearing it | The process change is in name only: the check gets skipped once, then always, and we're back to unforced work with extra ceremony |
| A13 | Archbee can hold per-release documentation at an acceptable cost | Version-matched docs — the part GxP customers care most about — is the expensive half of the proposal, and the tooling decides it |

**A3, A5, A6 — and now A7 — are the ones everything else rests on.** A3 tells us how big this is. A5 tells
us whether it exists at all for our biggest customers. A6 tells us whether we want it. **A7 tells us
whether any of it can be collected**, and it was originally ranked below eleven other assumptions on the
strength of no evidence at all. If any of the four fails, that should stop this direction rather than
shrink it.

**A5 and A6 are new compared to the Scheduler doc, and they're why this had to be a strategy document.**
In a validated environment, a customer changing system behavior may trigger revalidation — so
self-service can be actively *unwanted*. And if HRB-P-1 or HRB-P-2 time is billable, self-service trades
services revenue for reach. Both are executive calls, not product ones.

**A4 and A8 are already piling up whether we act or not.** The scripting IDEs and REST APIs are shipped
and presumably in use, so we're already carrying the compatibility risk in A4 and the "whose code broke
it" problem in A8 — we just never decided to. That changes the question: it isn't "should we open these
up," it's "should we start supporting things we already opened." Doing nothing is a position, not
neutrality.

**A9 and A10 are the price of the good news.** Shared spec tooling is what makes this cheap, and it's
exactly what makes it possible to mass-produce a mistake — a generated site built from an out-of-date spec
produces confident, well-formatted, wrong documentation at scale, faster than a person could. A10 can be
checked in about a day (experiment 2) and shouldn't be assumed. A9 is the claim that the *way of doing it*
carries across products even though the *content* doesn't; if it turns out not to, this is three
programs rather than one thing done three times.

---

## 7. Cheap Ways to Find Out

In order. The first two are required — without them nothing else can be measured or scoped. None of them
needs any engineering work.

1. **Labor census (weeks 1–6).** Tag incoming support cases *and* Applications / Automation Engineering
   hours three ways: **who absorbed the work** (HRB-P-1 Automation Engineer, HRB-P-2 Applications
   Engineer, HRB-P-3 Automation Support Engineer, HRB-P-4 Field Service Engineer — they handle different
   things, and HRB-P-4's hours are the comparison group, since hardware service is never something a
   customer could do themselves), **which product** (OS / Scheduler / Solutions / hardware), and **could
   the customer have done it** — *given (a) perfect documentation, (b) a better interface, (c) never?* Six
   weeks of consistent tagging, no tooling needed. **Tests A3, and separates L1 from L2/L3 at the same
   time.** If (c) dominates, stop here.

2. **Spec readiness check (about a day).** Three questions engineering can answer without a study:

   - **Is each OpenAPI spec generated from shipping code, or written by hand?** *Tests A10. Any spec
     written by hand needs fixing before we publish anything from it — otherwise the toolchain
     mass-produces something out of date.*
   - **Does Solutions have an event subscriber model?** The one unanswered cell in the section 5 table.
   - **What would a documentation check have to check the scripting engines against?** REST has a spec;
     the scripting APIs may not, which would make checking them a much bigger job than checking the REST
     side (see open question 5).

   *Cheapest thing on the list and a prerequisite for the rest. Run it first — a bad answer about where
   the specs come from reorders everything below it.*

3. **Read what people ask Ask-AI (weeks 1–6, in parallel, free).** Collect every question asked of the
   site assistant plus every search that returned nothing, and rank by how often each comes up. *Tests A2
   and A7 — for Scheduler only, which limits what it can tell us.*

4. **Customer appetite interviews (weeks 2–6).** 8–10 conversations, split between validated/GxP sites and
   research sites. Ask what they currently wait on us for, what they'd rather own, and what their
   change-control rules actually allow. **Pick the right people rather than whoever answers the phone:**
   COMPANY-P-12 and COMPANY-P-3 for appetite, COMPANY-P-14 for the change-control answer. *Tests A1 and
   A5 — the two questions no internal data can answer.*

5. **First-task tests, one per product (weeks 6–8).** Five people per product, given only the public
   material for that product, no help, timed, with every point where they got stuck written down. Each
   product gets a task that suits *its own use case* rather than one forced into a common shape — the
   question is whether a capable stranger can get somewhere real, not whether the three products behave
   alike. **Recruit COMPANY-P-12 and COMPANY-P-3** (or the closest internal stand-in: someone who can
   really program and has never used that product).

   - **API task:** log in, submit a piece of work, read back its status.
   - **Scripting task:** open the IDE, change one scheduling decision, watch it take effect, and work out
     whether it worked.

   The scripting task is the important one, and the one we've never tested. *Tests A2 and A8.*

   **What matters in the comparison is how much documentation exists, not how similar the products are.**
   Scheduler and OS are both partly documented; if people finish more often where there's more
   documentation, L1 is the problem. If they get stuck in the same places regardless, the problem is L2 or
   L3 and writing more won't help. That reading holds up even though the products are genuinely
   different — it compares *why people got stuck* within each product, not the products against each
   other.

6. **Answer with a link first (weeks 6–10).** For the next batch of incoming requests, whoever receives it
   — usually HRB-P-2 or HRB-P-3 — replies first with a link to something that already exists instead of
   doing the work, and we track how often that closes it versus how often it escalates. Split the results
   by role, since the two field different kinds of question. **HRB-P-3 already lists "build self-service
   resources to reduce ticket volume" as one of their goals, so this runs with that role's own objectives
   rather than against them.** *This is the direct test of "could the document have replaced the person?"
   — and it tests A1 and A2 against what people actually do rather than what they say.*

7. **Try the release-linked process on one product, one release (next release cycle).** The process change
   in section 2 is a proposal and should be tested rather than mandated. Pick one product's next version:
   documentation goes in version control, a small documentation check goes on the release checklist, and
   the docs publish alongside the version. Whoever clears the check is whatever the group decides — this
   tests *how it works*, not a particular owner. Measure three things: did the check hold when the release
   got tight, what did clearing it actually cost, and did the published docs match the shipped build on
   day one?

   *Tests A12 and A13, which no amount of analysis can settle. **Run it on the product whose leadership is
   most willing rather than the one that needs it most** — a first attempt that fails on politics teaches
   nothing about the process. And prefer a real release under real pressure to a quiet one; a check that
   held during an easy release hasn't been tested.*

Total cost: roughly one person part-time for ten weeks, plus six weeks of consistent tagging across
support, HRB-P-1, HRB-P-2, and HRB-P-3. Still no engineering work. The two real costs are the tagging
discipline — a management commitment, not a product one — and recruiting up to 30 test sessions (3
products × 2 tasks × 5 people). **If recruiting is the thing that limits us, cut breadth before depth: run
both tasks on two products rather than one task on three.** Keeping the task the same across products is
where the signal comes from.

---

## 8. How We'd Know It's Working

**We'd call this valid if, within 10 weeks** (6 weeks establishing a baseline + 4 weeks measured), **we
see:**

*Numbers*

- The share of HighRes hours a customer could have handled (categories a + b from experiment 1) is at
  least **[set later]** of the census — the target gets set once experiment 1 reports, before the measured
  window opens
- **At least 4 of 5** test participants finish the API task on **Scheduler** — our most complete
  documentation — using only public material, with no help from a person. *This is the ceiling check: if
  our best-documented product can't clear it, then writing more isn't the answer anywhere, and finishing
  the other two wouldn't have helped.*
- Where people get stuck, it's because **something wasn't written down rather than because the interface
  itself defeated them**, in most recorded cases. *This is the real bet, and it works within each product
  rather than across them: if people get stuck on things a document could have fixed, L1 is the problem;
  if they get stuck on login, error messages, or not knowing what state they're in, it's L2 or L3 and
  writing more won't help.*
- **At least 3 of 5** participants finish the scripting task on Scheduler
- **At least half** of link-first replies close the request without escalating
- **At least 5 of 8** interviewed customers name specific work they'd rather own themselves, and confirm
  their change-control rules allow it

*Judgement calls*

- The top 20 questions asked of Ask-AI change in character — from "how do I do this at all" toward
  narrower edge cases
- Test participants can say where they'd look next, unprompted, even when they're stuck
- The census and the tests point at a *specific* problem per product, rather than "all three, everywhere."
  The latter would mean our tests weren't sharp enough to tell things apart, not that everything is broken

**Setting the labor-share target after the census rather than now is deliberate.** A number picked today
would be theater.

---

## 9. What Would Prove This Wrong

Named up front so we can't talk our way past them later:

- **The census shows most of the work could never have been done by a customer** (physical, calibration,
  hardware, site-specific) → this is a services-efficiency problem, not a self-service problem. Completely
  different strategy.
- **Customers in our biggest segment would rather we did the work** → self-service is a niche play. Size
  it that way and stop calling it strategy.
- **Change-control rules forbid customer-side changes at most sites** → A5 is dead. Self-service points
  toward *reading* (monitoring, getting data out, reporting) and away from *changing* (protocols,
  configuration).
- **Test participants fail because of what the interface can't do, not what they didn't know** → the
  problem is L2/L3, not L1. Writing more is the wrong answer, and it would have felt productive.
- **People finish just as often on every product regardless of how much is documented** → these
  interfaces are self-evident to capable engineers, self-service already works, and the real problem is
  somewhere else entirely — awareness, permission, or willingness, not enablement.
- **Nobody can tell a customer's script apart from our own behavior when a run fails** → A8 is dead and
  self-service *multiplies* support load. Fix that before enabling anyone else.
- **The toolchain turns out not to carry across** — each product's publishing setup ends up one-off → A9
  is dead. This is three programs rather than one thing done three times, the cost estimate here is
  badly wrong, and it should be re-scoped one product at a time on its own merits.
- **The specs turn out to be hand-written or out of date** → A10 is dead. The cheapness was an illusion;
  fixing spec generation becomes the actual first project, and publishing before that would mass-produce
  wrong documentation.
- **The documentation check gets skipped the first time a release gets tight** → A12 is dead and the
  process change is in name only. Don't just try harder — a checklist item can't force anything on its
  own, and the pressure has to come from somewhere with real teeth: a customer commitment, a regulatory
  requirement, or a funded team that owns the outcome.
- **People do about as well on a thinly documented product as a well documented one** → A11 is dead, and
  so is the finishing argument. Partial coverage is evidently good enough, "finish before starting" is
  wrong, and breadth beats depth.
- **Support volume doesn't change but Ask-AI shows people reached the right pages** → the problem is the
  product, not how we describe it. Point this at the interfaces themselves.
- **Nobody shows up at all** → this is a discoverability problem. Different epic.
- **Services revenue depends on the hours we'd be removing** → the trade-off is real, and this becomes a
  business-model decision above product's level. Escalate rather than proceed.

---

## 10. Open Questions and Decisions to Make

1. **Why are Drivers out of scope?** They were left out of this pass, but letting customers write their
   own device drivers is arguably the highest-value self-service thing we have — it's the one where
   customer capability directly expands what our systems can do. If leaving it out was deliberate, the
   reason should be written down. If it was an oversight, this document needs a fourth product.
2. **One developer site or three?** Three products on three release schedules argues for three, each
   versioned with its own product, which is what the process proposal implies. The counter-argument is a
   customer running two Cellario products who now has two sites to find and two ways of thinking to hold.
   **Per-product sites look right, with the shared parts being the toolchain and the front door rather
   than the content** — but this is worth deciding deliberately, because whichever way we drift will be
   hard to undo once versioned content exists.
3. **Who is responsible for documentation?** The open half of decision 2 — tech writing, engineering, a
   shared function, or something else. The process change doesn't work without an answer, and the answer
   determines what the release check can reasonably ask for.
4. **Does Solutions need the event subscriber model and Python?** The two gaps in the section 5 table.
   Both might be perfectly reasonable product decisions rather than gaps — but Python deserves a
   deliberate call rather than one made by default, since it's the language of the people most likely to
   script against it.
5. **Does the documentation check apply to the scripting engines, or only to REST?** REST has a generated
   spec to check against. The scripting APIs may not, which would make *producing a spec a tool can check
   against* a prerequisite for checking them at all — potentially the biggest hidden cost in the whole
   proposal, and one per product rather than once.
6. **Is there support history we could go back and tag?** That would turn the six-week census into an
   afternoon.
7. **Do we start marketing self-service before it actually works?** COMPANY-P-13 and COMPANY-P-14 judge us
   on whether an API exists at evaluation time, and all three products can truthfully say one does.
   There's a real temptation to sell this before it's usable, and it should be refused out loud rather
   than by omission.
8. **What's our policy on versions and backward compatibility for these interfaces?** A4 can't be answered
   without one, and since the interfaces are already shipped we're accumulating risk with no stated
   policy. Publishing version-matched docs makes that risk visible to customers — which is an argument for
   settling the policy first, not for publishing less.
9. **How do we support scheduling code a customer wrote?** The live question behind A8: what we owe a
   customer whose own script broke their run, and how a support engineer tells that apart from a bug of
   ours.
10. **Does the Scheduler user guide stay a stub?** Across all three products this is the COMPANY-P-2
   question — lab technicians and operators are a far larger group than the six developer personas, and
   they've told us plainly they want complete, accurate manuals. If they're the underserved majority, the
   developer-first bet may be aimed at the smaller prize. *The user guide is also the part the process fix
   helps least — written guides don't fall out of a code change, so it needs its own answer whoever ends
   up owning it.*

---

## If Validated

Work in the order the census and the tests point at, not in the order that's easiest to produce. Expect
the answer to differ per product — one may be an L1 problem while another is L2/L3 — and treating them all
as writing work would waste the census.

The likely shape, given what section 5 shows:

0. **Change the process first.** Documentation in version control, shipping alongside the version it
   describes, with a small check on the release checklist. Everything below decays back to half-built
   without this — which is the lesson of the two efforts that already stalled — and it needs someone named
   as responsible to be real.
1. **Finish before starting.** Get Scheduler and OS to a defined standard before opening up Solutions. Two
   half-built sites is where we are now and the thing to stop repeating; a third would make the pattern a
   policy. **Defining that standard — what "documented" means — is a prerequisite, not a detail, and it's
   what makes the release check checkable.**
2. **Then reuse the toolchain.** Spec → generated client → generated reference → published site, for
   Solutions. Built once, proven once, cheap to run again — the method carries even though the content
   doesn't.
3. **Document each product's scripting engine on its own terms.** Three separate jobs, not one shared
   guide — and the most expensive item here.
4. **Fix "whose code broke it"** for customer-written scheduling logic before encouraging more of it.
   Nobody will ask for this, and it decides whether self-service reduces support load or multiplies it.
5. **Decide Solutions' event model and Python support explicitly.** Real decisions, not drift.

A note on order: item 1 is the least glamorous work here and among the most important — finishing
something that's already 60% done shows up as no progress on a status report, which is exactly why it
keeps not happening. Item 0 is what stops that being a recurring conversation. **The way this whole
direction fails is that we stand up a new site for Solutions, declare victory, and never do 0, 1, or 4.**

**If it turns out to be wrong:** say so out loud, write down why, and point the effort somewhere else. A
direction proved wrong that saved a quarter of misdirected work is a good outcome, not a failure.
