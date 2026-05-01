# Hellion Chat

A GDPR-compliant, Linux-aware fork of [Chat 2](https://github.com/Infiziert90/ChatTwo)
for FINAL FANTASY XIV / Dalamud.

Same chat replacement you know from upstream, with extra controls for what
actually gets stored:

- **Channel whitelist** for database persistence (GDPR Art. 25 — privacy by
  default). Out-of-the-box only your own conversations are kept: Tells, Party,
  Free Company, Linkshells, Cross-World Linkshells, Alliance, ExtraChat. Public
  chat from strangers (Say/Shout/Yell), Novice Network, NPC dialogue, system
  spam and battle messages stay outside the database unless you opt them in.
- **Per-channel retention** with a 24-hour idempotent background sweep. Tells
  default to 365 days, own-conversation channels to 90, the global default to
  30. Off until you switch it on — the plugin never deletes history without
  your explicit consent.
- **Retroactive cleanup** with a Ctrl+Shift confirm. Apply the current
  whitelist to an existing 800-MB+ database, watch it shrink to MBs, all on a
  background thread.
- **Export** to Markdown, JSON or CSV (GDPR Art. 15 right of access). Filter
  by channel, date range or sender substring; written via Dalamud's file
  dialog without blocking the UI.
- **First-run wizard** with three profiles (Privacy-First / Casual / Full
  History) that maps to concrete configuration sets. Reopenable from the
  Privacy tab any time.
- **Independent plugin state.** Config and database live under
  `pluginConfigs/HellionChat/`, completely separate from upstream Chat 2 — you
  can run both side by side, or migrate from Chat 2 once and keep going.
- **Migration recovery.** Heals databases left in a half-applied Migrate3
  state (columns added, `user_version` never bumped) without needing the
  backup file upstream creates.
- **Localized UI (EN + DE).** All Hellion-specific surfaces follow Dalamud's
  language override and switch live. Translations live in
  `Resources/HellionStrings.<lang>.resx`.

## Status

Bootstrap (v0.1.x). Used in production on a single user's setup. Not (yet)
submitted to the official Dalamud plugin repository — distributed as a
custom-repo / dev-plugin while the architecture stabilises.

## Install (testers)

Hellion Chat is shipped via a Dalamud **custom repository** during the
bootstrap phase.

### If you have never used Chat 2

1. Open Dalamud settings (`/xlsettings`) → **Experimental**.
2. Add a new entry under **Custom Plugin Repositories**:
   ```
   https://raw.githubusercontent.com/JonKazama-Hellion/HellionChat/main/repo.json
   ```
3. Click **Save**, then back in `/xlplugins` hit **All Plugins** and refresh.
4. **Hellion Chat** now appears in the list — install it.

### If you are migrating from Chat 2 (and want to keep your history)

The two plugins share `pluginConfigs/ChatTwo/` (database) and
`pluginConfigs/ChatTwo.json` (settings). Hellion Chat moves both into
`pluginConfigs/HellionChat/` on first start, but only if the upstream
plugin isn't holding the database file open. Order matters:

1. **Disable Chat 2** in `/xlplugins` (don't uninstall, just disable).
2. **Close FFXIV completely** so SQLite releases its file lock — a plain
   plugin reload is not enough.
3. Re-launch the game.
4. Add the custom repo URL as in the previous section.
5. Install Hellion Chat. On its first start it migrates the Chat 2 config
   file and the entire database directory into the HellionChat layout
   without losing data.
6. Verify in **Settings → Privacy → Apply filter to existing database →
   Refresh preview** that the message count is what you expect (millions
   of rows if you used Chat 2 for a while).

If the message count comes back as zero, the migration was blocked
(usually because Chat 2 was still active or the previous game session
hadn't fully closed). See the troubleshooting section below.

### Troubleshooting

**Hellion Chat shows zero messages but I had Chat 2 history:**
The migration either didn't run or hit a locked file. Close the game,
then move the data manually:

Linux / XIVLauncher Core:
```bash
mv ~/.xlcore/pluginConfigs/ChatTwo/chat-sqlite.db \
   ~/.xlcore/pluginConfigs/HellionChat/chat-sqlite.db
[ -d ~/.xlcore/pluginConfigs/ChatTwo/EmoteCacheV1 ] && \
  mv ~/.xlcore/pluginConfigs/ChatTwo/EmoteCacheV1 \
     ~/.xlcore/pluginConfigs/HellionChat/
```

Windows / standard XIVLauncher:
```powershell
Move-Item "$env:AppData\XIVLauncher\pluginConfigs\ChatTwo\chat-sqlite.db" `
          "$env:AppData\XIVLauncher\pluginConfigs\HellionChat\chat-sqlite.db" -Force
```

Then start the game and Hellion Chat — your full history is back.

### Updates

Updates land in the same plugin list once the maintainer pushes a new
`v0.1.x` tag and re-publishes the GitHub release. No re-installation
needed.

## Why a fork

The upstream maintainer has left filtering-related issues open since 2024
([#84](https://github.com/Infiziert90/ChatTwo/issues/84),
[#173](https://github.com/Infiziert90/ChatTwo/issues/173),
[#174](https://github.com/Infiziert90/ChatTwo/issues/174)). The original
design treats the database as an unlimited searchable archive of *everything*
the chat window sees, which is fine in the US-/JP-shaped privacy mindset but
hard to reconcile with EU GDPR data minimization rules when the archive
contains messages from third parties.

Forking under EUPL-1.2 is explicitly permitted, the upstream stays
authoritative for the chat-replacement engine, and we cherry-pick relevant
upstream bugfixes from `Infiziert90/ChatTwo` periodically.

## Build

```bash
# Linux with XIVLauncher Core
cp .env.example .env
# adjust DALAMUD_HOME if your hooks live somewhere else
set -a; source .env; set +a
dotnet build ChatTwo/ChatTwo.csproj
```

The output assembly is `ChatTwo/bin/Debug/HellionChat.dll`. Add the parent
directory as a Dev Plugin Location in Dalamud's experimental settings.

## Branding assets

`ChatTwo/images/icon.png` is the upstream Chat 2 icon and stays in place
until a hand-drawn Hellion logo replaces it. **No AI-generated artwork —
ever.**

## License

EUPL-1.2 (same as upstream Chat 2). See `LICENCE`.

## Acknowledgments

- **Infi & Anna (ascclemens)** — original Chat 2 engine, filtering, IPC, all
  the heavy lifting before this fork existed.
- **Dalamud team** — the plugin framework underneath everything.
- **JonKazama-Hellion** — fork maintenance, privacy/retention/export
  features, German localization.

## AI assistance disclosure

See `AI_DISCLOSURE.md`.
