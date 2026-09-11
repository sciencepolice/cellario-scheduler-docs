# Problem Statement: Nobody Owns the Customer Documentation Experience

**Status:** Draft for discussion — a reframe, not a plan
**Owner:** Daniel Stugan
**Date:** 2026-08-18
**Personas:** From `cellarioscheduler-personas.md`
**Scope:** Cellario OS, Cellario Scheduler, Cellario Solutions — all customer-facing published information

> **Internal document.** Lives outside `docs/` deliberately — Archbee's sync root is `./docs`, so nothing
> here reaches the published customer site. Do not move it under `docs/`.

> **This document is the parent** of `portfolio-hypothesis-customer-self-service.md` and
> `problem-framing-canvas-customer-self-service.md`. Both of those frame the problem as **completion** —
> two corpora stalled around 60%, nobody accountable for "done." That's real but downstream. Completion
> measures one channel against itself; the customer's experience is all five channels at once, minus
> whatever they can't reach. Where this document and the other two disagree, this one governs. Section 7
> itemizes the disagreements.

> **Supersedes an earlier draft** that framed the problem as "five channels, customers can't tell which is
> authoritative." That framing was wrong in an important way — see section 1.

---

## 1. The Problem

**We don't have a documentation publishing strategy, and nobody owns the customer documentation
experience.**

Our five channels evolved to serve genuinely different audiences under genuinely different constraints.
But nobody owns how they work together, so we have never defined which audience each channel serves, what
content belongs where, which source wins when two of them cover the same thing, or how a customer moves
between them.

**The problem is not that five channels exist. The problem is that five channels evolved independently and
nobody owns the system they collectively create.**

That distinction matters, because the obvious-sounding fix — consolidate everything into one place — is
wrong. An automation engineer troubleshooting a runtime error at a workstation, an operator recovering an
error mid-run, an IT admin installing software onto a segmented lab network, and a developer integrating
against an API legitimately need different documentation experiences. We may well finish this work and conclude that five
channels is the right number. **What's missing isn't fewer channels. It's a defined experience:**

> **For this audience, doing this task, on this software version → go here. And where two channels overlap
> → we know which one wins.**

We cannot say that sentence today for any audience, any task, or any version.

---

## 2. The Five Channels, and What Each Is Actually Good At

Read this as a defense of the channels, not an indictment of them. Every one of them exists for a reason,
and four of the five serve a real customer audience that no other channel serves as well.

| Channel | Audience it evolved for | The job it does well | What it structurally can't do |
|---|---|---|---|
| **Controlled PDF** — emailed at handover *and* uploaded to the Salesforce KB | **Us, not a customer audience** — it exists to show we control document revisions, which we need in order to say we practice GAMP 5 | Prove our documents are revision-controlled | Be found by public search, or reached by anyone without a KB login |
| **Salesforce knowledge base** | COMPANY-P-3 / COMPANY-P-2 post-sale, under an entitlement | Carry the freshest operational answers — **and hold the controlled PDFs** | Reach anyone without portal credentials, or be found by public search |
| **Developer site (Archbee)** | SWP-P-3, SWP-P-4, COMPANY-P-11 — and COMPANY-P-13 during evaluation | Be public, searchable, and indexed, which is a pre-sale requirement | Say which product version it describes |
| **Installed HTML help** | COMPANY-P-2 / COMPANY-P-3 standing at the workstation | Match the installed version exactly, and work with no network at all | Be reached from anywhere but that one machine |
| **Confluence PDF export**, sent on request | Customers **self-deploying Cellario Scheduler** | Answer a real installation need that nothing else covers | Exist at all unless someone remembers to send it; carry a revision; be found again later |

**Two things in that table are more interesting than the count of channels.**

