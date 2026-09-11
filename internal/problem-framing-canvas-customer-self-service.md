# Problem Framing Canvas: Customer Self-Service

**Status:** Draft for discussion — companion to `portfolio-hypothesis-customer-self-service.md`
**Owner:** Daniel Stugan
**Date:** 2026-08-17
**Framework:** MITRE Problem Framing Canvas v3 (Look Inward → Look Outward → Reframe)
**Personas:** From `cellarioscheduler-personas.md`

> **Internal document.** Lives outside `docs/` deliberately — Archbee's sync root is `./docs`.

> **Read this before the hypothesis doc.** That one proposes a direction. This one asks whether the
> problem is framed correctly in the first place, including where we are the cause of it. The two
> disagree in one place, noted at the end.

> **Repointed 2026-08-18.** `problem-statement-documentation-ownership.md` is now the parent of both
> this canvas and the hypothesis document. It reframes the problem as **an ownership gap**: we have no
> documentation publishing strategy and nobody owns the customer documentation experience across our five
> channels. Those channels each serve a real and different audience — the parent explicitly does not argue
> for consolidating them — but nobody owns which audience each one is for, what belongs where, which is
> authoritative where they overlap, or how a customer moves between them. This canvas remains valid but is
> scoped to the *extension surfaces*; the parent is scoped to everything we publish. Where they disagree,
> the parent governs.

> **Filled in solo.** The framework warns that a solo canvas preserves the biases it exists to surface.
> Treat every answer as a claim to argue with, particularly Phase 1. **No customer has been interviewed
> for any of it.**

---

## Phase 1: Look Inward

### What is the problem? (Symptoms)

Technical staff at customer sites cannot independently integrate, extend, or diagnose our products.
Observable symptoms:

- Two documentation efforts — Scheduler and Cellario OS — both stalled roughly 60% built on Archbee.
- Scripting engines ship in all three products, let customers change scheduling behavior, and carry
  close to no public description of how to use them.
- Integration work routes through a support case, an Applications Engineer (HRB-P-2), an Automation
  Engineer (HRB-P-1), an Automation Support Engineer (HRB-P-3), or a services engagement by default.
- Our own persona library leaves the two integration personas (SWP-P-3, SWP-P-4) essentially undefined.

### Why hasn't it been solved?

**Because there is no clear owner.** The other candidate explanations either reduce to it or can't be
assessed yet.

- **Low priority** is not a separate cause — it's what having no owner produces. Nothing blocks when
  documentation is late, so it loses every prioritization conversation it enters, in every team, without
  anyone deciding against it.
- **Resources** aren't the constraint on their own. Two teams found enough to start; neither had anyone
  accountable for finishing.
- **It isn't new.** The gap has been visible and unaddressed for a long time.
- **It isn't hard, or at least difficulty isn't what stopped it.** Nobody has ever been responsible for
  the work, so it never got far enough for difficulty to matter.

**One thing we can't yet size:** the REST path is cheap and we know it, because it's been run end to end
once. The scripting surfaces are unknown — there's no machine-checkable description to generate from, and
no one has owned documenting them long enough to find out what it costs (open question 5 in the hypothesis
doc).

**The cause is organizational; the cost is partly unknown.** Assigning an owner is what makes the cost
knowable.

### How are we part of the problem?

- **Solution-first thinking.** The Scheduler documentation site was built before anyone measured
  whether documentation was the blocker. The hypothesis document is a hypothesis retrofitted
  onto work already underway. That doesn't make it wrong, but the sequence was backwards and the
  measurement plan in section 7 of that doc is partly an attempt to catch up.
- **We've never described this audience, so we can't claim to know it.** The personas who own the
  integration surface are the thinnest entries in our own library. We have proto-personas standing in
  for the people we're proposing to serve, and zero interviews.
- **Our delivery model assumes we do the work.** "The HighRes way" is to integrate on the customer's
  behalf. Documentation that lets a customer bypass us runs against the grain of how HRB-P-1 and HRB-P-2
  are organized and utilized. HRB-P-7's stated challenge — *"balancing sales promises vs. delivery
  reality"* — and HRB-P-1's goal of a handover where *"customer team is trained and confident"* both
  assume we get the customer to competence, not that they arrive with it. We should expect organizational resistance that presents itself as
  prioritization rather than as disagreement.
