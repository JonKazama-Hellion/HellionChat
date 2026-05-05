# AI Assistance Disclosure

HellionChat uses AI assistance per the
[Dalamud Plugin AI Usage Policy](https://github.com/goatcorp/DalamudPluginsD17/)
at the **Pair** level.

A note up front: HellionChat is currently not submitted to the official
Dalamud plugin repository and technically has no obligation to disclose
this. I would rather be upfront about how it is built.

HellionChat is my entry point into game modding and plugin development.
I have never written a plugin for a game before. I work alone, so I get
help where I need it. That is not something I want to hide.

## How I Actually Work

I plan the architecture, decide what gets built, and own every design
decision. For each change I:

- Read the code Claude drafts before I integrate it
- Test with my own tooling and in the running game
- Read the Dalamud log output to verify behaviour
- Run security and privacy audits on anything that touches user data

One of the main reasons I use AI is consistency. I want the HellionChat
code to match the style of the upstream Chat 2 codebase and stay
readable for anyone who opens the repo, not just for me. Claude helps
me catch when I am drifting from upstream conventions or writing
something that only makes sense in my own head.

The balance is shifting toward more hand-written work as I get more
comfortable with Dalamud and plugin development in general.

## What AI Is Used For

- API explanations (Dalamud, ImGui, .NET specifics I have not worked
  with before)
- Code drafts that I read, edit, and integrate
- Pattern suggestions and code review
- Keeping style aligned with the upstream Chat 2 codebase

## What AI Is Not Used For

- **Visual assets.** Logos, icons, banners, and screenshots are
  human-drawn or taken from the running game.
- **German translations.** Written by me as a native speaker.

## What Is Where

Upstream Chat 2 (by Infi & Anna, EUPL-1.2) is the foundation and was
not produced with AI assistance. HellionChat-specific code lives in
`HellionChat/Privacy/`, `HellionChat/Export/`,
`HellionChat/Resources/HellionStrings*`, `Ui/SettingsTabs/Privacy.cs`,
`Ui/FirstRunWizard.cs`, `Ui/HellionStyle.cs`, plus the Migrate3
recovery and plugin layout migration in `MessageStore.cs` and
`Plugin.cs`. These were developed with Pair-level assistance as
described above.

## If AI-Assisted Development Is a Dealbreaker for You

Fair enough. There are solid alternatives:

- [Chat 2](https://github.com/Infiziert90/ChatTwo), the upstream
  project HellionChat is built on
- [XIV Instant Messenger](https://github.com/NightmareXIV/XIVInstantMessenger),
  a different approach to FFXIV chat

Both are good projects. Use what fits you best.

## Tooling

| Tool | Purpose |
| ---- | ------- |
| [Claude](https://claude.ai) (Anthropic) | Pair-level AI assistance via Claude Code CLI |
| [VS Code](https://code.visualstudio.com) + C# Dev Kit | Primary IDE |
| Dedicated Windows 11 VM | Build and in-game test environment (Dalamud requires Windows) |
| [dalamud.dev](https://dalamud.dev) | Dalamud API reference |
| [Microsoft Learn](https://learn.microsoft.com) | .NET and C# documentation |
| [Context7](https://context7.com) | Up-to-date library docs for Claude context |
| [Stack Overflow](https://stackoverflow.com) | General C# and .NET problem-solving |

## Contact

Questions about this disclosure:
<https://github.com/JonKazama-Hellion/HellionChat/issues>
