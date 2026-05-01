# AI assistance disclosure

Per the [Dalamud Plugin AI Usage Policy](https://github.com/goatcorp/DalamudPluginsD17/),
this fork uses AI assistance at the **Pair** level. Pair means the maintainer
plans the architecture, decides what gets built, reviews each change and
tests against the running game; Claude (Anthropic) helps explain Dalamud
APIs, suggests patterns, drafts code on request, and reviews approaches.
Neither side acts autonomously: nothing ships without the maintainer's
review, and Claude can't run the game.

The level varies by area and over time. Some commits are mostly hand-written
with the AI used as a sounding board, others lean more on Claude for an API
walkthrough or a code draft that the maintainer then reads, edits and
integrates. The maintainer's commitment is to be able to explain why every
piece of Hellion code is the way it is — not "I typed every character."

## What's where

Upstream Chat 2 (by Infi & Anna, EUPL-1.2) is the foundation and was not
produced with AI assistance. Hellion-specific code lives in
`ChatTwo/Privacy/`, `ChatTwo/Export/`, `Resources/HellionStrings*`,
`Ui/SettingsTabs/Privacy.cs`, `Ui/FirstRunWizard.cs`, `Ui/HellionStyle.cs`,
plus the Migrate3 recovery and plugin layout migration in `MessageStore.cs`
and `Plugin.cs`. These were developed with Pair-level assistance as
described above; the share of human vs. AI authorship varies file by file
and is expected to keep shifting toward more hand-written work as the
maintainer's plugin-dev experience grows.

## What AI is not used for

- **Visual assets.** Logos, icons, banners, screenshots are human-drawn or
  taken from the running game.
- **German translations.** Written by the maintainer (native speaker).

## Tooling

- Claude (Anthropic) via Claude Code CLI as the main pair partner.
- Context7 / Microsoft Learn for current Dalamud and .NET documentation.

## Contact

Questions about this disclosure: <https://github.com/JonKazama-Hellion/HellionChat/issues>.
