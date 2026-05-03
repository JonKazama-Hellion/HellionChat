# Contributing to HellionChat

Thanks for taking a look. HellionChat is a small, opinionated fork of
[Chat 2](https://github.com/Infiziert90/ChatTwo) maintained by one
person in spare time. This document explains what I am looking for,
what I am not, and how to make a contribution land smoothly.

## Before you open anything

- Read the [README](README.md) so you understand the scope: this is a
  privacy-focused, EUPL-1.2-licensed Dalamud plugin that intentionally
  removes the upstream webinterface and ships smaller defaults.
- Read [UPSTREAM_SYNC.md](UPSTREAM_SYNC.md). Cherry-picks from upstream
  Chat 2 are selective and conscious; not everything that lands there
  belongs here.
- Read [SECURITY.md](SECURITY.md). Anything security-sensitive goes
  through a private advisory, never a public issue or PR.
- Read the [code of conduct](CODE_OF_CONDUCT.md).

## What I will accept

- Bug fixes for behaviour documented in the README, the in-plugin
  settings or the changelog.
- Translation contributions for Hellion-specific strings via direct
  pull requests against `ChatTwo/Resources/HellionStrings.*.resx`.
  Translations for the upstream Chat 2 strings (`Language.*.resx`) are
  not handled here; they go through the upstream Chat 2 project.
- Documentation improvements (README, comments, this file).
- Performance fixes with a measurable before/after.
- New features that fit the privacy-first scope and do not duplicate
  what an existing Dalamud plugin already does well.

## What I will probably decline

- Re-introducing the webinterface or any remote-access feature. It was
  removed in v0.2.0 on purpose. See README "Was gegenüber Chat 2 fehlt".
- Features that bypass the privacy filter or weaken the default
  retention behaviour without an explicit, documented opt-in.
- Sweeping refactors that touch large parts of the upstream codebase.
  They make selective upstream cherry-picks much harder and the
  maintenance cost outweighs the benefit for a one-person project.
- AI-generated code dropped in without disclosure or human review. See
  [AI_DISCLOSURE.md](AI_DISCLOSURE.md) for how I handle AI assistance
  on my side; I expect comparable transparency from contributors.

If you are unsure whether an idea fits, open a feature-request issue
first and ask before writing code. I would rather say "no" to a
proposal than to a finished pull request.

## Workflow

1. Open an issue (bug or feature request) using the templates under
   `.github/ISSUE_TEMPLATE/`. Skip this step only for trivial typos.
2. Fork the repository and branch off `main`. Branch naming is
   informal; something like `fix/auto-tell-history-empty` or
   `feat/adblock-light-mode` is plenty.
3. Match the existing code style. The repository ships an
   `.editorconfig` that VS Code and Rider pick up automatically.
4. Keep commits focused. Several small commits with clear messages are
   easier to review than one big one. Squash-on-merge happens at the
   PR level if needed.
5. If your change touches user-visible behaviour, update the README
   and/or the changelog block in `ChatTwo/HellionChat.yaml` and
   `repo.json` for the next version. I bump the version number myself
   at release time, so you do not need to.
6. Open the pull request against `main`. The PR template will ask
   you to summarise the change, the testing you did and any
   compatibility notes.

## Build and test

The project targets `net10.0-windows` against Dalamud SDK 15. To build
locally you need:

- .NET 10 SDK
- A working Dalamud development environment with `DALAMUD_HOME` set
  (XIVLauncher installed and launched once is the simplest path)
- VS Code with the C# Dev Kit, Rider, or Visual Studio

```
dotnet restore
dotnet build ChatTwo.sln -c Release
dotnet test  ChatTwo.sln -c Release
```

The test project is `ChatTwo.Tests`. New behaviour should come with a
test where the existing test infrastructure makes that practical
(privacy filter, configuration migration, message store).

For a smoke test in-game: build, copy the output into your Dalamud
`devPlugins/HellionChat/` directory and load it through `/xlplugins`.

## Continuous integration

Every push and every pull request runs:

- `build.yml` — `dotnet build` and `dotnet test`
- `codeql.yml` — CodeQL security analysis

A pull request will not be merged while either of these is failing.
CodeQL findings on changed code need to be addressed; pre-existing
findings on untouched code are tracked separately.

## Licensing

By submitting a pull request you confirm that:

- Your contribution is your own work, or you have the right to
  contribute it under the project licence.
- You agree that your contribution will be released under the
  [EUPL-1.2](LICENSE), the same licence as the rest of the project.

There is no separate CLA.

## Translations

Hellion-specific strings live in `ChatTwo/Resources/HellionStrings.resx`
(English source) and `HellionStrings.<lang>.resx` (per-language).
Translations are accepted as direct pull requests against those files.

The upstream Chat 2 strings in `ChatTwo/Resources/Language.*.resx` are
**not** translated in this repository. They are owned by the upstream
Chat 2 project and synced in via cherry-pick. Please contribute
upstream-string translations to
[Infiziert90/ChatTwo](https://github.com/Infiziert90/ChatTwo) instead.

## A note on response times

I respond on weekdays during European business hours and I take
weekends and FFXIV patch days off. A pull request that sits for a few
days has not been ignored; I just have not gotten to it yet. Pinging
once after a week is fine; please do not ping daily.
