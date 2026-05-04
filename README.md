# Hellion Chat

[![Build](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/build.yml)
[![CodeQL](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/JonKazama-Hellion/HellionChat/security/code-scanning)
[![License: EUPL-1.2](https://img.shields.io/badge/License-EUPL--1.2-blue.svg)](LICENSE)
[![Latest release](https://img.shields.io/github/v/release/JonKazama-Hellion/HellionChat?display_name=tag&sort=semver&color=brightgreen)](https://github.com/JonKazama-Hellion/HellionChat/releases/latest)
[![Dalamud API](https://img.shields.io/badge/Dalamud-API_15-purple)](https://github.com/goatcorp/Dalamud)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![FFXIV](https://img.shields.io/badge/FFXIV-Dawntrail-c3a37f)](https://www.finalfantasyxiv.com/)

**Version 1.0.0** — DSGVO-bewusster Chat-Ersatz für FINAL FANTASY XIV / Dalamud, basierend auf [Chat 2](https://github.com/Infiziert90/ChatTwo) (EUPL-1.2).

Hellion Chat ergänzt das ursprüngliche Chat-2-Fundament um Datenschutz- und Daten-Handling-Kontrollen, die mit den Datenschutz-Regeln in der EU, den USA und Japan im Einklang sind. Alle aus Chat 2 übernommenen Funktionen, Befehle und Tastenkürzel funktionieren unverändert. Eigenständiger Plugin-Slot, eigene Konfiguration, eigene Datenbank.

Eigenständiges Repository, EUPL-1.2-lizenziert. Mit v1.0.0 ist der Standalone-Cut abgeschlossen: eigener Namespace `HellionChat.*`, eigene IPC-Kanäle, eigene Source-Tree-Struktur. Distribution über Custom-Repo. Selektive Cherry-Picks von Upstream-Chat-2 nach Bedarf, dokumentiert in [`docs/UPSTREAM_SYNC.md`](docs/UPSTREAM_SYNC.md).

## Acknowledgements

Hellion Chat baut auf [Chat 2](https://github.com/Infiziert90/ChatTwo) von **Infiziert90 (Infi)** und **Anna Clemens** auf, die das Plugin über Jahre gepflegt haben bevor ich den Source-Code überhaupt gesehen habe. Die ganze Kern-Architektur, der Message-Store, die Channel-Logik, das Hook-System und vieles mehr stammt von ihnen. Wenn dir Hellion Chat hilft, dann läuft die Anerkennung dafür zu großen Teilen an Infi und Anna. Eine ausführliche Danksagung liegt in [NOTICE.md](NOTICE.md).

---

## Tech Stack

| Kategorie       | Technologie                                          |
| --------------- | ---------------------------------------------------- |
| Plattform       | Dalamud Plugin (API Level 15)                        |
| Sprache         | C# 13 / .NET 10 (`net10.0-windows`)                  |
| Build           | Dalamud.NET.Sdk 15.0.0, DalamudPackager 15.0.0       |
| UI              | Dear ImGui (Dalamud-Bindings)                        |
| Datenbank       | SQLite (Microsoft.Data.Sqlite, MessagePack-Storage)  |
| Lokalisierung   | ResX (HellionStrings.resx, .de.resx; PR-basiert)     |
| Schriftart      | Exo 2 (SIL Open Font License 1.1, gebündelt)         |
| Toolchain       | dotnet 10 SDK, VS Code mit C# Dev Kit                |
| Deployment      | GitHub Releases + Custom-Repo (`repo.json`)          |

---

## Features

### Privacy / Compliance

- **Channel-Whitelist** für die Datenbank-Persistenz mit Privacy-First-Default. Out-of-the-box werden nur eigene Konversationen gespeichert (Tells, Gruppe, FC, Linkshells, Cross-World-Linkshells, Allianz, ExtraChat). Öffentlicher Chat, NPC-Dialoge, System-Spam und Battle-Logs werden auf der Storage-Ebene verworfen.
- **Aufbewahrungsdauer pro Kanal** mit täglicher Background-Bereinigung. Tells 365 Tage, eigene Konversations-Kanäle 90 Tage, globaler Default 30 Tage. Standard ist AUS, das Plugin löscht ohne ausdrückliche Zustimmung nichts.
- **Retroaktive Säuberung** mit Vorschau und Strg+Umschalt-Bestätigung. Wendet die aktuelle Whitelist auf eine bestehende Datenbank an, läuft im Hintergrund, ruft danach VACUUM auf.
- **Export** nach Markdown, JSON oder CSV via Dalamud-Datei-Dialog (DSGVO Art. 15 Auskunftsrecht). Filter nach Kanal, Datums-Bereich oder Sender-Substring.
- **Vollständige Datenschutz-Übersicht** in [`PRIVACY.md`](PRIVACY.md) und Drittanbieter-Komponenten in [`docs/THIRD_PARTY_NOTICES.md`](docs/THIRD_PARTY_NOTICES.md): was gespeichert wird, welche zwei Outbound-Calls existieren (BetterTTV opt-out, Square-Enix-Lodestone-Font), explizite Telemetry-None-Zusage und das Mapping der DSGVO-Rechte (Art. 15/17/18/20/21) auf konkrete Plugin-Funktionen.

### Onboarding

- **First-Run-Wizard** mit drei Profilen (Privacy-First, Locker, Volle Historie) und DSGVO-Hinweis bei der "Volle Historie"-Option.
- **Konfigurations-Migration** seedet Privacy-Defaults bei Bestands-Usern und zeigt eine Benachrichtigung beim ersten Plugin-Start nach Update. Mit v1.0.0 wird zusätzlich für User auf Config-Version 12 oder älter ein einmaliger Tab-Layout-Reset durchgeführt; die alte Tab-Konfiguration wird als `pluginConfigs/HellionChat.json.pre-v13-backup` gesichert.
- **Layout-Migration aus Chat 2** verschiebt Konfiguration und Datenbank in `pluginConfigs/HellionChat/` ohne Datenverlust. Robust gegen blockierte Dateien (Warnung beim User wenn Chat 2 noch geladen ist).
- **Migrate3-Recovery** heilt halb-migrierte Datenbanken aus alten Chat-2-Installationen.

### Look & Feel

- **Bilinguale UI** (Englisch + Deutsch) mit Live-Sprachwechsel. Hellion-spezifische Strings in `HellionStrings.<lang>.resx`.
- **Hellion-HUD-Theme** mit Cyan-Teal-Akzenten, Slate-Violet-Tabs, Bernstein-Highlights für aktive Zustände.
- **Chat-Farben-Presets** (v0.6.0) mit sieben Built-in-Bundles in Settings → Aussehen → Chat-Farben: Klassik (Chat 2 Default), High-Contrast, Pastell, Dark-Mode-Tuned, Hellion (Brand), plus Bonus-Stimmungen Night Blue und Indigo Violet. One-Click-Apply, Battle-Channels bleiben unangetastet.
- **Fenster-Deckkraft-Slider** für Kampf-freundliche Transparenz.
- **Mitgelieferte Hellion-Schrift** (Exo 2, OFL-1.1) als optionaler Default statt System-Font.
- **Hellion-Logo** im Plugin-Bundle und in der Dalamud-Plugin-Liste.

### Pop-Out Convenience (v0.6.0)

- **Eingabe-Bar in Pop-Out-Fenstern** als globaler Opt-In in Settings → Fenster → Fenster-Rahmen. Wenn aktiv hat jedes Pop-Out-Window unten einen kompakten Input mit kanal-farbigem Icon-Button und Text-Eingabe — kein Wechsel mehr ins Hauptfenster für eine schnelle Antwort.
- **Pro-Pop-Out unabhängiger Text-Buffer und History-Cursor.** Channel-Wechsel im Pop-Out wirkt global wie im Hauptfenster (FFXIV-Channel-API), aber halb-getippte Eingaben kollidieren nicht zwischen Hauptfenster und Pop-Outs.
- **Geteilte Eingabe-Historie** zwischen allen Fenstern via Singleton-Service — Up/Down-Pfeile navigieren überall durch dieselbe Liste der letzten 30 Eingaben.

### Stability

- BetterTTV-Cache-Crash-Fix (Null-Key-Handling).
- Font-Atlas-Build-Fallback bei nicht-installierten System-Fonts.
- Defensive Wrapping aller Migrations-Operationen.

### Was gegenüber Chat 2 fehlt

- **Webinterface** wurde in Hellion Chat 0.2.0 entfernt. Es bedient einen anderen Anwendungsfall als der Fokus dieses Forks, nämlich Remote-Zugriff auf den Chat von einem zweiten Gerät. An die kleineren Defaults dieses Forks anzupassen hätte einen erheblichen Umbau bedeutet, also ist es ersatzlos entfernt worden. Wer den vollen Funktionsumfang von Chat 2 möchte, ist mit dem Upstream-Plugin besser bedient. Hellion Chat fokussiert sich auf einen schmaleren Datenbestand und verzichtet bewusst auf Remote-Zugriffs-Features.

---

## Architektur

```
HellionChat/
├── Privacy/
│   └── PrivacyDefaults.cs       # Whitelist-Sets, Spec-Retention-Tabelle
├── Export/
│   └── MessageExporter.cs       # Markdown / JSON / CSV Serializer
├── Resources/
│   ├── HellionStrings.resx      # Hellion-eigene UI-Strings (EN)
│   ├── HellionStrings.de.resx   # Deutsche Übersetzung
│   ├── HellionStrings.Designer.cs # Hand-maintained Accessor
│   ├── ChatColourPresets.cs     # Sieben Built-in-Color-Presets (v0.6.0)
│   ├── HellionFont.ttf          # Exo 2 Variable Font
│   ├── HellionFont-OFL.txt      # OFL-1.1 Lizenztext (mit Font gebundelt)
│   └── Language*.resx           # Upstream-Lokalisierung (Crowdin)
├── Ui/
│   ├── FirstRunWizard.cs        # Drei-Profile-Onboarding
│   ├── HellionStyle.cs          # ImGui-Theme-Push (lokal + global)
│   └── SettingsTabs/
│       └── Privacy.cs           # Datenschutz-Tab (Filter, Retention, Cleanup, Export)
├── Ipc/                         # IPC-Kanäle, in v1.0.0 auf HellionChat.* migriert
├── ChatTwoConflictDetector.cs   # Verweigert Plugin-Start wenn Upstream Chat 2 aktiv
├── images/
│   └── icon.png                 # Hellion-Logo (256×256)
├── HellionChat.csproj           # SDK Dalamud.NET.Sdk/15.0.0
└── HellionChat.yaml             # Plugin-Manifest (DalamudPackager-Source)
```

### Regeln

- **Code-Namespace ist `HellionChat.*`** — seit v1.0.0 vollständig konsolidiert auf den Plugin-Namen, kein verbleibender `ChatTwo.*`-Bestand im Source-Tree.
- **AssemblyName ist `HellionChat`** — eigener Slot in `pluginConfigs/`, eigenes Datei-Manifest, kein Shared State mit Chat 2. Parallel-Load mit Upstream Chat 2 wird beim Start aktiv geblockt (bilinguale Konflikt-Meldung).
- **IPC-Kanäle sind `HellionChat.*`** — sechs Kanäle für Drittplugin-Anbindung (`Register`, `Available`, `Unregister`, `Invoke`, `GetChatInputState`, `ChatInputStateChanged`). Details in [`docs/IPC.md`](docs/IPC.md).
- **Hellion-eigene Strings in `HellionStrings.*.resx`**, übernommene Strings aus dem Chat-2-Bestand in `Language.*.resx` — die Original-`Language.*.resx` bleibt strukturell erhalten, weil die existierenden Übersetzungen aus dem Crowdin-Bestand der Upstream-Community weiter wertvoll sind.
- **Kein Direkt-Eingriff in `Plugin.Interface.UiBuilder.FontAtlas`** außerhalb von `FontManager` — Font-Fallback und Hellion-Font laufen zentral.

---

## Datenbank

SQLite, Schema von Upstream Chat 2 übernommen (Migration-Stand v3). Hellion-Erweiterungen sind in `Configuration` als Felder, nicht im DB-Schema:

| Spalte           | Typ      | Beschreibung                                  |
| ---------------- | -------- | --------------------------------------------- |
| Id               | BLOB     | Guid                                          |
| Receiver         | INTEGER  | Empfänger-ContentId                           |
| ContentId        | INTEGER  | Sender-ContentId                              |
| Date             | INTEGER  | Unix-Timestamp (ms)                           |
| ChatType         | INTEGER  | XivChatType / LogKind                         |
| SourceKind       | INTEGER  | Player / NPC / Server / etc.                  |
| TargetKind       | INTEGER  | Player / NPC / Server / etc.                  |
| Sender           | BLOB     | MessagePack `List<Chunk>`                     |
| Content          | BLOB     | MessagePack `List<Chunk>`                     |
| SenderSource     | BLOB     | MessagePack `SeString`                        |
| ContentSource    | BLOB     | MessagePack `SeString`                        |
| ExtraChatChannel | BLOB     | Guid                                          |
| Deleted          | BOOLEAN  | Soft-Delete-Marker                            |

Pfad: `pluginConfigs/HellionChat/chat-sqlite.db`. WAL-Modus, Synchronous=NORMAL.

---

## Installation (Tester)

Hellion Chat wird während der Bootstrap-Phase über ein Dalamud-**Custom-Repository** verteilt.

### Frische Installation (kein Chat 2 vorher)

1. Dalamud-Settings (`/xlsettings`) → **Experimental** öffnen.
2. Neuen Eintrag unter **Custom Plugin Repositories** anlegen:
   ```
   https://raw.githubusercontent.com/JonKazama-Hellion/HellionChat/main/repo.json
   ```
3. **Save**, dann in `/xlplugins` → **All Plugins** → Refresh.
4. **Hellion Chat** taucht in der Liste auf — installieren.

### Migration aus Chat 2 (mit bestehendem Verlauf)

Chat 2 und Hellion Chat teilen sich die Datenbank-Datei, bis Hellion Chat sie beim ersten Start in den eigenen Pfad verschiebt. Die Reihenfolge ist wichtig:

1. **Chat 2 deaktivieren** in `/xlplugins` (nicht deinstallieren, nur deaktivieren).
2. **FFXIV komplett schließen**, damit SQLite die Datei-Sperre freigibt. Plugin-Reload allein reicht nicht.
3. Spiel neu starten.
4. Custom-Repo wie oben hinzufügen.
5. Hellion Chat installieren. Beim ersten Start wandert die Konfigurations-Datei und das gesamte Datenbank-Verzeichnis in das HellionChat-Layout.
6. **Verifizieren** unter Einstellungen → Datenschutz → Vorschau aktualisieren, dass die Nachrichten-Anzahl plausibel ist.

### Troubleshooting

**Hellion Chat zeigt 0 Nachrichten, obwohl Chat 2 vorher aktiv war:**

Migration wurde durch eine gesperrte Datei blockiert. Spiel schließen und manuell verschieben:

Linux / XIVLauncher Core:
```bash
mv ~/.xlcore/pluginConfigs/ChatTwo/chat-sqlite.db \
   ~/.xlcore/pluginConfigs/HellionChat/chat-sqlite.db
[ -d ~/.xlcore/pluginConfigs/ChatTwo/EmoteCacheV1 ] && \
  mv ~/.xlcore/pluginConfigs/ChatTwo/EmoteCacheV1 \
     ~/.xlcore/pluginConfigs/HellionChat/
```

Windows / XIVLauncher:
```powershell
Move-Item "$env:AppData\XIVLauncher\pluginConfigs\ChatTwo\chat-sqlite.db" `
          "$env:AppData\XIVLauncher\pluginConfigs\HellionChat\chat-sqlite.db" -Force
```

Spiel starten, Hellion Chat aktivieren, Verlauf ist zurück.

### Updates

Updates erscheinen automatisch in der Plugin-Liste, sobald ein neuer `v0.X.Y`-Tag mit GitHub-Release publiziert ist. Keine Neu-Installation nötig.

---

## Distribution

Hellion Chat wird über ein eigenes Dalamud-Custom-Repository verteilt
(`repo.json` im Repo-Root). Tag-Pushes auf `vX.Y.Z` lösen den
[`release.yml`](.github/workflows/release.yml)-Workflow aus, der den
Build-Output (`HellionChat/bin/Release/HellionChat/latest.zip`) plus den
passenden Changelog-Block aus `HellionChat.yaml` an das GitHub-Release
hängt. Manueller Recovery-Pfad bei verpasstem Auto-Trigger:
`gh workflow run release.yml -f tag=vX.Y.Z`.

Eine optionale Submission ans Dalamud-Main-Plugin-Repo (zusätzlich zum
eigenen Custom-Repo) steht in der [Roadmap](docs/ROADMAP.md).

---

## Projektstatus

**Version 1.0.0** — Standalone-Cut live (Stand: 2026-05-04).

Mit v1.0.0 ist Hellion Chat ein eigenständiges Plugin, kein Fork mehr im
Repository-Sinne. Vollständig abgeschlossen:

- Privacy-Filter (Whitelist, Retention, retroaktive Cleanup, Export)
- First-Run-Wizard mit drei Profilen
- Plugin-Identity: eigener `HellionChat`-Slot, Layout-Migration aus Chat 2, Migrate3-Recovery
- Bilinguale UI (EN + DE) mit Live-Sprachwechsel
- Hellion-Theme, Hellion-Logo, gebündelter Exo-2-Font
- Custom-Repo-Pipeline mit automatisierter GitHub-Release-Distribution
- Slash-Commands auf die `/hellion`-Familie konsolidiert
- Webinterface entfernt (v0.2.0)
- Audit-Hardening (Path-Traversal, Retention-Race, DbViewer-Konsistenz)
- About-Tab im Hellion-Branding, EN + DE lokalisiert, mit License und Disclaimer
- AI-Disclosure dokumentiert (siehe [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md))
- Standalone-Cut: Namespace `HellionChat.*`, IPC-Kanäle `HellionChat.*`, Source-Tree-Restructure, Conflict-Detection gegen Upstream Chat 2, SQLite-CVE-Härtung (3.50.3)

Was als Nächstes geplant ist und welche Themen langfristig auf der Liste
stehen, steht in [`docs/ROADMAP.md`](docs/ROADMAP.md). Konkrete
eingeplante Items werden zusätzlich im
[GitHub-Issue-Tracker](https://github.com/JonKazama-Hellion/HellionChat/issues)
mit dem `roadmap`-Label geführt.

### Zur Release-Kadenz

Wer den Repo zum ersten Mal sieht, bemerkt schnell viele Releases und sehr viele Commits in kurzer Zeit. Beides ist eine bewusste Entscheidung, keine KI-Slop-Symptomatik: Vorarbeit vor dem Fork (Issues und Commits gelesen, Chat 2 ingame genutzt), eine sauber strukturierte Upstream-Codebase als Fundament, atomare Commits im Stil des Upstream und AI-gestütztes Review-Sparring, das ich nicht blind übernehme. Die volle Begründung steht in [`docs/LEARNING-JOURNEY.md`](docs/LEARNING-JOURNEY.md), Sektion "Wie ich so schnell release".

---

## Community & Support

- **Hellion Forge Discord** (Modding- und Plugin-Community von Hellion Online Media): https://discord.gg/X9V7Kcv5gR
- Bug-Reports und Feature-Requests: [GitHub Issues](https://github.com/JonKazama-Hellion/HellionChat/issues)
- Weitere Kontaktwege (Security, Privacy, Quick-Questions): siehe [SUPPORT.md](SUPPORT.md)

---

## Lizenz

EUPL-1.2 (gleiche Lizenz wie Upstream Chat 2). Volltext in [LICENSE](LICENSE), Copyright-Notes mit Dual-Holder-Block in [COPYRIGHT](COPYRIGHT), persönliche Danksagung an die Upstream-Autoren in [NOTICE.md](NOTICE.md).

© 2023–2026 die Chat-2-Autoren (Infi, Anna und die Upstream-Contributors) für die Engine, IPC und Storage-Schicht.
© 2026 Hellion Online Media für die Hellion-Chat-Erweiterungen.

### Acknowledgments

- **Infi & Anna (ascclemens)** — die Chat-2-Engine, ohne die dieser Fork nicht existieren würde.
- **Dalamud-Team** — das Plugin-Framework.
- **Chat-2-Crowdin-Community** — Übersetzungen der Upstream-Strings (siehe Settings → Info → "Chat 2 community translators").

### FFXIV-Disclaimer

FINAL FANTASY XIV © SQUARE ENIX CO., LTD. Alle Rechte vorbehalten. Hellion Chat ist ein inoffizielles, von Fans erstelltes Plugin und ist weder mit Square Enix verbunden noch von ihnen unterstützt, gesponsert oder genehmigt.

### KI-Unterstützung

Siehe [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md) für die Pair-Level-Disclosure.

---

## Projekt-Dokumente

Im Repo-Root liegen die Standard-Repository-Dokumente, vertiefende
Dokumentation lebt unter [`docs/`](docs/).

### Repo-Root

| Dokument | Inhalt |
| --- | --- |
| [`PRIVACY.md`](PRIVACY.md) | Datenschutz-Übersicht: lokale Speicherung, Outbound-Calls, Telemetry-Status, DSGVO-Rechte und ihre Plugin-Entsprechungen. |
| [`SECURITY.md`](SECURITY.md) | Vulnerability-Reporting via Private Advisory, Scope und Disclosure-Fenster. |
| [`SUPPORT.md`](SUPPORT.md) | Wegweiser für Bugs, Security, Privacy, Quick-Questions. |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Was ich akzeptiere bzw. ablehne, Workflow, EUPL-1.2-Bestätigung. |
| [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md) | Verhaltens-Erwartungen und Reporting-Pfad. |
| [`NOTICE.md`](NOTICE.md) | Attribution an Upstream-Maintainer und Komponenten-Credits. |
| [`COPYRIGHT`](COPYRIGHT) | Copyright-Notes mit Dual-Holder-Block. |
| [`LICENSE`](LICENSE) | EUPL-1.2 Volltext. |

### `docs/`

| Dokument | Inhalt |
| --- | --- |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | Geplante Cycles, mittelfristige und langfristige Themen. |
| [`docs/CHANGELOG.md`](docs/CHANGELOG.md) | Kuratierte Versions-Übersicht mit Verweis auf die GitHub-Release-Pages. |
| [`docs/CONTRIBUTORS.md`](docs/CONTRIBUTORS.md) | Tester, Übersetzer und Code-Beiträger der Hellion-Seite. |
| [`docs/LEARNING-JOURNEY.md`](docs/LEARNING-JOURNEY.md) | Entwicklungsgeschichte, vom Web-Stack zu C# / Dalamud, was ich aus dem Fork gelernt habe. |
| [`docs/IPC.md`](docs/IPC.md) | IPC-Kanal-Reference, Tuple-Payload-Felder, Migrations-Diff für Drittplugins. |
| [`docs/UPSTREAM_SYNC.md`](docs/UPSTREAM_SYNC.md) | Cherry-Pick-Policy gegenüber Chat 2. |
| [`docs/THIRD_PARTY_NOTICES.md`](docs/THIRD_PARTY_NOTICES.md) | NuGet-Dependencies mit Lizenzen, Bundled Assets, Network-Status pro Komponente. |
| [`docs/AI_DISCLOSURE.md`](docs/AI_DISCLOSURE.md) | Offenlegung der KI-Unterstützung im Entwicklungsprozess. |

---

**Hellion Online Media** | Bad Harzburg | [hellion-media.de](https://hellion-media.de)
