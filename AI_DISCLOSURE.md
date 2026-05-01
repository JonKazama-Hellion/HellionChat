# AI assistance disclosure

This document follows the [Dalamud Plugin AI Usage Policy](https://github.com/goatcorp/DalamudPluginsD17/)
disclosure guidelines.

## Classification

**Pair** — from the official spectrum (None / Hint / Assist / **Pair** / Copilot / Auto).

The maintainer (JonKazama-Hellion / Florian Wathling) plans the architecture,
makes every design decision, reviews and integrates each change, and personally
tests the plugin against a running game client. Claude (Anthropic) acts as a
mentor/reviewer: it explains Dalamud APIs from official documentation,
suggests implementation patterns, drafts code snippets when asked, and
challenges design choices — but it never autonomously decides what gets
shipped, and it cannot test the running game itself.

The pragmatic test of "Pair" is not "did you type every character yourself"
but "can you explain why each piece of code is the way it is". The maintainer
can answer that question about every line of Hellion-specific code in this
fork.

## What is upstream and what is Hellion

This fork is built on top of [Chat 2 by Infi & Anna](https://github.com/Infiziert90/ChatTwo).
The upstream code is licensed under EUPL-1.2 and was not produced by AI
assistance; the upstream repository has its own contribution history and
licensing.

Hellion-specific changes (everything in
`ChatTwo/Privacy/`, `ChatTwo/Export/`, `ChatTwo/Resources/HellionStrings*.resx`,
`ChatTwo/Resources/HellionStrings.Designer.cs`, `ChatTwo/Ui/SettingsTabs/Privacy.cs`,
`ChatTwo/Ui/FirstRunWizard.cs`, the Migrate3 idempotency block in
`MessageStore.cs`, the assembly rename and migration logic in `Plugin.cs`)
were authored under the Pair workflow described above. Existing files
modified for the fork carry the same EUPL-1.2 license; original copyright
headers remain in place.

## Workflow rules followed

1. **Understanding before code.** Every Dalamud API used was checked against
   the official `dalamud.dev/versions/v15` documentation, ChatTwo's own
   usage as a reference, or the FFXIVClientStructs source.
2. **Verified APIs.** Microsoft Learn / Dalamud docs are consulted before
   accepting AI-suggested API surfaces, because LLM training data
   frequently lags behind Dalamud's API changes.
3. **Live testing.** Every feature was loaded as a dev plugin, exercised in
   the running game, and the database state inspected via `sqlite3` /
   `python3 -m sqlite3` queries to confirm the expected effect.
4. **Hand-drawn icon — no AI artwork.** The Hellion logo, when added, will
   be human-drawn. The `images/icon.png` currently in the repository is the
   original Chat 2 icon by the upstream authors.
5. **Transparent disclosure.** This file is part of the repository from
   v0.1 onward, before any submission to the official Dalamud plugin repo.

## Tooling

- **Claude (Anthropic) via Claude Code CLI** — primary AI mentor, used
  through interactive sessions. Each session starts with the maintainer
  describing intent and architectural questions; Claude reviews proposed
  changes, points out issues, and produces code drafts that the maintainer
  reads, edits, and integrates.
- **Context7** — official documentation lookup MCP, used to ground answers
  about Dalamud, .NET, MessagePack, MySqlConnector etc. in current docs
  rather than LLM training memory.
- **Microsoft Learn** — referenced for .NET SDK 10 features and
  cross-platform behavior.

## What AI is *not* used for in this fork

- **Code review of license-sensitive boundaries.** All EUPL-1.2 attribution
  decisions are made by the maintainer.
- **Translations.** German translations in `Resources/HellionStrings.de.resx`
  are written and reviewed by the maintainer (native speaker). Future
  community translations will go through human translators or
  community-driven platforms (e.g. a dedicated Crowdin project), not
  bulk-translated via LLM.
- **Visual assets.** No AI-generated icons, banners, or screenshots.

## Contact

Issues with this disclosure or with the AI handling of any specific commit
can be raised at <https://github.com/JonKazama-Hellion/HellionChat/issues>.
