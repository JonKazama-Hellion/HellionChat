# Hellion Chat — Roadmap

Geplante Arbeit nach dem v1.0.0 Standalone-Cut. Diese Liste ist absichtlich
grob: konkrete Specs, Größenschätzungen und Repro-Steps liegen im
internen Backlog. Tracking nach außen läuft über
[GitHub Issues](https://github.com/JonKazama-Hellion/HellionChat/issues)
mit dem `roadmap`-Label, sobald ein Item für einen Cycle eingeplant ist.

Reihenfolge ist Priorität, nicht Garantie. Items können sich verschieben
oder ganz wegfallen wenn sie sich beim Brainstorm als nicht passend zur
Privacy-First-Schnittmenge des Plugins erweisen.

---

## Nächster Cycle (v1.4.0)

**Polish & Motion** - Theme-Crossfade, Header-Quick-Picker,
Lerp-Animationen, ggf. Theme-Family-Picker (Carls Grün-Familie
mit Forest/Moss/Mint-Helligkeitsstufen).

Spec wird im Brainstorming-Cycle vor Beginn der Phase ausgearbeitet.

## v1.3.0 - Plugin Integrations: Honorific (geplant <Datum>)

Erster Cycle der Plugin-Integrations-Roadmap. Honorific-Custom-
Titles werden im Chat-Header angezeigt, mit Auto-Detect und
silent Fallback. Neuer Integrations-Settings-Tab. Pattern-
Etablierer für die fünf folgenden Cycles (Context-Menu,
NotificationMaster, RP-Status-Block, ExtraChat, XIVIM).

Spec: [Plugin-Integrationen-Übersicht](../Hellion%20Chat%20Plugin-Integrationen.md)

## v1.2.3 — Theme Expansion (released 2026-05-06)

Vier neue Built-In-Themes: Night Blue, Indigo Violet, Forge
Merchantman, Hellion Spectrum (Deuteran/Protan-safe).
Keine Engine-Änderungen. Siehe `docs/CHANGELOG.md`.

(v1.2.2 wurde verbrannt weil das `repo.json`-Manifest beim
ersten Push nicht synchron mitgebumpt wurde — Re-Release als
v1.2.3 mit kompletter Manifest-Synchronisation.)

## v1.2.1 — Settings Cleanup (released 2026-05-06)

Re-sortierte Settings (9 Cards thematisch), 4 tote Settings entfernt,
Auto-Migration v15 → v16 ohne Daten-Verlust.

## v1.2.0 — Layout Refresh (released 2026-05-05)

Top-Tabs-Refresh, Sidebar-Tab-Icons, Bottom-Status-Bar, Card-Rows als
Default-Message-Render, Auto-Tell-Tab-Hashing.

## v1.1.0 — Theme Foundation (released 2026-05-05)

Theme-Engine mit fünf Built-In-Themes, Settings-Card-Grid, Custom-
Themes via JSON, Theme-Authoring-Doku. Plugin-Icon auf Hellion Forge.
Siehe `docs/CHANGELOG.md` für Details.

Aus dem ursprünglichen v1.1.0-Plan (Ad-Block / Spam-Filter, Receive-
Suppressed-Tells-Toggle) wurden zugunsten der Theme-Engine zurück­
gestellt — beide Items leben weiter im Mittelfrist-Block.

## Mittelfristig (v1.3.x – v1.4.0)

- **Plugin-Integrations-Roadmap (Cycles 2-6)** - sechs Plugin-
  Integrationen geplant, Honorific (Cycle 1) ist live, danach folgen
  Context-Menu, NotificationMaster, RP-Status-Block, ExtraChat und
  XIVIM in eigenen Cycles. Spec und Cycle-Reihenfolge in
  [Plugin-Integrationen-Übersicht](../Hellion%20Chat%20Plugin-Integrationen.md).
- **Ad-Block / Spam-Filter** — Hybrid-Konzept aus eigenem Light-Filter und
  optionaler `NoSoliciting`-IPC-Integration. Adressiert Werbe-Spam in
  öffentlichen Channels und Tells. Aus dem v1.1.0-Plan zurückgestellt.
- **Receive-Suppressed-Tells-Toggle** — Auto-Tell-Tabs greift auch wenn ein
  Drittplugin (z.B. XIVMessenger) die /tell-Anzeige global suppressed.
  Gleicher Hook-Layer wie Ad-Block, deshalb gebündelt.
- **Database-Viewer Inline-Search** — Volltext-Suche im DB-Viewer via
  SQLite FTS5. Aktuell gibt es nur Datums- und Channel-Filter.
- **TempTell Persistence** — Pin-Toggle auf TempTell-Tabs damit ausgewählte
  Tells einen Relog überleben. Tester-Wunsch von Jingliu.
- **FontManager Async-Refactor** — `LoadGameSymFontAsync` aus dem
  blockierenden Plugin-Constructor herausziehen. Cold-Start-Hitching beim
  ersten Plugin-Start beheben (Severity niedrig, Plugin ist funktional).
- **Separate Opacity Active vs. Inactive** — zweiter Slider für inaktive
  Fenster-Deckkraft. Upstream lehnt das ab; wir können hier anders
  entscheiden.
- **Failed-Tell-Notification** — sichtbare Nachricht bei /tell-Fail
  (offline, restricted instance, blacklisted, world-mismatch) statt
  stillem Failure.
- **Per-Tab Sound-Notification** — Sound-Toggle und optional eigene .wav
  pro Tab, mit Mute-In-Combat-Option.

## Langfrist (v1.x+)

### Storage-Backends (drei Stufen Bestätigung)

- MySQL/MariaDB-Backend für Multi-Device-Setups
- PostgreSQL-Backend
- AES-256-Verschlüsselung für sensible Channels mit lokalem Key

### Linux-spezifisch

- WireGuard-Network-Detection als optionaler Filter-Trigger
- libnotify-Integration für native Linux-Toasts
- XDG-Compliance (komplex unter Wine)

### UX und Tab-Management

- **Regex Tab Routing** — Plugin-Output-Spam in eigene Tabs, Tells
  bestimmter Personen automatisch sortieren. Klar abgegrenzt zum Ad-Block:
  Routing sortiert in Views, Block versteckt global.
- **Auto-Detect Duties** — Tab-Switch beim Duty-Start via Condition-Flag.
- **UX Bundle** — Vertical-Tab-Bar als Layout-Option, Shift+Mousewheel zum
  Tab-Header-Scrollen ohne Aktivierung, globaler Hotkey zum Schließen des
  aktiven Tabs.
- **Configure Tab Title** — konfigurierbares Tab-Title-Format
  (Name / Name + abgekürzter World / voller Name / Custom), pro Tab
  überschreibbar.
- **Name Display Options** — analog zu FFXIV-Vanilla (voller Name, Vorname
  abgekürzt, Initialen), per-Channel-Override möglich.
- **Item & Flag Linking** — Outgoing: Shift-Klick auf Item/Flag sendet ins
  fokussierte Plugin-Input. Incoming: Item-Links und Map-Coords klickbar.
- **Color Currently Selected Input Channel** — Channel-Selector-Button im
  Input-Bar mit Channel-Farbe einfärben.
- **Plugin-Disclosure Pre-Send Filter** — konfigurierbare Wort-/Regex-Liste
  blockiert das Senden mit Pre-Send-Confirm. Schutz vor versehentlicher
  Plugin-Nennung in öffentlichen Channels.
- **Chat Clear on Name Change** — bei Charakter-Namensänderung lokalen
  Verlauf migrieren oder löschen, Default Wipe für maximale Privacy.
- **Hide Plugin Window on NG+ Screen** — Hide-Logik um zusätzliche
  Addon-Namen erweitern.
- **Kick from Novice Network** — Mentor-Nische, Context-Menü-Eintrag mit
  Confirmation.
- **Text-to-Speech für /tell** — eingehende Tells via TTS, optional pro
  Sender, mit Channel-Filter und Mute-In-Combat. Geringe Priorität.

### Distribution und Branding

- Hand-gezeichnetes Hellion-Logo (aktuell Platzhalter aus dem
  Hellion-Online-Media-Brand-Repo)
- GitHub Action für automatischen `repo.json`-Sync nach Tag-Push
- Submission ans Dalamud-Main-Plugin-Repo (zusätzlich zum Custom-Repo)

---

## Bug-Verifizierungen

Aus dem Upstream-Issue-Tracker übernommen, in Hellion Chat 1.0.0 noch
nicht reproduziert oder verifiziert. Werden bei Gelegenheit gegen den
aktuellen Stand getestet.

- **Right-Click Whisper Error** in Field Ops / Special Instances (Eureka,
  Bozja, Occult Crescent, DRS) — Upstream
  [#168](https://github.com/Infiziert90/ChatTwo/issues/168). Reply-Helper
  scheint `@World`-Suffix zu schlucken.
- **FPS Drops with Plugin active** — Upstream
  [#145](https://github.com/Infiziert90/ChatTwo/issues/145). 10–20 % Drop
  seit upstream v1.29.19.0. v1.0.0 hat mehrere Fixes auf den verdächtigen
  Pfaden, Repro-Test gegen aktuellen Stand offen.
- **Add Blacklist from Plugin Window** — Upstream
  [#140](https://github.com/Infiziert90/ChatTwo/issues/140). Right-Click
  Add-to-Blacklist wirft "Cannot locate character with that name", via
  Vanilla-Chat funktioniert es.
- **DB-Viewer Column Sort** — sortiert State-Column lexikografisch statt
  numerisch (10 vor 2). XIVIM
  [#82](https://github.com/NightmareXIV/XIVInstantMessenger/issues/82),
  Repro in Hellion Chat offen.

---

## Lizenz-Boundary

Hellion Chat ist EUPL-1.2-lizenziert. Konzept-Imports aus AGPL-3.0-Plugins
(z.B. XIV Instant Messenger) sind ausschließlich architektonische
Inspiration, kein Code-Port. Imports aus dem GPL-3.0-kompatiblen
Upstream-Bestand laufen weiter über
[`UPSTREAM_SYNC.md`](UPSTREAM_SYNC.md).
