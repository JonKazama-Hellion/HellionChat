# Changelog — Hellion Chat

Alle nutzersichtbaren Änderungen an Hellion Chat. Das Format orientiert
sich an [Keep a Changelog](https://keepachangelog.com/de/1.0.0/), die
Version-Nummern folgen [Semantischer Versionierung](https://semver.org/lang/de/).

Detaillierte Release-Notes pro Version stehen direkt am
[GitHub-Release](https://github.com/JonKazama-Hellion/HellionChat/releases)
und im Plugin-Changelog-Block (`HellionChat/HellionChat.yaml` →
`changelog:`). Diese Datei fasst die Releases als Überblick zusammen
und verlinkt für Details auf die Release-Pages.

---

## Hellion Chat 1.4.2 — ChatLog Frame-Hot-Path

Third sub-patch of the v1.4.x Polish Sweep series. Per-frame
allocations from the chat-log render path eliminated.

- `DrawMessages` card-mode hoists `theme`/`drawList`/`winLeft`/
  `winRight`/`borderColorAbgr` out of the per-message loop. About
  500 redundant calls per frame at 100 visible messages, multiplied
  by every pop-out window
- Auto-tell tab tint and icon use a per-tab cache. Hash
  computation and string allocation only happen when the tell
  target name or world drifts. `AutoTellTabTint` stays a pure
  hash helper; cache lives in a thin `TabTintCache` wrapper
- Status bar gates its tab aggregation behind the same
  one-second cache it already used for the format strings.
  LINQ `Sum` and `Count` replaced with a single `foreach` pass
  that runs on roughly 1 % of frames

Realistic frame-time recovery: 2-5 % in typical scenes, more
on pop-out-heavy setups because the card-border hoist scales
per window.

Modding & support: join Hellion Forge — https://discord.gg/X9V7Kcv5gR

Based on Chat 2 1.35.3 (upstream Infiziert90/ChatTwo, EUPL-1.2).

---

## Hellion Chat 1.4.1 — Theme Engine Performance

Second sub-patch of the v1.4.x Polish Sweep series. Heap
pressure from the theme engine's per-frame render path
removed, plus a tenth built-in theme and hardening for
the custom-theme hot-reload.

- Theme records carry a pre-computed ABGR-packed cache
  for every color slot; cache is filled when the theme
  is registered and refreshed defensively on every
  `Switch()`
- `HellionStyle.PushGlobal` reads ABGR values from the
  cache instead of calling `ColourUtil.RgbaToAbgr` per
  slot per frame; ~13 % render-time recovery measured
  in typical scenes (plan estimate was 2–6 %, real
  ~10–15 %)
- `ThemeRegistry` custom-theme reload distinguishes a
  recoverable file lock (editor mid-save) from a
  permanent IO failure; locked themes keep their
  last-known-good snapshot and retry on the next
  lookup instead of dropping out of the picker
- New built-in: **Synthwave Sunset** — Hot Magenta + Cyan
  on midnight violet, 80s neon-grid vibes; tenth theme
  in the picker
- Author credits refreshed: brand themes are credited
  as "Hellion Forge"; **Mint Grove** and **Forge
  Merchantman** now credited to **Carla Beleandis** as
  a community thanks

No schema bump, no user-visible behaviour change other
than smoother frames on GC-sensitive setups and one
additional colour option.

Modding & support: join Hellion Forge — https://discord.gg/X9V7Kcv5gR

Based on Chat 2 1.35.3 (upstream Infiziert90/ChatTwo, EUPL-1.2).

---

## Hellion Chat 1.4.0 — Critical Lifecycle Fixes

First sub-patch of the v1.4.x Polish Sweep series. Seven
known lifecycle and race bugs eliminated before any
performance refactor sits on top.

- MessageStore disposal no longer triggers GC.Collect
  globally; Pooling=false on the SQLite connection means
  there's nothing left to clean up by hand
- PendingMessage and RetentionSweep worker threads are
  explicitly marked IsBackground=true so the plugin domain
  can unload during XIVLauncher reload without waiting
  for them
- EmoteCache image and gif loaders moved from async-void
  to async Task with a shared task tracker, draining
  on Dispose so an in-flight load can no longer write
  to a disposed EmoteImages entry
- DisposeAsync 10s timeout now warns loudly instead of
  silently leaving the worker behind
- Plugin.Dispose flushes any pending DeferredSaveFrames
  before tearing services down, so settings changes
  made in the last few frames before disable are no
  longer lost
- The v13→v14 config migration now reads the pre-v13
  backup and carries HellionThemeWindowOpacity into the
  new WindowOpacity field instead of falling back to
  the default 0.85

Modding & support: join Hellion Forge — https://discord.gg/X9V7Kcv5gR

Based on Chat 2 1.35.3 (upstream Infiziert90/ChatTwo, EUPL-1.2).

---

## Hellion Chat 1.3.0 - Plugin Integrations: Honorific

First step on the plugin-integration roadmap. HellionChat now
listens to Honorific and shows your custom title in the chat
header. The slot auto-hides when Honorific is not installed,
when no custom title is active, or when you are using the
original FFXIV title.

- New "Integrations" settings tab
- Honorific integration with auto-detection and live updates
- "Coming soon" preview of the next five planned integrations:
  context menu actions, smart notifications, RP status block,
  ExtraChat channels, and quick DM compose
- Maintainer attribution buttons for Honorific repo and Caraxi
- New service-class pattern under HellionChat/Integrations/

Modding and support: join Hellion Forge - https://discord.gg/X9V7Kcv5gR

Based on Chat 2 1.35.3 (upstream Infiziert90/ChatTwo, EUPL-1.2).

---

## v1.2.3 — Theme Expansion (2026-05-06)

### Added
- Four new built-in themes:
  - **Night Blue** — Royal Blue on deep marine, cool tech-dashboard mood
  - **Indigo Violet** — Royal Violet on deep indigo with a turquoise-mint counter (aurora glitter feel)
  - **Forge Merchantman** — Patina bronze on workshop slate with warm amber counter (Hellion Forge identity)
  - **Hellion Spectrum** — Deuteran/Protan-safe channel colours using Wong/Okabe-Ito palette tones; channel identity (Tell pink, Yell yellow, Shout orange, Party blue, FC green) preserved while keeping every channel separable under red-green colour vision deficiency
- Built-in theme catalogue grown from five to nine

### Notes
- No engine changes, no settings touched, no migration
- Default theme unchanged (Hellion Arctic). Existing custom themes keep working.
- Hellion Spectrum covers the ~99 % of CVD cases that are red-green; a Tritan-safe variant could follow in a later cycle if there is demand.

---

## v1.2.1 — Settings Cleanup (2026-05-06)

### Changed
- Settings cards re-sorted thematically: 9 cards remain, each card has one clear job and a one-line subtitle.
- **Theme & Layout** (new) collects the theme picker, window frame style (title bar, sidebar, hide button, pop-out title bar) and the timestamp style options.
- **Fonts & Colours** (new) houses font choice, font size and per-channel chat colours.
- **Data Management** (new) collects retention windows, cleanup, export, the database viewer and the shift-click advanced tools.
- **Privacy** is now focused on the privacy filter alone.
- **Chat** absorbs the Auto-Tell-Tabs history-preload slider that used to live under Privacy.
- **General** groups the keybind mode under Input.

### Removed
- Legacy "Style override" option and the unused style-name field — both obsolete since the v1.1.0 themes engine.
- Legacy `WindowAlpha` slider — if you had it set, the value is auto-migrated to Theme & Layout → Window Style → Window Transparency.
- Unused `ShowThemeQuickPicker` schema field.

### Migration
- v15 → v16 with backup at `pluginConfigs/HellionChat.json.pre-v16-backup`.
- All other settings preserved unchanged.
- One-time toast on first start if Style override was previously active.

---

## v1.2.0 — Layout Refresh (2026-05-05)

### Added
- Sidebar tab modernization: icon-only at fixed 44 px, tooltip on hover, vertical accent pill for active tab
- Top tabs: accent underline pill replaces background fill on active tab
- Per-tab custom icons in Settings → Tabs (15-glyph FontAwesome picker)
- Bottom status bar (22 px): channel indicator, privacy badge, counters, tells, version — updates 1×/sec
- Card rows as default message render: sender header in channel color, subtle border between cards
- Compact-Density toggle in Appearance: switches back to single-line `[HH:mm] Sender: Text` layout
- Auto-Tell tabs: per-partner hashed icon (7-glyph pool: envelope/star/heart/bell/bookmark/flag/fire) plus hashed color (12-color palette) — 84 distinct icon+color combinations
- Unread indicator: pulsing red dot in the top-right corner of any sidebar tab icon with unread messages, 2-second sine-wave pulse, respects `Configuration.ReduceMotion`

### Changed
- Migration v14 → v15: deprecated Configuration fields `HellionThemeEnabled` and `HellionThemeWindowOpacity` removed
- Appearance settings cleaned: legacy theme-engine bindings replaced by Themes tab (introduced in v1.1.0)

### Fixed
- Settings save no longer wipes chat history by default — the heavy `ClearAllTabs + FilterAllTabsAsync` cycle now only runs when a filter-relevant setting actually changed (Privacy filter, persisted channels, per-tab channel selection). Cosmetic changes keep the in-session chat intact
- Identifier-based `MessageList` restore in `Configuration.UpdateFrom` plus TempTab skip in `ClearAllTabs`/`FilterAllTabs` ensure persistent tabs and Auto-Tell tabs both survive the save
- Sidebar buttons now align vertically with the first message row (top padding mirrors the chat header toolbar height)
- Sidebar child window no longer paints the top padding area with its frame background
- Status bar version slot (`vX.Y.Z · Hellion`) no longer clips its rightmost character

### Notes
- Polish phase (animations, theme crossfade, header quick-picker) follows in v1.3.0
- Top-Tab icon prefixes were considered but dropped: Dalamud's default font atlas does not include FontAwesome codepoints, so mixed-font in a single TabItem label renders as tofu. Underline pill alone is the v1.2.0 visual treatment for top tabs. Resolution would require Font-Atlas merge at FontManager level — out of scope.

## [1.1.0] — 2026-05-05 — Theme Foundation

Erster großer UI-Cycle nach v1.0.0. Theme-Engine, fünf Built-In-Themes,
Custom-Themes via JSON, Settings-Card-Grid.

### Hinzugefügt

- **Theme-Engine** mit fünf Built-In-Themes: Hellion Arctic (Default),
  Chat 2 Klassik, Event Horizon, Moonlit Bloom, Mint Grove.
- **Settings → Themes** mit Mini-Mockup-Preview pro Theme. Klick auf
  eine Card switcht sofort das ganze Plugin (Chat, Settings, Pop-Out).
- **Custom-Themes via JSON** in `pluginConfigs/HellionChat/themes/`.
  Beim ersten Start wird `example-theme.json` als Vorlage abgelegt.
- **Optional Theme-Chat-Channel-Colors**: Themes können eigene
  Channel-Farben mitliefern. Beim Switch erscheint ein Banner mit
  *Übernehmen / Behalten* — nie automatisch.
- **Settings-Card-Grid**: neue Übersicht beim Öffnen, Card-Klick führt
  in die Detail-Ansicht der Section. Breadcrumb + ESC führen zurück.
- **`docs/THEME-AUTHORING.md`** als Anleitung zum Schreiben eigener
  Themes, mit Hellion-Forge-Branding.

### Geändert

- **Plugin-Icon** auf Hellion Forge Hammer (vorher ChatTwo-Derivat).
- **Settings-Detail-View** verwendet die volle Breite — die zweite
  Tab-Liste links ist weg, weil die Card-Übersicht den Wechsel
  übernimmt.
- **`HellionStyle.PushGlobal`** ist jetzt theme-driven (`PushGlobal(theme,
  opacity)`) statt const-palette-driven.
- **Configuration v13 → v14**: alle User landen auf `hellion-arctic`.
  Wer den Upstream-Look will, wählt `chat2-classic` in Settings →
  Themes.

### Veraltet

- `Configuration.HellionThemeEnabled` und `HellionThemeWindowOpacity`
  bleiben für ein Release lesbar als Safety-Net, werden aber nicht
  mehr ausgewertet. Entfernung geplant in v1.2.0.

### Sicherheit

- Custom-Theme-JSON-Loader prüft `schemaVersion`, Pflichtfelder und
  Hex-Format. Ungültige Themes werden mit Warning übersprungen, das
  Plugin lädt mit Built-Ins weiter.

### Intern

- 51 lokale Unit-Tests (Theme-Records, Registry, JSON-Round-Trip,
  Sanity pro Built-In-Theme). Tests sind gitignored.

---

## [1.0.3] — 2026-05-04 — Polish patch

Vier kleine Polish-Items aus dem Backlog gebündelt:

- **Hide bei New Game+ Menü**: Optionaler globaler Toggle der Hellion
  Chat (und alle weiteren Plugin-Fenster wie Settings, DB-Viewer,
  Pop-Outs) ausblendet, solange das NG+-Menü offen ist. Settings →
  Fenster → Rahmen, Default aus. Skipt analog zum bestehenden
  LoadingScreens-Pattern den gesamten `WindowSystem.Draw()`-Pfad.
- **Channel-Selector-Färbung**: Optionales Tinting des
  Channel-Auswahl-Knopfs (Comment-Icon) neben dem Eingabefeld in der
  aktuellen Channel-Farbe. Settings → Aussehen → Chat-Farben, Default
  an. Konsistent zur bestehenden Eingabetext-Färbung, ExtraChat-Override
  wird übernommen.
- **(De)Buff-Icon Aspect-Ratio-Fix**: `PayloadHandler.InlineIcon` quetschte
  alle Hover-Icons auf 32×32. Status-Icons mit nicht-quadratischen
  Dimensionen (Debuffs mit Pfeil-Indikator) sind jetzt aspekt-erhaltend
  geshrinkt. Eigenständige Float-Math-Implementierung mit Zero-Size-Guard
  statt Cherry-Pick aus dem offenen ChatTwo PR #157 (der hatte eine
  int-Division-Falle).
- **HideState-Logging-Sweep**: Alle HideState-Transitions
  (Battle/Cutscene/User/Override plus die Pop-Out-Spiegelung) loggen sich
  auf Verbose-Level. Aus by default, Aktivierung via
  `/xllog set HellionChat verbose` für Bug-Report-Diagnose.

[Release-Notes 1.0.3](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v1.0.3)

## [1.0.1] — 2026-05-04 — Window Position Recovery

Fixes an off-screen-window scenario the user could end up in after a
monitor disconnect or display layout change between sessions. An
automatic one-shot bounds check on the first draw after plugin load
snaps the window back into the visible viewport, and a new
"Reset Window Position" button in Settings → Window → Frame serves as
the manual escape hatch for edge cases.

Bundled housekeeping since v1.0.0: documentation restructured into
`docs/`, stale ChatTwo/* paths in repo configs cleaned up, Pidgin
parser library bumped from 3.3.0 to 3.5.1, GitHub Actions bumps for
`actions/setup-dotnet` (4 → 5) and `github/codeql-action` (3 → 4).

[Release-Notes 1.0.1](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v1.0.1)

## [1.0.0] — 2026-05-03 — Standalone Major Release

Erste vollständig eigenständige Version. Code-Namespace, IPC-Kanäle und
Source-Tree-Struktur wurden auf `HellionChat.*` konsolidiert. Plugin
verweigert den Start bei aktivem Upstream Chat 2 (bilinguale
Konflikt-Meldung). SQLite-Native auf 3.50.3 gepinnt (CVE-2025-6965,
CVE-2025-7709). Tab-Layout-Default für neue Installationen und für
User auf Config-Version 12 oder älter neu strukturiert (5 thematische
Tabs statt 6+ kitchen-sink). Sweep aus Critical- und Major-Findings
aus dem Codebase-Audit eingearbeitet.

[Release-Notes 1.0.0](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v1.0.0)

## [0.6.1] — 2026-05-03 — Pop-Out Discoverability & /tell Auto-Pop-Out

Pop-Out-Button im Chat-Header sichtbar, einmaliger Hint-Banner für die
Pop-Out-Funktionalität. Neue Einstellung "Neue /tell-Tabs direkt als
Pop-Out öffnen". Pop-Out-Input ist jetzt standardmäßig aktiv.
Bugfixes: Ghost-Windows bei LRU-Drop / Logout, Dead-Zone unter dem
Input-Bar bei aktivem Hint-Banner.

[Release-Notes 0.6.1](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v0.6.1)

## [0.6.0] — 2026-05-03 — UX Polish: Pop-Out Input + Colour Presets

Zwei opt-in UX-Features. Pop-Out-Fenster bekommen optional eine
kompakte Eingabe-Bar mit channel-farbigem Icon-Button und unabhängigem
Text-Buffer pro Pop-Out. Sieben Built-in-Color-Presets (Klassik,
High-Contrast, Pastell, Dark-Mode-Tuned, Hellion, Night Blue, Indigo
Violet) zum One-Click-Apply. Konfigurations-Migration v10 → v11.

[Release-Notes 0.6.0](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v0.6.0)

## [0.5.4] — 2026-05-02 — WrapText Hardening

`ImGuiUtil.WrapText` von Pointer-Arithmetik auf Span- und
Index-basierten Control-Flow umgestellt. Schließt das wiederkehrende
CodeQL-Critical-Alert "unvalidated local pointer arithmetic"
dauerhaft. Keine nutzersichtbare Verhaltensänderung — Word-Wrap-Output
ist byte-identisch zu 0.5.3.

[Release-Notes 0.5.4](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v0.5.4)

## [0.5.3] — 2026-05-02 — Pointer Arithmetic Hardening

Erster Anlauf zur Schließung des CodeQL-Critical-Alerts in
`ImGuiUtil.WrapText`. Encoded-Byte-Buffer-Length wird vor der
Pointer-Arithmetik via `GetByteCount` validiert.

[Release-Notes 0.5.3](https://github.com/JonKazama-Hellion/HellionChat/releases/tag/v0.5.3)

---

## Frühere Versionen

Releases vor 0.5.3 (Bootstrap-Phase 0.1.0 bis 0.5.2) sind direkt am
GitHub-Release-Stream einsehbar:

[Alle Releases](https://github.com/JonKazama-Hellion/HellionChat/releases)

---

## Pflege-Hinweis

Die Source-of-Truth für den nutzersichtbaren Changelog ist der
`changelog:`-Block in `HellionChat/HellionChat.yaml`. `repo.json` und
der GitHub-Release-Body werden daraus gespeist. Diese Datei
(`docs/CHANGELOG.md`) ist eine kuratierte Zusammenfassung mit Verweis
auf die Release-Pages und wird beim Versions-Bump manuell ergänzt.
