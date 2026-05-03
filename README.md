# Hellion Chat

[![Build](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/build.yml)
[![CodeQL](https://github.com/JonKazama-Hellion/HellionChat/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/JonKazama-Hellion/HellionChat/security/code-scanning)
[![License: EUPL-1.2](https://img.shields.io/badge/License-EUPL--1.2-blue.svg)](LICENSE)
[![Latest release](https://img.shields.io/github/v/release/JonKazama-Hellion/HellionChat?display_name=tag&sort=semver&color=brightgreen)](https://github.com/JonKazama-Hellion/HellionChat/releases/latest)
[![Dalamud API](https://img.shields.io/badge/Dalamud-API_15-purple)](https://github.com/goatcorp/Dalamud)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![FFXIV](https://img.shields.io/badge/FFXIV-Dawntrail-c3a37f)](https://www.finalfantasyxiv.com/)

**Version 0.6.1** — DSGVO-bewusste Erweiterung von [Chat 2](https://github.com/Infiziert90/ChatTwo) für FINAL FANTASY XIV / Dalamud.

Hellion Chat baut auf Chat 2 auf und ergänzt es um Datenschutz- und Daten-Handling-Kontrollen, die mit den Datenschutz-Regeln in der EU, den USA und Japan im Einklang sind. Alle Chat-2-Funktionen, Befehle und Tastenkürzel funktionieren unverändert. Eigenständiger Plugin-Slot, eigene Konfiguration, eigene Datenbank.

Eigenständiges Repository, EUPL-1.2-lizenziert. Distribution über Custom-Repo. Selektive Cherry-Picks von Upstream-Chat-2 nach Bedarf, dokumentiert in [UPSTREAM_SYNC.md](UPSTREAM_SYNC.md).

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
- **Vollständige Datenschutz-Übersicht** in [`PRIVACY.md`](PRIVACY.md): was gespeichert wird, welche zwei Outbound-Calls existieren (BetterTTV opt-out, Square-Enix-Lodestone-Font), explizite Telemetry-None-Zusage und das Mapping der DSGVO-Rechte (Art. 15/17/18/20/21) auf konkrete Plugin-Funktionen.

### Onboarding

- **First-Run-Wizard** mit drei Profilen (Privacy-First, Locker, Volle Historie) und DSGVO-Hinweis bei der "Volle Historie"-Option.
- **Konfigurations-Migration v6→v7** seedet Privacy-Defaults bei Bestand-Usern und zeigt eine Benachrichtigung beim Ersten Plugin-Start nach Update.
- **Layout-Migration aus Chat 2** verschiebt Konfiguration und Datenbank in `pluginConfigs/HellionChat/` ohne Datenverlust. Robust gegen blockierte Dateien (Warnung beim User wenn Chat 2 noch geladen ist).
- **Migrate3-Recovery** heilt halb-migrierte Datenbanken aus alten Chat-2-Installationen.

### Look & Feel

- **Bilinguale UI** (Englisch + Deutsch) mit Live-Sprachwechsel. Hellion-spezifische Strings in `HellionStrings.<lang>.resx`.
- **Hellion-HUD-Theme** mit Cyan-Teal-Akzenten, Slate-Violet-Tabs, Bernstein-Highlights für aktive Zustände.
- **Chat-Farben-Presets** (v0.6.0) mit sieben Built-in-Bundles in Settings → Aussehen → Chat-Farben: ChatTwo Default, High-Contrast, Pastell, Dark-Mode-Tuned, Hellion (Brand), plus Bonus-Stimmungen Night Blue und Indigo Violet. One-Click-Apply, Battle-Channels bleiben unangetastet.
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
ChatTwo/
├── Privacy/
│   └── PrivacyDefaults.cs       # Whitelist-Sets, Spec-Retention-Tabelle
├── Export/
│   └── MessageExporter.cs       # Markdown / JSON / CSV Serializer
├── Resources/
│   ├── HellionStrings.resx      # Hellion-eigene UI-Strings (EN)
│   ├── HellionStrings.de.resx   # Deutsche Übersetzung
│   ├── HellionStrings.Designer.cs # Hand-maintained Accessor
│   ├── HellionFont.ttf          # Exo 2 Variable Font
│   ├── HellionFont-OFL.txt      # OFL-1.1 Lizenztext (mit Font gebundelt)
│   └── Language*.resx           # Upstream-Lokalisierung (Crowdin)
├── Ui/
│   ├── FirstRunWizard.cs        # Drei-Profile-Onboarding
│   ├── HellionStyle.cs          # ImGui-Theme-Push (lokal + global)
│   └── SettingsTabs/
│       └── Privacy.cs           # Datenschutz-Tab (Filter, Retention, Cleanup, Export)
├── images/
│   └── icon.png                 # Hellion-Logo (256×256)
├── DalamudPackager.targets      # Override für ImagesPath / HandleImages
└── HellionChat.yaml             # Plugin-Manifest (DalamudPackager-Source)
```

### Regeln

- **Code-Namespace bleibt `ChatTwo.*`** — Cherry-Picks von Upstream-Bugfixes bleiben damit konfliktarm.
- **AssemblyName ist `HellionChat`** — eigener Slot in `pluginConfigs/`, eigene Datei-Manifest, kein Shared State mit Chat 2.
- **Hellion-eigene Strings nur in `HellionStrings.*.resx`** — die Upstream-`Language.*.resx` bleiben unverändert, damit Cherry-Picks aus Chat 2 (inklusive deren Übersetzungs-Updates aus dem Upstream-Crowdin) konfliktarm bleiben.
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

## Entwicklung

### Voraussetzungen

- .NET 10 SDK (`10.0.104+`) und .NET 9 SDK (`9.0.115+` parallel)
- Dalamud-Hooks im XIVLauncher-`addon`-Verzeichnis
- VS Code mit C# Dev Kit (oder Rider, JetBrains)
- Linux: WireGuard-Mount für Test-Spiel-Setup falls Remote-DB

### Setup

```bash
git clone --recurse-submodules https://github.com/JonKazama-Hellion/HellionChat.git
cd HellionChat
git remote add upstream https://github.com/Infiziert90/ChatTwo.git

# Linux: DALAMUD_HOME exportieren falls Hooks nicht im Standardpfad
cp .env.example .env
set -a; source .env; set +a

dotnet build ChatTwo/ChatTwo.csproj
```

Output: `ChatTwo/bin/Debug/HellionChat.dll`. Den Ordner `ChatTwo/bin/Debug` in Dalamud unter Experimental → Dev Plugin Locations eintragen.

### Build-Konfigurationen

| Configuration | Output                                                | Zweck                            |
| ------------- | ----------------------------------------------------- | -------------------------------- |
| Debug         | `bin/Debug/HellionChat.dll`                           | Dev-Plugin-Loading               |
| Release       | `bin/Release/HellionChat/latest.zip` + Manifest       | Custom-Repo / GitHub Release     |

### Upstream-Sync

```bash
git fetch upstream
git log --oneline HEAD..upstream/main          # Welche Commits gibt es?
git cherry-pick -x <commit>                    # Selektiv übernehmen
```

Konflikte in Upstream-Sprach-Ressourcen (`Language.<lang>.resx`) kommen häufig vor, weil Upstream-Übersetzungen (über das Chat-2-Crowdin-Projekt, nicht unseres) regelmäßig nachkommen. Pragmatisch mit `git checkout --theirs` auflösen, da wir sie selbst nicht editieren.

---

## Distribution

| Phase           | Version       | Distribution                                       |
| --------------- | ------------- | -------------------------------------------------- |
| Bootstrap       | v0.1.x        | Eigenes Custom-Repo (`repo.json` im Repo-Root)     |
| Stable          | v1.0          | Eigenes Custom-Repo                                |
| Optional        | v1.1+         | Submission ans Dalamud-Main-Plugin-Repo (zusätzlich) |

`repo.json` wird beim Versions-Bump per Hand aus dem generierten `HellionChat.json` plus den GitHub-Release-Download-Links zusammengebaut. Skript-Automatisierung via GitHub Actions ist geplant aber noch nicht eingerichtet.

---

## Projektstatus

**Version 0.6.1** | Stand: 2026-05-03

Alle Bootstrap-Phasen abgeschlossen:

- [x] Privacy-Filter (Whitelist + Retention + Cleanup + Export)
- [x] First-Run-Wizard mit drei Profilen
- [x] Plugin-Identity (eigener Slot, Layout-Migration, Recovery)
- [x] Bilinguale UI (EN + DE) mit Live-Sprachwechsel
- [x] Hellion-Theme + Hellion-Logo + gebündelter Exo-2-Font
- [x] Custom-Repo-Pipeline mit GitHub-Release-Distribution
- [x] About-Tab im Hellion-Branding mit License + Disclaimer
- [x] AI-Disclosure dokumentiert (Pair-Klassifikation)
- [x] Webinterface entfernt (Phase 1.5, Audit-Konsequenz aus 2026-05-02)
- [x] Audit-Hardening Phase 2 (Path-Traversal, Retention-Race, DbViewer-Konsistenz, Privacy-Filter-Help-Text)
- [x] Slash-Commands auf `/hellion`-Familie umbenannt
- [x] Theme auf Hellion-Online-Media-Brand-Palette aligned (Arctic Cyan + Ember Orange)
- [x] About-Tab vollständig lokalisiert (EN + DE) mit Mission-Statement und neutraler Tonart

Phase 3 (offen, kein festes Datum):

- [ ] MySQL/MariaDB-Backend mit Drei-Stufen-Bestätigung
- [ ] PostgreSQL-Backend
- [ ] Encryption für sensible Channels (AES-256, lokaler Key)
- [ ] WireGuard-Network-Detection (optionaler Filter)
- [ ] libnotify-Integration (native Linux-Toasts)
- [ ] XDG-Compliance (komplex unter Wine)
- [ ] Hand-gezeichnetes Hellion-Logo (Platzhalter aus Hellion-Online-Media-Brand-Repo)
- [ ] GitHub-Actions für reproduzierbaren Build und automatischen `repo.json`-Sync
- [ ] Submission ans Dalamud-Main-Plugin-Repo

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

Siehe [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md) für die Pair-Level-Disclosure.

---

## Projekt-Dokumente

| Dokument | Inhalt |
| --- | --- |
| [`PRIVACY.md`](PRIVACY.md) | Datenschutz-Übersicht: lokale Speicherung, Outbound-Calls, Telemetry-Status, DSGVO-Rechte und ihre Plugin-Entsprechungen. |
| [`SECURITY.md`](SECURITY.md) | Vulnerability-Reporting via Private Advisory, Scope und Disclosure-Fenster. |
| [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md) | NuGet-Dependencies mit Lizenzen, Bundled Assets, Network-Status pro Komponente. |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Was ich akzeptiere bzw. ablehne, Workflow, Build-Anleitung, EUPL-1.2-Bestätigung. |
| [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md) | Verhaltens-Erwartungen und Reporting-Pfad. |
| [`SUPPORT.md`](SUPPORT.md) | Wegweiser für Bugs, Security, Privacy, Quick-Questions. |
| [`UPSTREAM_SYNC.md`](UPSTREAM_SYNC.md) | Cherry-Pick-Policy gegenüber Chat 2. |
| [`NOTICE.md`](NOTICE.md) | Attribution an Upstream-Maintainer und Komponenten-Credits. |
| [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md) | Offenlegung der KI-Unterstützung im Entwicklungsprozess. |

---

**Hellion Online Media** | Bad Harzburg | [hellion-media.de](https://hellion-media.de)
