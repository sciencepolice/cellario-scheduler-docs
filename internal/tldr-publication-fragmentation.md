# Where are customers actually supposed to find our documentation?

**Daniel Stugan · 2026-08-18 · 2-minute read**

## The short version

We hand customers documentation five different ways: PDFs exported out of Confluence, controlled PDFs
emailed, the Salesforce knowledge base, the developer site, and the help files that install
with the product. Nothing tells a customer which one is the real one, and each has potentially different audiences but should be accessible to all. Nobody picked five — we ended up with five because nobody ever owned the question *"where does a customer go when they need to know how their version works?"* So each one is missing something the others have: the only document with a revision number on it goes to one person's inbox, the only content that actually matches the customer's version can't be searched, and our most up-to-date answers sit behind a login that's often held by the customer's IT team rather than the engineer looking at the error. Picture that engineer with a run down and scientists waiting — working out which copy to trust takes longer than just calling us. **That's the part that bothers me: calling us is the smart move even when we've written everything down.** Which means finishing the docs doesn't fix it.

## The five, and what each one can't do

- **Confluence PDF export** — a snapshot of a page that keeps changing, and no way for the customer to know it's out of date
- **Controlled PDF, emailed** — the one document with a revision number on it; goes to one person, usually once, at handover
- **Salesforce knowledge base** — our freshest answers; needs a login the person hitting the problem often doesn't have
- **Developer site** — the only one they can actually search; doesn't say which version it's describing
- **Help files installed with the product** — always match the version they're running; only exist on that one machine

Every one of them is either trustworthy, easy to reach, or clearly tied to a version. None of them is two
of those at once.

## What I'd like from the group

1. **Tell me if you think I've got the problem right** — that this is about *where* we put things, not
   just how much we've written. If you don't buy it, say so and I'll drop it.
2. **Let me spend a week checking.** Pick one product at one version, pull together everything we've
   published about it across all five places, and see how often they contradict each other. If they
   mostly don't overlap, this is a small problem and the fix is just pointing people to the right place.
   If they contradict, it's a much bigger one. We've genuinely never looked.

Two things I'm **not** saying. I'm not saying move everything onto one website — we can't, because the
controlled documents are a compliance requirement and plenty of lab machines have no internet access. And
this isn't a version control problem; docs-as-code already handles that.

The whole point is just this: **five places, no way to tell which one wins, and a different lock on each
door. Customers can't work out what they're even able to see, and we've never asked whether they can.**

*Longer write-up with the personas, the GxP angle, and what would prove me wrong:
`internal/problem-statement-publication-fragmentation.md`*
