# Privacy notice

HellionChat is a Dalamud plugin for FINAL FANTASY XIV that focuses on
giving the user explicit control over what their chat client stores
locally. This document describes what the plugin does with your data,
what it does not do, and how you exercise the rights the GDPR gives
you over data you generate yourself.

This document is informational. The maintainer of HellionChat is
**not** a controller or processor of your data in the GDPR sense,
because no data ever leaves your machine on the maintainer's
infrastructure. Independently of that, the plugin is built so that
you can act on your own data the way the GDPR expects.

Last reviewed: 2026-05-05 (HellionChat v1.0.3).

---

## TL;DR

- All chat data the plugin stores stays on your machine, in your
  Dalamud `pluginConfigs/HellionChat/` directory.
- The plugin does not phone home. There is no telemetry, no analytics,
  no crash reporter, no usage counter, no remote update check beyond
  what Dalamud itself does.
- Two outbound network calls exist by design: the BetterTTV emote
  service (for chat emotes) and the Square Enix Lodestone font CDN
  (for the in-game symbol font). Both are documented in detail below
  and both can be reasoned about per request.
- You can export every message the plugin has stored, in Markdown,
  JSON or CSV, and you can wipe stored history per channel, per date
  range, or globally.

---

## What the plugin stores locally

HellionChat keeps three kinds of state on your machine, all under
`%appdata%\XIVLauncher\pluginConfigs\HellionChat\` on Windows
(`~/.xlcore/pluginConfigs/HellionChat/` on Linux/macOS via XIVLauncher
Core):

1. **Configuration** (`HellionChat.json`)
   Plugin settings, channel whitelist, retention values, layout state,
   theme colours. Contains no chat content.

2. **Message database** (SQLite file in the same directory)
   Chat messages from the channels on your whitelist, stored as
   MessagePack-encoded blobs. Default whitelist out of the box covers
   only your own conversations: tells, party, free company, linkshells,
   cross-world linkshells, alliance, ExtraChat. Public chat, NPC
   dialogue, system messages and battle logs are dropped on the
   storage layer and never written to disk.

3. **Cached emote images** (`EmoteCacheV1/` directory)
   Image files downloaded from BetterTTV when an emote appears in a
   message you receive. See "Outbound network calls" below.

There is no shared state with the upstream Chat 2 plugin.
`pluginConfigs/HellionChat/` is independent from `pluginConfigs/ChatTwo/`.

### Retention defaults

- Tells: 365 days
- Your-conversation channels (party, FC, linkshells, cross-world LS,
  alliance, ExtraChat): 90 days
- Global default for anything else: 30 days

**Retention is off by default.** The plugin does not delete anything
on its own until you explicitly turn the retention sweep on in the
settings. Until then, stored messages stay until you clear them.

---

## What the plugin does not store

- Public chat (`/say`, `/yell`, `/shout`), NPC dialogue, system
  messages and battle logs. These are filtered before they reach the
  storage layer.
- Anything from channels you remove from the whitelist. The privacy
  filter runs on the way in, not on the way out.
- Login credentials, character IDs, account IDs. The plugin uses
  whatever Dalamud already exposes about the local character to
  attribute messages; nothing of that is sent anywhere or persisted
  beyond the message itself.

---

## Outbound network calls

HellionChat makes two kinds of automatic outbound network requests.
Both are inherited from upstream Chat 2 and both are documented here
because "DSGVO-by-design" means you should know what your client does
on your behalf.

### 1. BetterTTV emote service (`api.betterttv.net`, `cdn.betterttv.net`)

- **What it does:** When a chat message arrives that references a
  BetterTTV emote, the plugin asks the BetterTTV API for the emote
  metadata and downloads the image from the BetterTTV CDN to display
  it inline.
- **What is sent:** A standard HTTPS GET request. Your IP address
  reaches BetterTTV (unavoidable for any HTTPS request); the request
  itself contains no identifying user data, no character name, no
  message text. Only the emote ID being looked up is in the URL path.
- **When it triggers:**
  - The emote *list* (global emotes plus the top-1500 community emotes
    over fifteen API pages) is fetched from `api.betterttv.net` once
    per session at plugin startup, provided the **Show emotes** option
    is on. This first list-fetch happens before any chat message has
    arrived; BetterTTV's edge therefore sees your IP as soon as the
    plugin loads, not only after an emote is mentioned.
  - The individual emote *images* on `cdn.betterttv.net` are fetched
    on demand, only when an incoming chat message contains a token
    matching one of the cached IDs. These are cached locally
    (`emoteCache/`) and reused across sessions.
- **Cached:** Yes, in `emoteCache/`. A given emote is downloaded once
  per machine and reused.
- **How to opt out:** Turn off the **Show emotes** option in
  Settings → Chat. With it disabled, the emote cache does not load
  and no requests to BetterTTV are made for the rest of the session.
- **BetterTTV's privacy policy:** <https://betterttv.com/privacy>

Source: `HellionChat/EmoteCache.cs`.

### 2. Square Enix Lodestone font (`img.finalfantasyxiv.com`)

- **What it does:** Downloads the `FFXIV_Lodestone_SSF.ttf` font file
  from the official Square Enix Lodestone CDN once during font setup,
  so the plugin can render in-game special symbols (job icons, item
  glyphs, etc.) inside ImGui.
- **What is sent:** A single HTTPS GET request to the public Square
  Enix font URL. Your IP address reaches Square Enix (unavoidable);
  no character data, no plugin identifier, no message content.
- **When it triggers:** Once per font initialisation, not per session
  if the file is already cached locally.
- **Cached:** Yes, by Dalamud's font subsystem.
- **How to opt out:** This call is part of the font pipeline inherited
  from upstream Chat 2 and not toggleable from the settings UI today.
  If a user-facing opt-out for this would be useful for you, please
  open a feature-request issue.

Source: `HellionChat/FontManager.cs`.

### Links you click yourself (no automatic traffic)

The settings panel contains a few buttons that open external pages in
your browser when you click them: the upstream Chat 2 GitHub repo,
the upstream maintainers' Ko-fi pages, the HellionChat issue tracker
and `hellion-media.de`. Nothing happens until you click. They are
documented here for completeness, not because they generate background
traffic.

---

## What the plugin does not do

- **No telemetry.** Source verified: no calls to AppInsights, Sentry,
  PostHog, Plausible, Google Analytics, Microsoft Clarity or any
  comparable service exist in the codebase, nor in the direct
  dependencies the plugin pulls in. See `docs/THIRD_PARTY_NOTICES.md`.
- **No crash reporting.** Crashes go to Dalamud's local `xllog`,
  not to a remote endpoint controlled by HellionChat.
- **No usage counters.** The plugin does not count installs, sessions,
  feature usage, channel activity or anything else for the maintainer.
- **No phone-home update check.** Updates are delivered through
  Dalamud's plugin installer, which polls the custom-repo
  `repo.json` on GitHub. That is GitHub's traffic and falls under
  GitHub's privacy policy; the plugin code does no separate update
  check.
- **No background sync.** Messages stay on your machine. There is no
  cloud backup, no sharing feature, no remote viewer.

---

## Your data, your rights

The GDPR gives you specific rights over data about you. Because
HellionChat stores everything locally, those rights translate
directly into plugin features:

### Right to access (Art. 15)

Use the export feature in the plugin settings. You can export to
**Markdown**, **JSON** or **CSV**, filtered by channel, date range
or sender substring. The export goes through a Dalamud file dialog
and writes wherever you point it, on your machine.

### Right to erasure (Art. 17)

Two options:

1. **Targeted deletion** — the "retroactive cleanup" feature lets you
   apply your current whitelist to the existing database. It shows a
   preview of what will be removed before you confirm with
   Ctrl+Shift, runs in the background, and calls `VACUUM` afterwards
   to actually shrink the file.
2. **Full deletion** — close the game and delete the
   `pluginConfigs/HellionChat/` directory. Next plugin start will
   produce a fresh, empty configuration.

### Right to portability (Art. 20)

The JSON and CSV exports are open formats. The Markdown export is
human-readable and machine-parseable. Nothing is locked into a
proprietary container.

### Right to object / restrict processing (Art. 21, 18)

Adjust the channel whitelist or set retention to a low value. Both
take effect immediately on new messages; existing data needs the
retroactive cleanup to apply retroactively, by design.

---

## Third parties involved

| Party | Why they appear | What reaches them | Their privacy policy |
| --- | --- | --- | --- |
| BetterTTV (NightDev LLC) | Optional emote rendering | HTTPS request for an emote ID; your IP | <https://betterttv.com/privacy> |
| Square Enix | Lodestone font download (once) | HTTPS request for the font file; your IP | <https://www.square-enix.com/privacy> |
| GitHub (Microsoft) | Plugin distribution via custom repo, issue tracker | Whatever GitHub sees from any HTTPS request to a public repo | <https://docs.github.com/site-policy/privacy-policies/github-general-privacy-statement> |
| Dalamud / XIVLauncher (goatcorp) | Plugin loader, font subsystem, repo polling | Whatever Dalamud reports for itself; out of HellionChat's scope | <https://github.com/goatcorp/Dalamud> |

Square Enix and GitHub are unavoidable for anyone playing FFXIV
through Dalamud at all. BetterTTV is the only third party HellionChat
introduces on top of the baseline that is not also part of using FFXIV
or Dalamud, and BetterTTV is opt-out via settings.

---

## Dependencies that touch the network

For a full dependency inventory see `docs/THIRD_PARTY_NOTICES.md`. Of the
direct dependencies the plugin pulls in:

- `MessagePack` — local serialisation, no network.
- `Microsoft.Data.Sqlite` — local SQLite access, no network.
- `morelinq` — LINQ helpers, no network.
- `Pidgin` — parser combinators, no network.
- `SixLabors.ImageSharp` — image decoding (used for the BetterTTV
  emote pipeline), no network on its own.

The two network calls listed under "Outbound network calls" are
written directly in HellionChat's own source, not delegated to a
dependency.

---

## Changes to this notice

If a future release changes what HellionChat stores, sends or caches,
this document will be updated and the change called out in the
changelog block of that release. The "Last reviewed" date at the top
tracks the version this document is accurate for.

---

## Questions

For privacy-related questions specific to HellionChat:

- Email: `kontakt@hellion-media.de`
- Discord DM: `@j.j_kazama`

Security-relevant findings (e.g. the plugin storing or sending
something this document says it does not) go through the private
advisory in `SECURITY.md`, not a public issue.