**One: the Salesforce KB is quietly becoming the real destination, and it's the most gated one we have.**
It carries the freshest operational answers *and* it holds the controlled PDFs. That means the most current
material and the only revision-controlled material now live in the same place — which is the closest thing
to a canonical channel we've ever had, and nobody decided it. It also means **everything authoritative sits
behind the same login**, invisible to public search, unreachable by a prospect, and often held by the
customer's IT team rather than the person with the problem. We may have already converged on an answer
without noticing, and picked the hardest door to open.

*One thing worth checking inside that channel:* a controlled document and a support article written last
Tuesday sit side by side there with nothing marking which is which. **[Unverified — does the KB show
revision numbers, and does a superseded revision come down when a new one goes up?]** Authority between
channels is undefined; this asks whether it's defined *within* the one channel that matters most.

**Two: the Confluence export has a real audience and no real channel.** Customers self-deploying Cellario
Scheduler need installation documentation, and it exists — in an internal wiki, reaching them only when
someone remembers to export it and attach it to an email. **Self-deployment is the most self-sufficient
thing a customer can do with our software**, which makes it exactly the behavior the portfolio hypothesis
says we want more of. Its documentation depends on a person remembering. That isn't an internal artifact
leaking outward, which is what I assumed before — it's a genuine customer job with no home.

**And notice what the other four rows have in common: the audience column is my inference, not our
policy.** Nobody has ever written down that the KB serves runtime troubleshooting, or that the developer
site serves pre-sale evaluation. Those assignments feel obvious in a table and have never been stated, so
in practice content lands wherever its author happened to have publishing access. **An unstated assignment
isn't an assignment.**

---

## 3. What "Nobody Owns the Seams" Produces

Each channel's property set is correct *for its own audience*. The failures all live between them — which
is exactly the space nobody owns.

1. **Everything authoritative sits behind one gate, and nothing inside it says which is which.** The
   controlled PDFs are uploaded to the KB as well as emailed, so they reach further than a handover
   attachment — but only for someone holding portal credentials, never through public search, and sitting
   alongside support articles with nothing marking which one carries a revision. Reach improved; authority
   didn't. And no channel outside that gate picks up version identity at all, so for anyone without a KB
   login the only version-identified documentation we publish may as well not exist.
2. **Searchable and version-identified never meet.** The two channels that reliably state which version
   they describe are invisible to search. The channel a customer reaches first is the one that can't answer
   "does this describe the 4.5 I run or the 4.7 you ship?"
3. **Current and reachable never meet.** Our freshest answers sit behind an entitlement — a commercial
   decision, not an oversight. But COMPANY-P-9's persona notes that some IT Directors require all user
   tickets go through their own system first, so at those sites the credential sits with IT, one
   organization away from the person holding the error message.
4. **Overlap has no tiebreaker.** Where two channels cover the same topic, nothing says which wins. HRB-P-2
   can cite a Confluence page while HRB-P-3 cites a KB article in the same week, and both are "the
   documentation." **We are a source of the contradiction, not only a victim of it.**
5. **Nothing hands a customer from one channel to another.** No channel tells you when you're in the wrong
   one, and none of them links to the others.
6. **One customer job depends on a person remembering.** Self-deploy Scheduler documentation reaches
   customers only when somebody exports it and sends it. That isn't a channel with a weakness — it's a
   delivery mechanism whose reliability is somebody's attention, and it serves the most self-sufficient
   thing a customer can do with our software.

Items 1–3 can't be fixed inside any single channel. That's the point. They're seams, and the seams have no
owner.

---

## 4. What This Feels Like — COMPANY-P-3, Experienced Automation Engineer

**I am** an automation engineer with ten years and several vendor integrations behind me. My primary
responsibility is keeping the integrated system running and dealing with errors during runs, so I meet your
documentation under time pressure rather than while exploring. The single thing I dislike most, in my own
words, is **downtime**. Scientists are standing next to the instrument while I work.

**Trying to** get a verified answer about how *my installed version* behaves, fast enough that the run
recovers before it costs the lab a day — and to keep owning that recovery myself.

