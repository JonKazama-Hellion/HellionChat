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

## v1.2.0 — Layout Refresh (2026-XX-XX)

### Added
- Sidebar tab modernization: icon-only at fixed 44 px, tooltip on hover, vertical accent pill for active tab
- Top tabs: accent underline pill replaces background fill on active tab
- Per-tab custom icons in Settings → Tabs (15-glyph FontAwesome picker)
- Bottom status bar (22 px): channel indicator, privacy badge, counters, tells, version — updates 1×/sec
- Card rows as default message render: sender header in channel color, subtle border between cards
- Compact-Density toggle in Appearance: switches back to single-line `[HH:mm] Sender: Text` layout

### Changed
- Migration v14 → v15: deprecated Configuration fields `HellionThemeEnabled` and `HellionThemeWindowOpacity` removed
- Appearance settings cleaned: legacy theme-engine bindings replaced by Themes tab (introduced in v1.1.0)

### Notes
- Polish phase (animations, theme crossfade, header quick-picker) follows in v1.3.0
- Top-Tab icon prefixes were considered but dropped: Dalamud's default font atlas does not include FontAwesome codepoints, so mixed-font in a single TabItem label renders as tofu. Underline pill alone is the v1.2.0 visual treatment. Resolution would require Font-Atlas merge at FontManager level — out of scope.

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
