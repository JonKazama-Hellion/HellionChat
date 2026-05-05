# Contributing to HellionChat

Thanks for taking a look. HellionChat is a one-person side project
developed under Hellion Forge. It started as a fork of
[Chat 2](https://github.com/Infiziert90/ChatTwo) and has since become
a standalone plugin under its own namespace, IPC channels and
source tree (standalone-cut completed in v1.0.0). Forking HellionChat
itself is explicitly permitted under the EUPL-1.2.

This document explains what I am looking for, what I am not, and how
to make a contribution land smoothly.

## Before You Open Anything

- Read the [README](README.md) so you understand the scope: a
  privacy-focused, EUPL-1.2-licensed Dalamud plugin that intentionally
  removes the upstream webinterface and ships privacy-first defaults.
- Read [`docs/UPSTREAM_SYNC.md`](docs/UPSTREAM_SYNC.md). Cherry-picks
  from upstream Chat 2 are selective and deliberate; not everything
  that lands there belongs here.
- Read [`SECURITY.md`](SECURITY.md). Anything security-sensitive goes
  through a private advisory, never a public issue or PR.
- Read the [Code of Conduct](CODE_OF_CONDUCT.md).

## What I Will Accept

- Bug fixes for behaviour documented in the README, the in-plugin
  settings or the changelog.
- Translation contributions for Hellion-specific strings via direct
  pull requests against
  `HellionChat/Resources/HellionStrings.*.resx`. Translations for
  upstream Chat 2 strings (`Language.*.resx`) are not handled here;
  those go to the upstream Chat 2 project.
- Documentation improvements (README, comments, this file).
- Performance fixes with a measurable before/after.
- New features that fit the privacy-first scope and do not duplicate
  what an existing Dalamud plugin already does well.

## What I Will Probably Decline

- Re-introducing the webinterface or any remote-access feature. It was
  removed in v0.2.0 on purpose. See the README section
  "Was gegenüber Chat 2 fehlt".
- Features that bypass the privacy filter or weaken the default
  retention behaviour without an explicit, documented opt-in.
- Sweeping refactors that touch large parts of the codebase. They make
  selective upstream cherry-picks much harder and the maintenance cost
  outweighs the benefit for a one-person project.
- AI-generated code dropped in without disclosure or human review. See
  [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md) for how I handle
  AI assistance on my side; I expect comparable transparency from
  contributors.

If you are unsure whether an idea fits, open a feature-request issue
first and ask before writing code. I would rather say "no" to a
proposal than to a finished pull request.

## Workflow

1. Open an issue (bug or feature request) using the templates under
   `.github/ISSUE_TEMPLATE/`. Skip this for trivial typos.
2. Fork the repository and branch off `main`. Branch naming is
   informal; something like `fix/auto-tell-history-empty` or
   `feat/theme-export` is fine.
3. Match the existing code style. The repository ships an
   `.editorconfig` that VS Code and Rider pick up automatically.
4. Keep commits focused. Several small commits with clear messages are
   easier to review than one large one. Squash-on-merge happens at
   the PR level if needed.
5. If your change touches user-visible behaviour, update the README
   and/or the changelog block in `HellionChat/HellionChat.yaml` and
   `repo.json`. I bump the version number myself at release time.
6. Open the pull request against `main`. The PR template will ask
   you to summarise the change, the testing you did and any
   compatibility notes.

## Build and Test

The project targets `net10.0-windows` against Dalamud SDK 15. To build
locally you need:

- .NET 10 SDK
- A working Dalamud dev environment with `DALAMUD_HOME` set
  (XIVLauncher installed and launched once is the simplest path)
- VS Code with the C# Dev Kit, Rider, or Visual Studio

```bash
dotnet restore
dotnet build HellionChat.sln -c Release
```

There are currently no tests in `HellionChat.sln`. If you add a test
project, point it at the relevant subsystems (privacy filter,
configuration migration, message store) and mention it in the PR.

For a smoke test in-game: build, copy the output into your Dalamud
`devPlugins/HellionChat/` directory and load it via `/xlplugins`.

## Continuous Integration

Every push and every pull request runs:

| Workflow      | What it checks                        |
| ------------- | ------------------------------------- |
| `build.yml`   | `dotnet build` and `dotnet test`      |
| `codeql.yml`  | CodeQL security analysis              |

A pull request will not be merged while either of these is failing.
CodeQL findings on changed code need to be addressed; pre-existing
findings on untouched code are tracked separately.

## Translations

Hellion-specific strings live in
`HellionChat/Resources/HellionStrings.resx` (English source) and
`HellionStrings.<lang>.resx` (per-language). These are accepted as
direct pull requests.

The upstream Chat 2 strings in `HellionChat/Resources/Language.*.resx`
are **not** translated here. They are owned by the upstream project
and synced in via cherry-pick. Please contribute those to
[Infiziert90/ChatTwo](https://github.com/Infiziert90/ChatTwo) instead.

## Licensing

By submitting a pull request you confirm that:

- Your contribution is your own work, or you have the right to
  contribute it under the project licence.
- You agree that your contribution will be released under the
  [EUPL-1.2](LICENSE), the same licence as the rest of the project.

There is no separate CLA. Forking HellionChat is explicitly permitted
under the EUPL-1.2, as with any EUPL-licensed project.

## Response Times

| Channel       | Address                    |
| ------------- | -------------------------- |
| GitHub Issues | Preferred for bugs and feature requests |
| Discord DM    | `@j.j_kazama`              |
| Email         | `kontakt@hellion-media.de` |

I respond on weekdays during European business hours and take weekends
and FFXIV patch days off. A pull request that sits for a few days has
not been ignored. Pinging once after a week is fine; please do not
ping daily.