**But** nobody ever told me which door is mine. The help file on the workstation matches my version and
can't be searched from my desk. The site I can search doesn't say which version it describes. The knowledge
base probably has the freshest answer and wants a login my IT team holds. Somewhere there's a controlled PDF
with a revision number on it that went to my project manager at handover. When two of these disagree,
nothing tells me which to believe. **So I open a support case — not because the information doesn't exist,
but because working out which copy to trust takes longer than calling you.**

**Which makes me feel** like the answer is out there and I'm not allowed to have it. I can explain hardware
downtime to the scientists waiting on me. I can't explain downtime spent deciding which of my vendor's five
documents to believe.

**Also affected:** COMPANY-P-2 (largest population, lowest product knowledge, explicitly wants manuals
that are "up to date, comprehensive, and accurate"); COMPANY-P-13 (scores us during evaluation and sees
only the public channel, so four fifths of what we've written is invisible exactly when we're being judged);
HRB-P-3 (whose stated goal is building self-service to reduce tickets — into the channel with the narrowest
access, against a customer base spread across versions).

---

## 5. Three Peers, and What Each One Got Right

Worth putting in front of the group, because it turns "we should sort this out" from an opinion into a
comparison. *(All three checked 2026-08-18 by looking at the sites. Shallow looks — worth a second pair of
eyes before anyone quotes this in a meeting.)*

| | Public, no login | Version in the docs | Organized by | Developer / SDK | Operator | IT / install |
|---|---|---|---|---|---|---|
| **Formulatrix** `help.formulatrix.com` | Yes | **Yes — in the URL** | Product | Not visible | **Yes** | Not visible |
| **UniteLabs** `docs.unitelabs.io` | Yes | No | **What you're doing** | **Yes** | Some | **Yes** |
| **Automata** `docs.automata.tech` | Yes | No | **What you're doing** | **Yes** | Some | Some |
| **HighRes** | 1 of 5 channels | Only behind a login or on one machine | **Who published it** | Yes, partial | Stub | **Only if you ask** |

**Formulatrix** publishes at `help.formulatrix.com/[product]/[version]/` — the version is in the address,
and Rock Maker alone has help live for 3.10 through 4.11, old versions still up, across roughly ten
products. Operator-facing; no developer or API documentation visible.

**UniteLabs** navigates by **Get Started / Integrate / Operate / Automate / Observe / Reference / CDK.**
Four steps to your first script, walkthroughs, a browsable Swagger UI, SDK docs, a connector development
kit, and a real section for IT — network requirements, multi-device networking, headless install. Evergreen,
with no version anywhere.

**Automata** — the closest of the three to what we do — navigates by **Installation / Getting Started / CLI
Quickstart / Topics / Concepts / Tutorials.** Python SDK, CLI, tutorials, concept reference. Public. No
version indicator, and no search visible.

### Three things fall out of that table

**One: all three are fully public, and our best material isn't.** COMPANY-P-13 scores us during evaluation
on whatever they can read without asking, and never files a support case. Every one of these competitors
can be read end to end by someone who hasn't spoken to sales. Our freshest content sits behind a support
entitlement that a prospect doesn't have yet, and our most authoritative document is an email attachment.

**Two: they organize by what the reader is doing. We organize by who published it.** Integrate. Operate.
Automate. Observe. Installation. Getting Started. Every one of those is a verb belonging to a person with a
job. Our five channels are Quality's, Support's, Engineering's, Product's, and whatever got exported out of
Confluence. **That contrast is the ownership gap, visible from outside without knowing anything about how
either company is run.** A documentation set arranged by publisher rather than by reader is what an unowned
documentation experience looks like from the customer's side of the table.

**Three: only one of the three solved version identity — and we already have it, in the channel nobody can
reach.** Two of these peers are evergreen, exactly like our developer site. So version-matched
documentation is genuinely uncommon, which cuts two ways: it's harder than it sounds *and* our installed
HTML help has been doing it correctly by construction the whole time. **We're closer to the best example
here than we assumed — we just keep it on one machine in the lab, or behind a support login.**

### Two cautions on how these get used