- **We count "started" as "shipped."** Two half-built sites read as two documentation projects on a
  status report. The completion problem is partly a reporting artifact — nothing in how we track work
  distinguishes 60% from done.
- **We grew five publishing channels and never owned the system they make.** *Added 2026-08-18.*
  Confluence PDF exports, doc-controlled PDFs emailed at handover, the Salesforce knowledge base, the
  developer site, and HTML help installed with the product. **Four of the five serve a real and distinct
  audience that no other channel could serve as well** — a quality unit needs an auditable artifact, a lab
  workstation may have no internet, evaluators need something public and indexed. Having five is not the
  error. The error is that nobody owns how they work together: which audience each is for, what belongs
  where, which wins where they overlap, and how a customer moves between them are all undefined. So “is it
  documented?” can always be answered yes by pointing at whichever channel happens to have it. **This
  belongs in Phase 1 rather than Phase 2 — it is our arrangement, not their circumstance — and it is the
  same missing owner this canvas already identified for completeness, one level up.**
- **Survivorship bias in every signal we have.** We hear from customers who called us. A customer who
  tried to self-serve, failed, gave up, and quietly went back to filing tickets never appears in our
  data at all. Ask-AI is the only channel that even glimpses them, and against a half-complete corpus
  its signal can't separate "undocumented" from "documented badly."

---

## Phase 2: Look Outward

### Who experiences the problem?

**Who — the people using the surfaces**

| Persona | When and where they hit it |
|---|---|
| **COMPANY-P-12** — Advanced "DIY" Automation Engineer | Continuously, by choice. Their listed dislike is literally *"lack of open source information and APIs."* |
| **COMPANY-P-3** — Experienced Automation Engineer | Under pressure — during a run, recovering an error, on the clock. |
| **SWP-P-2** — Protocol Author | When a scientific step won't translate into the software model, which their persona already names as a top frustration. |
| **SWP-P-4 / SWP-P-3** — API Consumer, Integration Manager | At integration time, wiring us to LIMS/ELN and pulling data out. |
| **COMPANY-P-11** — Software Developer | Same, from the customer's software organization. Dislikes *"needing to involve third parties in software integrations"* — us. |

**Who — the people evaluating whether the surface exists**

| Persona | What they do about it |
|---|---|
| **COMPANY-P-14** — Informatics Director | *"Highly interested in API and DB schemes,"* runs CSV/GxP validation, and **holds veto rights in RfPs.** |
| **COMPANY-P-13** — Software Architect | Scores integration potential during evaluation. Never files a support case. |
| **COMPANY-P-9** — IT Director | Reads for the auth and SSO model, security questionnaire in hand. |

**Consequences.** Their work queues behind our response time rather than their own capability. For
COMPANY-P-3 specifically, whose stated primary responsibility is keeping the system running, a stall
isn't an inconvenience — it's a direct failure of their job, in front of the scientists waiting on the
instrument. For COMPANY-P-14, an interface they can't validate is grounds to veto a purchase.

### Who else has this problem, and how do they deal with it?

