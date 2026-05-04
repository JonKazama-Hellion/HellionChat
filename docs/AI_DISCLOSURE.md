# AI assistance disclosure

This fork uses AI assistance per the [Dalamud Plugin AI Usage Policy](https://github.com/goatcorp/DalamudPluginsD17/)
at the **Pair** level.

A note up front: Hellion Chat is currently in a rebuild and adjustment
phase, and there are no plans to submit it to the Dalamud team for review
while it stays standalone. If the plugin stays out of the official repo I
technically wouldn't need to disclose any of this, but I'd rather be
upfront about how it's built.

Hellion Chat is my entry point into game modding and plugin development. I
have never written a plugin for a game before. I work alone, so I get help
where I need it. That's not something I want to hide.

## How I actually work

I plan the architecture, decide what gets built, and own every design
decision. For each change I:

- Read the code Claude drafts before I integrate it
- Test with my own tooling and in the running game
- Read the Dalamud log output to verify behaviour
- Run security and privacy audits on anything that touches user data

One of the main reasons I use AI is consistency. I want the Hellion code to
match the style of the upstream Chat 2 codebase and stay readable for
anyone who opens the repo, not just for me. Claude helps me catch when I'm
drifting from upstream conventions or writing something that only makes
sense in my own head.

The balance is shifting toward more hand-written work as I get more
comfortable with Dalamud and plugin development in general.

## What AI is used for

- API explanations (Dalamud, ImGui, .NET specifics I haven't worked with before)
- Code drafts that I read, edit, and integrate
- Pattern suggestions and code review
- Keeping the style aligned with the upstream Chat 2 codebase

## What AI isn't used for

- **Visual assets.** Logos, icons, banners, and screenshots are human-drawn
  or taken from the running game.
- **German translations.** Written by me as a native speaker.

## What's where

Upstream Chat 2 (by Infi & Anna, EUPL-1.2) is the foundation and was not
produced with AI assistance. Hellion-specific code lives in
`HellionChat/Privacy/`, `HellionChat/Export/`, `HellionChat/Resources/HellionStrings*`,
`Ui/SettingsTabs/Privacy.cs`, `Ui/FirstRunWizard.cs`, `Ui/HellionStyle.cs`,
plus the Migrate3 recovery and plugin layout migration in `MessageStore.cs`
and `Plugin.cs`. These were developed with Pair-level assistance as
described above.

## If AI-assisted development is a dealbreaker for you

Fair enough. There are solid alternatives that don't rely on AI in their
development:

- [Chat 2](https://github.com/Infiziert90/ChatTwo), the original upstream
  this fork is based on
- [XIV Instant Messenger](https://github.com/NightmareXIV/XIVInstantMessenger),
  a different approach to chat in FFXIV

Both are good projects. Use what fits you best.

## Tooling

- Claude (Anthropic) via Claude Code CLI
- Context7 / Microsoft Learn for current Dalamud and .NET documentation

## Contact

Questions about this disclosure: <https://github.com/JonKazama-Hellion/HellionChat/issues>.