**Don't read the table as an argument for one site.** All three are effectively single-product or
single-platform companies with no long tail of old versions still in service. We have three products, a
controlled-document obligation to satisfy, and lab workstations with no internet. **They are evidence about
how to organize it and who owns it, not evidence that five channels should become one.** And they aren't
equally strong: Formulatrix is the fair comparison on versioning cost, since it shares our market and the
same reality of customers staying on old versions for years. UniteLabs and Automata are younger and
software-only — cite them for *structure*, not for *what it would cost us*.

**And expect the reflex.** Showing competitor documentation to a room reliably produces "let's go build
that." **That's the consolidation misreading this document exists to avoid**, and it skips past the reason
all three can do it: somebody over there owns it. The takeaway isn't their sites. It's that **each of them
can answer "for this person, doing this job, go here" and we can't** — and somebody had to be responsible
for that to be true.

### And one thing the table exposes about our own

**The install audience is served by the least durable thing we have.** Two of the three peers give IT a
defined place — network requirements, headless install, multi-device setup. We have that content too: it's
the self-deploy Scheduler material, and it reaches customers as a Confluence export when somebody sends it.
So this isn't a content gap, it's a channel gap, and it lands on COMPANY-P-9 — who runs the security
questionnaire, controls the network layers our software has to live in, and can veto us over corporate
standards. **Nobody noticed because nobody owns the map** — section 1 restated as a concrete missing thing
rather than as an argument.

**A13 also gets weaker.** The hypothesis document treats per-version documentation as an open cost
question. One peer in our own market runs it in production across ten products. That doesn't tell us what it
costs us, or that Archbee can do it. It does mean **"that's probably too expensive" is no longer free** —
someone has to say why it's expensive here specifically.

---

## 6. Why This Isn't a Discovery Problem

**Everyone already knows the documentation is fragmented.** That isn't a hypothesis needing validation.
It's common knowledge, and has been for years.

Which means awareness was never the constraint, and more analysis won't move it. A thorough audit proving
our documentation is scattered would be a polished artifact telling a room full of people something they'd
tell you themselves in the hallway. **Shared awareness has already failed to produce action; doing it again
more rigorously is not a different strategy.**

The organizational failure is narrower and less comfortable: *"where does a customer go?"* is not any of
the five channel owners' question. Each owner is doing their own job correctly. The question sits above all
of them, and there is no seat where it lands. **Nobody is driving this, and nobody is failing at it either,
because it was never anyone's.**

That's the same vacuum the canvas identified for documentation completeness, one level up. Both symptoms
come from the same missing owner. Only one of them shows up on a status report.

---

## 7. Where This Supersedes the Existing Documents

1. **"60% complete" measures our effort, not the customer's access.** A topic can be fully documented on
   Archbee and still be the fourth-best-known copy at the site, sitting in the channel that audience never
   opens. Completion percentages aren't wrong, they're just not about the customer.
2. **A7 was the load-bearing assumption and got one line.** *"The audience finds and reaches whatever we
   publish"* is ranked twelfth of thirteen in the hypothesis document, on no evidence. It gates A3, A5, and
   A6 — something nobody can reach can't save a labor hour, win an evaluation, or replace a services
   engagement. Promoted to the top tier.
3. **The release gate has an undefined target.** Gating a release on documentation presupposes knowing
   *which channel* the gate applies to. Gate Archbee while controlled PDFs, KB articles, and installed help
   run on independent cadences, and we've gated one fifth of what the customer sees. A gap in the existing
   proposal, surfaced here rather than answered.
4. **Version-matched documentation already exists and we never counted it.** The hypothesis document treats
   version-matching as something to build. The installed HTML help has been
   version-matched by construction the whole time. Worth understanding why it's cheap there and expensive
   everywhere else before designing it from scratch.
5. **Contradiction may cost more trust than incompleteness does.** A11 says a reader failed once by a partial
   site doesn't return. Probably true — and a reader who finds two of our documents disagreeing has learned
   something worse, because absence merely fails to help while contradiction actively misleads.