Any instrument or hardware vendor with a software layer, and acutely so in regulated automation. Two
visible strategies in the market: publish a complete developer surface and position as an open
platform, or keep it closed and monetize the integration as services. **Both are coherent
strategies. We are currently executing neither** — the surface is open but undescribed, which captures
the downside of both (customers can reach it and fail; we don't get paid for the help they then need).

### Who doesn't have this problem?

- **Sites where COMPANY-P-12 already reverse-engineered their way in.** They solved it themselves, at
  their own cost, and now depend on undocumented behavior. They don't have our problem; they've given
  us a different one — an installed base coupled to internals we never committed to.
- **Customers who bought a competitor's open platform.** They don't have the problem, and we don't have
  them.
- **Small sites with no technical staff at all.** They were never going to self-serve. This bounds the
  addressable population — it is not a whole-market problem.

### Who's been left out of the conversation?

- **SWP-P-3 and SWP-P-4** — the two personas who own the integration surface, undefined in our own
  library. We are proposing to serve people we haven't bothered to write down.
- **COMPANY-P-2 (Lab Technician / Operator)** — a much larger population than every developer persona
  combined, whose stated likes include *"user manuals that are up to date, comprehensive, and
  accurate,"* and whose user guide is a stub. Left out of the developer-first framing entirely.
- **Customers running older versions.** Documentation describes what we ship; much of the installed
  base runs something else. In validated environments that's deliberate and long-lived.
- **The silent failures.** Everyone who tried, failed, and never told us.

### Who benefits when the problem exists?

The question the hypothesis document only half-asks, and the one most likely to determine what happens
after the meeting.

- **Services, HRB-P-1, and HRB-P-2.** If those hours are billable, the status quo is revenue. If
  they're a utilization target, the status quo is utilization.
- **Support (HRB-P-3) — though only partly.** A known, staffed workflow, and self-service replaces
  predictable ticket volume with unpredictable escalations from customers who got halfway. Note this role
  is the one internal exception: it already lists *"build self-service resources to reduce ticket volume"*
  among its goals, so the incentive here runs both ways.
- **Product teams shipping releases.** No documentation gate means nothing new can block a release.
- **Everyone who would otherwise have to define the bar.** As long as "documented" is undefined, no one
  is accountable for missing it.
- **COMPANY-P-12 and COMPANY-P-3 themselves — the non-obvious one.** Their own persona descriptions say
  *"their DIY capability is what they perceive as the value they offer to their organization."* For some
  of them, difficulty is the moat. Making the surface easy could reduce what makes them valuable
  internally, and we should expect a subset of our most technical users to be ambivalent rather than
  grateful. **This cuts directly against assumption A1 in the hypothesis document**, which assumes
  customers want to self-serve. The persona file has quietly disagreed with A1 since 2023.

### Who loses if the problem is solved?

The services revenue line, if it depends on those hours. The "we integrate it for you" differentiator,
which is a real part of how we sell against larger competitors — and HRB-P-6 already lists *"competing
against DIY/internal solutions"* as a challenge, which is the same dynamic seen from the commercial side.
Also HRB-P-1's and HRB-P-2's role definitions: hours freed from integration only become new deployments if
there are new deployments to staff.

---

## Phase 3: Reframe

### Stated another way, the problem is

**We have shipped extension surfaces on three products — REST APIs, event models, and scripting engines
that let customers change scheduling behavior — without ever making it anyone's job to explain them, and
without anything in our release process that stops when the explanation is missing.** Technical staff at
customer sites therefore route work through HighRes labor that they are perfectly capable of doing
themselves, which puts our headcount on the critical path of their productivity and caps how many
customers can succeed at once. This persists because nobody is accountable for it and no release has ever
been blocked by it — not because anyone judged the work too hard or too expensive, since nobody has owned
it long enough to judge. And it's gone unexamined because our delivery model, our reporting, and every
signal we collect all quietly assume the customer was going to call us anyway.

**Added 2026-08-18 — and one more sentence belongs in the restatement above.** Even where we *have*
explained a surface, the explanation is published through five channels with no stated authority and a
different access rule each, so a customer under time pressure cannot establish which copy to believe —
which makes calling us the rational move even when the documentation is complete. That is a separate
failure from the one described above, it is not fixed by finishing anything, and it is the subject of
`problem-statement-documentation-ownership.md`.

### How Might We

**How might we make each product's extension surface usable by a competent person who doesn't work
here — as we aim to take HighRes labor off the critical path of customer productivity?**

*Kept broad on purpose.* It does not say "documentation," because documentation is one of three
possible blockers and the narrower framing would foreclose the other two.

---

## Formal Problem Statement

### Primary — COMPANY-P-12, Advanced "DIY" Automation Engineer

**I am** an automation engineer with real programming ability, running Cellario systems at a pharma or
biotech site.

- I prefer to build and fix things myself; solving problems independently is the value I offer my
  organization
- I am responsible for keeping the integrated system running and for recovering it when it errors
- I have no access to HighRes source, internals, or engineers
- What I dislike most about vendors, in my own words, is *lack of open source information and APIs*

**Trying to** wire Cellario into our LIMS and change how the system schedules work, so my lab gets the
throughput and data flow it needs without waiting on anyone.

**But**

- The APIs and scripting engines exist and are largely undescribed, so I'm reverse-engineering a system
  I bought
- When my own script changes scheduling and a run fails, I can't tell whether I broke it or the product
  did
- What documentation exists is partial, so I can't tell the difference between "not supported" and "not
  written down"
- On the Solutions product I can only script in C#, not Python

**Because** publishing and maintaining a usable description of these surfaces has never been assigned to
anyone, and nothing in HighRes's release process stops when it's missing.

**Which makes me feel** capable but blocked — and increasingly like the product is a black box I rent
access to, when the whole reason I'm valuable here is that I don't need one.

### Secondary — COMPANY-P-14, Informatics Director

**I am** responsible for scientific software and data flow across a therapeutic area, with veto rights in
vendor selection and an obligation to validate anything that touches regulated work.

**Trying to** confirm that Cellario's interfaces are documented, versioned, and validatable before I let
this purchase proceed — and to keep them validatable afterward, on the version we actually run.

**But** the documentation doesn't correspond to a specific product version, and the surfaces I care most
about are the least described.

**Because** documentation isn't tied to the release it describes.

**Which makes me feel** that this vendor isn't yet safe for a regulated environment — which for me is a
procurement conclusion, not a support complaint.

---

## Context & Constraints

- **Three products, three release trains** — Cellario OS, Scheduler, Solutions. Different products,
  different use cases, different code; convergence is not a goal.
- **Regulated environments** run old versions by choice and require revalidation for behavioral
  change. Self-service may be unwanted in exactly the segment with the most buying power.
- **Mixed technical population** — from COMPANY-P-12 to COMPANY-P-2, sharing the same product and the
  same documentation.
- **No baseline.** Nothing measured on support mix, HRB-P-1 / HRB-P-2 hours, or site traffic.
- **No interviews.** Every persona claim here comes from the library or from inference.

---

## Where This Canvas Disagrees With the Hypothesis Document

Worth surfacing rather than smoothing over:

1. **A1 ("customers want to self-serve") is weaker than the hypothesis doc treats it.** Both DIY personas
   describe their self-sufficiency as their personal value proposition. A subset may prefer the surface
   stay hard. The appetite interviews should be designed to detect ambivalence, not just to confirm
   enthusiasm.
2. **"Who benefits from the status quo" deserves to be a decision, not a risk row.** In the hypothesis
   document this is A6, one line in a table. This canvas suggests it's the primary determinant of whether
   anything happens — several groups benefit from the current arrangement, and none of them are opposed
   in bad faith.
3. **The developer-first framing may be aimed at the smaller population.** COMPANY-P-2 outnumbers every
   technical persona and is asking, on the record, for complete and accurate manuals.

---

## Next Steps

1. **Argue with Phase 1 as a group.** It's the part a solo canvas gets most wrong, and the part that
   determines whether the rest holds up.
2. **Answer "who benefits from the status quo" with actual numbers** — is HRB-P-1 or HRB-P-2 time
   billable, and how much? This gates the direction more than any customer research does.
3. **Define SWP-P-3 and SWP-P-4 in the persona library.** Cheap, and we can't credibly plan for people we
   haven't described.
4. **Ask HRB-P-5 which capabilities customers bought and never used.** Adoption gaps are visible to
   Customer Success without waiting six weeks for a census, and HRB-P-3's ticket history is the other
   cheap internal source. Both list version sprawl and adoption as live concerns already.
5. **Design the appetite interviews to falsify A1**, not to confirm it.
6. **Settle the ownership question in `problem-statement-documentation-ownership.md`** before scoping any
   documentation work here. That document argues the constraint is an unowned cross-channel experience
   rather than anything discoverable, so its asks are an owner and a single-product pilot — not another
   study. If the group declines to assign an owner, the completeness work below will stall the same way
   the last two efforts did.
7. Then proceed to the measurement plan in `portfolio-hypothesis-customer-self-service.md` section 7.
