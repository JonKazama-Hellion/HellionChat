# Notice

## Acknowledgements

HellionChat is a fork of [Chat 2](https://github.com/Infiziert90/ChatTwo) by
**Infiziert90 (Infi)** and **Anna Clemens**, both of whom kept that plugin
running and maintained for years before I ever opened the source. Without
their work this fork would not exist, full stop. I owe them the architecture,
the message store, the channel filtering, the sidebar tab system, the
hooks into FFXIV's chat, the localisation infrastructure, and countless
small decisions that I only noticed because they had already been made
correctly.

If you find HellionChat useful, please remember that the foundation came
from Chat 2. The code Anna and Infi wrote is doing most of the heavy
lifting in this fork too.

## A direct word to Infi and Anna

Hi. I am Florian (Flo, also Jon Kazama in-game on Phoenix). I forked Chat 2
because I wanted a privacy-by-default version for my own use case and a
small group of friends I play with, not because I thought I could do
anything better than what you built. The opposite is true. ChatTwo's
default of full history and cross-character logging is the right call for
most users. I just have a different threat model and a different
data-handling philosophy that fits a smaller, locally-stored, retention-
limited approach.

What HellionChat adds is mostly Hellion-specific surface area: a privacy
filter, per-channel retention windows, an export pipeline, an Auto-Tell-
Tabs feature for FFXIV club greeters, the Hellion theme and font, German
localisation, and a settings UX rebuild. None of it touches the bones of
what you built. Where I had to modify your code I tried to keep the
edits minimal, isolated to clearly-marked Hellion files, and reversible.

Concrete example: when API 15 hit, I cherry-picked your fix for the
BetterTTV emote regression with `git cherry-pick -x` so authorship and
co-author trail stay intact. That is the standard I want to keep using as
long as both projects are alive. You should never have to look at this
fork and wonder if I quietly ate your work.

If anything in this fork ever steps on something you would not be okay
with, please reach out and I will fix it. Genuinely. The list of contacts
is below.

## Maintainer contact

If something in HellionChat causes problems, especially if it relates back
to Chat 2 or to anything Infi or Anna would want flagged:

- **GitHub Issues:** [JonKazama-Hellion/HellionChat/issues](https://github.com/JonKazama-Hellion/HellionChat/issues)
- **Discord:** `@j.j_kazama`
- **Email (business):** kontakt@hellion-media.de

I respond on weekdays during European business hours. For anything
urgent (security, attribution, takedown), email is the fastest path.

## Why this fork is not upstreamed

The privacy-by-default position fits a small audience. ChatTwo's
full-history-by-default position fits a much larger one, including the
roleplaying community where chat archive is part of the play experience.
Trying to upstream HellionChat's defaults would have meant arguing that
Chat 2's defaults are wrong, and they are not. They are right for the
user base ChatTwo serves. So I keep the fork separate, attribute clearly,
and pull selected upstream patches when they apply.

## Why HellionChat left the GitHub fork network

The Dalamud plugin ecosystem treats the GitHub-Fork relation as a signal
that a fork is either a development branch or a dead mirror. HellionChat
is neither. It is an independently-maintained EUPL-1.2 fork with its own
release cadence, its own custom repo, its own user base. Detaching the
fork-network relation just makes the situation honest. The git history,
the cherry-pick trail, and the attribution stay exactly the same. The
only thing that changes is the GitHub UI no longer says "forked from".

## Trademarks and naming

"Chat 2" and "ChatTwo" are the names Infi and Anna chose for the upstream
plugin. HellionChat does not use either of those names in user-facing
copy except where required to describe origin (settings tab, manifest,
this file, the README). The Hellion brand is mine.

## Questions

This file is the canonical place for "is this attribution correct, is the
maintainer reachable, is the relationship to Chat 2 documented". If
anything in here is wrong, please open an issue or contact me directly.

---

Maintained under **Hellion Forge**, the modding and plugin line of **Hellion Online Media** | Bad Harzburg | [hellion-media.de](https://hellion-media.de)