The canvas's Phase 3 reframe and How Might We stay valid; they were scoped to the extension surfaces, and
this is scoped to everything we publish. Its "who benefits from the status quo" analysis applies with one
addition: **every channel owner keeps their own cadence and their own definition of done, and nobody has to
negotiate a shared bar.** That's comfortable to defend, and nobody defending it is acting in bad faith.

---

## 8. Context & Constraints

- **Controlled documents have to keep existing.** They are how we demonstrate our documents are
  revision-controlled, which we need in order to say we practice GAMP 5. That's an obligation on us rather
  than a service to a customer audience — but it's permanent, any direction inherits it, and any proposal
  that removes them is dead on arrival.
- **Version control is not the constrained part.** Every document needs to be under version control, and
  docs-as-code already handles it. Stating this plainly so the problem isn't misread: we are not blocked on
  the ability to version a document.
- **Lab network layering is real and permanent.** Per COMPANY-P-9, sites separate lab, business, and
  internet layers. The installed help file isn't legacy cruft — it exists because the workstation may have
  no internet. Anything that assumes connectivity at the point of use fails the same way we're failing now.
- **The KB entitlement gate is a commercial decision**, not an accident to undo casually.
- **Version sprawl.** Customers run 4.5 while we ship 4.7, and validated sites stay put for years by choice.
  Evergreen publication silently misdescribes most of the installed base.
- **Three products, three release trains.** Content convergence is not a goal.
- **Mixed audience, one product.** COMPANY-P-12 through COMPANY-P-2.
- **No interviews**, and no analytics ever compared across channels.

---

## 9. What This Asks For

1. **Agree this is a cross-channel ownership problem, not a documentation-completeness problem.** If the
   group thinks completeness is still the primary framing, say so explicitly — that's a legitimate position
   and it changes what gets funded.
2. **Assign an owner for the customer documentation experience across channels** — with the authority to
   define, per channel: which audience it serves, what content belongs there, which source is authoritative
   where channels overlap, who can access it, and how it's versioned. This is cross-functional by
   construction. It sits above all five current channel owners, which is why it has never landed anywhere.
3. **Give that owner one product at one version as a pilot.** Map the experience a customer actually gets
   today and find where they hit gaps, contradictions, or ambiguity. Not to prove the problem exists — to
   produce the first real version of *"for this audience, this task, this version → go here."*

**What this is explicitly not asking for:** consolidation. The controlled-documentation obligation and the
offline lab workstation each rule out a single destination on their own. We may finish the pilot and keep
all five channels. The deliverable is a defined experience, not a smaller number of channels.

---

## 10. What Would Make Me Wrong

- **The assignments are already written down somewhere** and I haven't found them → this is a communication
  problem rather than an ownership one, and much smaller.
- **The channels barely overlap in practice** → authority and tiebreaking stop mattering, and what's left is
  a signposting problem. Cheaper, and worth knowing early.
- **Customers report they know exactly where to go** → the access framing is wrong, completion was the right
  diagnosis, and the parent/child relationship here should be reversed.
- **An owner exists and is blocked rather than absent** → the ask is air cover and funding, not assignment,
  and I've misdiagnosed the failure.
- **Document control constrains how we deliver, not just what we produce** → the revision-control-versus-
  reach seam is something we're required to live with rather than a design gap, and item 1 in section 3
  stops being a criticism of us.

---

## Related Documents

- `portfolio-hypothesis-customer-self-service.md` — completion framing and the release-process proposal.
  Child of this document; its A7 is this document's subject and its §2 layer model now carries an L0.
- `problem-framing-canvas-customer-self-service.md` — Look Inward / Look Outward, including who benefits
  from the status quo. Compatible; scoped to extension surfaces.
- `epic-hypothesis-documentation-as-code-integration.md` — the Scheduler docs-as-code instance.
- `tldr-documentation-ownership.md` — the short version, for sending round.
