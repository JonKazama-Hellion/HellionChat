# AI assistance disclosure

Per the [Dalamud Plugin AI Usage Policy](https://github.com/goatcorp/DalamudPluginsD17/),
this fork uses AI assistance at the **Pair** level: the maintainer plans,
reviews and integrates every change and tests it against a running game
client; Claude (Anthropic) helps explain Dalamud APIs, suggest patterns and
draft code on request.

## What's where

Upstream Chat 2 (by Infi & Anna, EUPL-1.2) is the foundation and was not
produced with AI assistance. Hellion-specific code — `ChatTwo/Privacy/`,
`ChatTwo/Export/`, `Resources/HellionStrings*`, `Ui/SettingsTabs/Privacy.cs`,
`Ui/FirstRunWizard.cs`, `Ui/HellionStyle.cs`, the Migrate3 recovery and
plugin layout migration in `MessageStore.cs` / `Plugin.cs` — was written
with Pair-level assistance.

## What AI is not used for

- **Visual assets.** Logos, icons, banners, screenshots are human-drawn or
  taken from the running game.
- **German translations.** Written by the maintainer (native speaker).

## Tooling

- Claude (Anthropic) via Claude Code CLI as the main pair partner.
- Context7 / Microsoft Learn for current Dalamud and .NET documentation.

## Contact

Questions about this disclosure: <https://github.com/JonKazama-Hellion/HellionChat/issues>.
