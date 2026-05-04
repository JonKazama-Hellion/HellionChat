# Contributors — Hellion Chat

Hellion Chat ist von der Code-Seite ein Ein-Personen-Projekt. Aber ohne die Leute auf dieser Seite gäbe es weder die Bug-Fixes noch die UX-Verbesserungen, die seit den frühen Versionen reingelaufen sind. Jeder Eintrag hier hat das Plugin konkret besser gemacht.

Die Anerkennung an die Upstream-Autoren von Chat 2 (Infi und Anna) liegt bewusst in [`../NOTICE.md`](../NOTICE.md), nicht hier. Diese Datei deckt explizit Beiträge zur Hellion-Chat-Seite ab.

---

## Entwicklung

### JonKazama (Florian Wathling) — Maintainer

Hellion Chat ist mein erstes FFXIV-Plugin und mein erstes größeres C#-/Dalamud-Projekt. Mein beruflicher Hintergrund ist Webentwicklung (Next.js, React, TypeScript, Prisma). Plugin-Entwicklung in einer fremden Codebase, ImGui, FFXIV-Game-Hooks und der gesamte Dalamud-Stack waren Neuland.

Privacy-First-Defaults, Per-Channel-Retention, Auto-Tell-Tabs, Pop-Out-Input, ChatColours-Presets, Hellion-Theme plus Exo-2-Font und der v1.0.0-Standalone-Cut sind die Hellion-spezifischen Surface-Areas, die ich auf das Chat-2-Fundament aufgebaut habe. Die Lern-Geschichte dahinter steht in [`LEARNING-JOURNEY.md`](LEARNING-JOURNEY.md).

Hellion Chat ist Teil von [Hellion Online Media](https://hellion-media.de).

---

## Tester

Eine kurze Notiz vorneweg: Ich teste das Plugin nicht allein. Die Leute hier haben mir Bugs gemeldet, bevor sie bei mehr Nutzern aufgeschlagen wären. Sie haben UX-Probleme angesprochen, die ich blind nicht mehr gesehen habe. Und sie haben Feature-Wünsche eingebracht, die das Plugin in Richtungen geschoben haben, in die ich von alleine nicht gegangen wäre. Das ist nicht selbstverständlich. Externe Tester sind ihre Zeit wert.

### Carl Beleandis (Carla) — Beta-Tester

Carl testet seit der Bootstrap-Phase und hat sowohl die Pop-Out-Mechanik als auch die Theme-Richtung geprägt. Sein Feedback kommt direkt und ohne Umschweife und das ist genau, was ich beim Testen brauche.

Konkrete Beiträge:

- **Pop-Out-Discoverability** — der Hinweis, dass Pop-Outs nur per Rechtsklick erreichbar waren, hat den Header-Button und den einmaligen Hint-Banner in v0.6.1 ausgelöst. Ich kannte den Rechtsklick-Pfad blind, deshalb hatte ich nicht mehr gesehen, dass neue Nutzer die Funktion gar nicht finden.
- **/tell-Pop-Out-Mode** — der Wunsch, /tell-Tabs direkt als Pop-Out zu öffnen statt über den Tab-Umweg, ist in v0.6.1 als opt-in Settings-Toggle gelandet. Bonus: Bei der Implementation ist ein alter Ghost-Window-Bug aufgefallen (LRU-Drop ließ Pop-Out-Fenster als Geister stehen), der gleich mit gefixt wurde.
- **Theme-Varianten mit Helligkeits-Abstufungen** — der Wunsch nach einer Grün-Familie hat mein Verständnis von "ein Theme = eine Farbe" auf "Theme-Familien mit Stimmungs-Varianten" verschoben. Steht in der [Roadmap](ROADMAP.md) für einen späteren Cycle.

### Jin (Jingliu) — Alpha-Tester

Jin ist der aktive Tester der ersten Stunde und hat den Pop-Out-Workflow architektonisch in eine andere Richtung geschoben.

Konkrete Beiträge:

- **Pop-Out-Tab mit Input-Feld** — der Vorschlag, in einem Pop-Out auch tippen zu können (statt nur lesen), hat die v0.6.0 Pop-Out-Input-Bar ausgelöst. Das war ein größerer Refactor: Der Input-Layer aus `ChatLogWindow` musste so geöffnet werden, dass er auch in `Popout.cs` lebt, mit unabhängigem Text-Buffer und History-Cursor pro Pop-Out. Hat den Cycle dominiert, weil das Design erst sauber sein musste, bevor Code passieren konnte.
- **TempTell Persistence** — der Wunsch, /tell-Tabs per Pin-Toggle einen Relog überleben zu lassen, steht in der [Roadmap](ROADMAP.md) für einen späteren Cycle. Berührt das Tab-System architektonisch und braucht eigenes Design.

---

## Übersetzungen

Hellion-eigene UI-Strings werden in `HellionChat/Resources/HellionStrings.<lang>.resx` gepflegt.

- **Deutsch (DE):** JonKazama (Native Speaker, Hauptsprache des Projekts)

Die Upstream-Sprach-Dateien (`Language.<lang>.resx`) sind nicht Teil dieser Datei. Sie werden über das [Chat-2-Crowdin-Projekt](https://github.com/Infiziert90/ChatTwo) gepflegt; Crowdin-Übersetzer findest du in den Plugin-Settings unter **Info → "Chat 2 community translators"**.

---

## Wie du beitragen kannst

Bug-Reports, Feature-Wünsche und Pull-Requests laufen über [GitHub Issues](https://github.com/JonKazama-Hellion/HellionChat/issues). Workflow und Erwartungen stehen in [`../CONTRIBUTING.md`](../CONTRIBUTING.md), Code of Conduct in [`../CODE_OF_CONDUCT.md`](../CODE_OF_CONDUCT.md).

Tester-Pool für neue Versionen läuft über den Hellion-Forge-Discord: [discord.gg/X9V7Kcv5gR](https://discord.gg/X9V7Kcv5gR). Wer in den Tester-Channel rein will, einfach im Forge melden.

