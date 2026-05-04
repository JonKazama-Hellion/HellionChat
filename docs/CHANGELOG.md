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
