# Nobody owns the customer documentation experience

**Daniel Stugan · 2026-08-18 · 2-minute read**

## The short version

We publish customer documentation five different ways: PDFs exported out of Confluence, controlled PDFs
emailed at handover, the Salesforce knowledge base, the developer site, and the help files that install with
the product. **Those five exist for good reasons** — an engineer troubleshooting a run, an IT admin
installing software, a quality team looking for controlled documents, and a developer wiring up an API all
genuinely need different things, and no single channel could serve all four. The problem isn't that we have
five. It's that all five grew up on their own and **nobody owns how they work together.** We've never said
which audience each one is for, what belongs where, which one wins when two of them cover the same thing, or
how a customer gets from one to another. So an engineer with a run down and scientists waiting has no idea
which door is theirs, and working out which copy to trust takes longer than just calling us. **The thing
we're missing isn't fewer channels. It's being able to say: for this person, doing this job, on this
version — go here. And when two of them overlap, we know which one wins.**

## And we all know this already

That's the part that actually matters. Nobody needs convincing that our documentation is fragmented — it's
been common knowledge for years. So I don't think this needs more analysis. **An audit proving our docs are
scattered would just be a very polished way of telling everyone something they'd tell you themselves in the
hallway.** Awareness hasn't produced action, and doing awareness again more rigorously isn't a strategy.

The real gap is that *"where does a customer go?"* isn't anyone's question. Every one of the five owners is
doing their own job correctly. That question sits above all of them and there's no seat where it lands.
Nobody's driving it, and nobody's failing at it either, because it was never theirs.

## Three companies in our space, and what they each got right

**Formulatrix** (`help.formulatrix.com`) puts **the version number in the web address** — Rock Maker alone
has help live for 3.10 through 4.11, across about ten products. Same market, same regulated customers, same
reality of sites staying on old versions for years. So "that's probably too expensive" isn't a free
objection any more.

**UniteLabs** (`docs.unitelabs.io`) and **Automata** (`docs.automata.tech`) both organize everything by
**what the customer is trying to do** — Get Started, Integrate, Operate, Automate, Observe. Both have real
developer and SDK documentation. Neither shows a version anywhere.

Two things I'd pull out of that:

- **All three are completely public.** Ours isn't — our freshest answers *and* our revision-controlled
  documents both sit in the Salesforce KB, behind a login a prospect doesn't have and public search can't
  see.
- **They organize by what the reader is doing. We organize by which internal team produced it.** You can
  see the ownership gap from the outside without knowing anything about how either company is run.
One caveat before anyone says "let's build that": all three are basically single-product companies with no
long tail of validated installs. **They're evidence about how to organize it and who owns it — not evidence
that our five channels should become one.**

## What I'd like from the group

1. **Agree this is an ownership problem across channels, not a "we haven't finished writing the docs"
   problem.** If you think it's really the second one, say so — that's a fair position and it changes what
   we'd fund.
2. **Put someone in charge of the customer documentation experience across all five channels** — with the
   authority to decide, for each one: who it's for, what goes in it, which one wins where they overlap, who
   can get into it, and how it gets versioned.
3. **Give them one product at one version to start with.** Map what a customer actually runs into today and
   find where it breaks down. Not to prove the problem — to produce the first real answer to "for this
   person, this job, this version, go here."

## What I'm not asking for

**Not consolidation.** Everything on one website doesn't work — we have to keep producing controlled
documents to show our documents are revision-controlled, and plenty of lab machines have no internet at
all. We might do this work and decide five
channels is exactly right. And this isn't a version control problem either; docs-as-code already handles
that.

*Longer write-up with the personas, the competitor comparison, and what would prove me wrong:
`internal/problem-statement-documentation-ownership.md`*
