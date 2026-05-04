# Entwicklungsgeschichte und Lernprozess

## Hintergrund

Ich bin Autodidakt. Hellion Chat ist mein erstes FFXIV-Plugin und mein erstes größeres C#-Projekt. Mein beruflicher Hintergrund ist Webentwicklung (Next.js, React, TypeScript, Prisma, MySQL), also Browser-Welt mit JavaScript-Toolchain. C# kannte ich vor diesem Projekt nur oberflächlich, ImGui gar nicht, Dalamud nur als Endnutzer über andere Plugins.

Wenn ich an einer Stelle nicht weiterkomme, nutze ich AI-Tools wie Claude Code als Pair-Hilfsmittel. Wie das genau aussieht und welche Klassifikation ich verwende, steht transparent in [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md).

---

## Warum überhaupt ein Chat-Plugin?

Hellion Chat soll Chat 2 nicht ersetzen. Chat 2 liefert ein vollständiges Chat-Erlebnis mit kompletter Historie, Filtern, Suche und Replay. Für die meisten Nutzer ist genau das richtig.

### Zwei Millionen Nachrichten in zwei Jahren

Mein Wunsch nach einem engeren Default war ehrlich gesagt erstmal persönlich. Nach zwei Jahren mit Chat 2 lag meine Datenbank bei über zwei Millionen Nachrichten, der Großteil davon /say, /shout und /yell von wildfremden Leuten in Limsa. Genau diese Daten machen Chat 2's Voll-Historie nützlich, und die meisten Nutzer behalten sie auch gerne. Mein eigener Geschmack wollte einen kleineren Default. Also habe ich diesen Fork gebaut.

### Greeter in mehreren Clubs

Dazu kam ein zweiter Use-Case: Ich bin in mehreren FFXIV-Clubs als Greeter aktiv. Für die Greeter-Arbeit reicht die Vanilla-Chat-Oberfläche nicht. Parallel laufende /tell-Gespräche schreiben in einem einzigen Tab durcheinander, und ich verliere ständig den Faden, wer mir gerade was geschrieben hat. Auto-Tell-Tabs (eines der frühen Hellion-Chat-Features) ist genau für diesen Workflow entstanden: ein Tab pro Gesprächspartner, automatisch gespawnt, mit manuellem Greeted-Status. Dass das auch der Privacy-Hygiene gut tut, war ein netter Bonus, nicht der Auslöser.

### Hellion Online Media

Die Privacy-Defaults sind außerdem eine Position aus meinem Hauptberuf. Hellion Online Media ist mein Einzelunternehmen, und Datenschutz gegenüber Kunden ist da kein Marketing-Slogan, sondern operativ relevant. Dieser Fork ist die Plugin-Form derselben Haltung.

---

## Warum nicht beim Original mitarbeiten?

Drei Gründe, in absteigender Wichtigkeit.

### Defaults sind nicht verhandelbar, auch nicht meine

Privacy-First als Standard ist eine Minderheits-Position. Chat 2 bedient zu Recht die breite Masse mit Voll-Historie als Default. Diese Defaults im Upstream zu ändern wäre falsch gewesen. Ich hätte den Standard für eine große Nutzerbasis umgekippt, die ihn so wollte, wie er ist. Saubere Trennung über einen eigenen Plugin-Slot war der respektvollere Weg.

### Das Webinterface musste weg

Das ist ein zentrales Chat-2-Feature für Remote-Zugriff vom Zweitgerät. Ein PR der das entfernt, hat in einem gepflegten Upstream-Projekt keine Chance, und das ist auch richtig so. Aber genau das Webinterface kollidiert mit der Privacy-First-These dieses Forks: Ein Chat-Plugin das einen lokalen HTTP-Server startet, ist für mein Threat-Model eine zu große Angriffsfläche. Also raus damit.

### Tempo

Ein Solo-Maintainer-Projekt mit kleinem Tester-Pool kann schneller iterieren als ein etabliertes Plugin mit großer Nutzerbasis. Das ist kein Vorwurf an Upstream, sondern eine andere Optimierung. Ich brauche keine Roadmap-Abstimmung, keine Reviewer-Verfügbarkeit, und kann Audit-Konsequenzen wie das Webinterface-Removal in einer einzigen Version durchziehen statt über mehrere Releases.

EUPL-1.2 erlaubt das alles ausdrücklich, mit klarer Attribution. Der Code liegt offen unter derselben Lizenz wie Chat 2. Infi, Anna oder sonst jemand dürfen reinschauen, Ideen mitnehmen, Fragen stellen oder den Fork einfach ignorieren. Alles drei ist für mich okay.

---

## Wie ich so schnell release

Wer auf den Repo schaut, sieht in kurzer Zeit viele Releases und sehr viele Commits. Beides wird von außen gerne als Red-Flag gelesen: KI-Slop, Salami-Taktik, Code-Spam. Bei Hellion Chat ist beides eine bewusste Entscheidung, und ich erkläre lieber einmal warum, als mich später dafür zu rechtfertigen.

### Vorarbeit, lange bevor der Fork existierte

Bevor ich die erste Zeile in `HellionChat/` getippt habe, war ich wochenlang nur Leser. Chat 2 ingame nutzen und damit rumspielen. Issues im Upstream-Tracker durchgehen, vor allem die geschlossenen, weil dort steht, wie Infi und Anna Bugs einkreisen. Commits lesen, gerne auch ältere, um zu verstehen, warum eine Architektur-Entscheidung getroffen wurde, nicht nur, dass sie getroffen wurde. Wenn ich heute weiß, wo im Code was liegt, dann nicht, weil ich besonders schnell durch eine Codebase navigiere, sondern weil ich den Code vorher gelesen habe.

Klingt nach Selbstverständlichkeit, ist es aber nicht. Die übliche Reihenfolge bei Solo-Forks heißt erst forken, dann verstehen. Ich habe es andersrum gemacht.

### Die Codebase von Infi und Anna

Hellion Chat baut auf einem Boden auf, der schon flach ist. Chat 2 ist sauber strukturiert, die Naming-Konventionen sind konsistent, die Trennung zwischen Layern (Storage, UI, Game-Hooks, IPC) ist klar gezogen. Das ist in Open-Source-Plugin-Welten nicht selbstverständlich, und es ist der Hauptgrund, warum sich Hellion-spezifische Features oft "fast nativ" einbauen lassen. Ich muss nicht erst Spaghetti entwirren bevor ich was Eigenes danebenstellen kann.

Side-Fact: Selbst beim ersten Codebase-Walkthrough mit Claude kam mehrfach der Hinweis, dass die Architektur ungewöhnlich gut aufgeräumt ist und mehrere Erweiterungspunkte vorbereitet. Das hat Gewicht, weil es von außen kommt, aber den eigentlichen Kredit kriegen Infi und Anna, nicht Claude.

### Atomar arbeiten, kleine Commits

Ein Commit, eine logische Änderung. Wenn ich einen Bug fixe, parallel eine Variable umbenenne und nebenbei einen Kommentar einbaue, sind das drei Commits, nicht einer. Klingt nach Mikro-Management, ist es aber nicht. Wenn in sechs Monaten ein Bug auftaucht und ich `git bisect` brauche, finde ich die kaputte Änderung in zwei Minuten statt in zwei Stunden. Bei einem 4000-Zeilen-Mega-Commit darf ich raten, welche der hundert Änderungen die kaputte ist.

Den Stil habe ich bewusst auch deshalb beibehalten, weil Infi im Upstream häufig genauso arbeitet. Manchmal ein Sechs-Zeilen-Commit, manchmal nur ein Typo-Fix. Das ist keine Schwäche, das ist eine Entscheidung für lesbare Git-History. Den Stil im Fork beizubehalten ist ein Respekt-Move: Wer die beiden Repos vergleicht, soll den gleichen Lese-Rhythmus haben.

Bonus für mich persönlich: Kleine Commits zwingen mich, jeden Schritt einzeln zu durchdenken und zu benennen. Wenn ich nicht in zwei Sätzen erklären kann, was ein Commit macht, ist die Änderung wahrscheinlich noch nicht klar genug. Auf Beginner-Niveau ist das ein eingebauter Sanity-Check, den ich bei einem Big-Bang-Commit nicht hätte.

### AI als Beschleuniger, ehrlich

Ja, AI hilft beim Tempo, und nicht zu knapp. Ohne CodeRabbit hätte ich Critical-Bugs der Klasse `Equals/GetHashCode`-Anti-Pattern, Hook-Subscription-Leaks und TOCTOU-Races nicht gefunden. Ich bin schlicht zu unerfahren für diese Klasse von Findings, das schreibe ich genau so hin.

Was ich aber nicht mache: blind Code übernehmen, weil ein Tool ihn als Fix markiert hat. Bei mehreren CodeRabbit-Findings stand in den Original-Commits von Infi oder Anna sogar ein Stackoverflow-Link mit Begründung dabei, warum eine bestimmte Stelle so aussieht wie sie aussieht. Die habe ich gelesen, bevor ich was geändert habe. Erst verstehen, dann anfassen, dann committen. Das ist der Unterschied zwischen "AI gibt mir Code, ich pushe" und "AI zeigt mir wo's klemmt, ich entscheide".

Klassifikation und konkrete Beispiele zur AI-Nutzung stehen in [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md). Hier in dieser Sektion ging es nur um den Tempo-Aspekt: Recherche plus saubere Codebase plus atomare Commits plus AI-gestütztes Review-Sparring sind die vier Faktoren zusammen. Kein einzelner davon erklärt das Tempo allein.

---

## Vom Web-Stack zu C# / Dalamud

### Type-System? Weniger Schock als erwartet

C# nach TypeScript war angenehmer als gedacht. Properties statt getter/setter sind sauber, nullable reference types fühlen sich an wie `strict: true` in TypeScript. Ungewohnt war Wert-Typen vs. Referenz-Typen explizit denken zu müssen (`struct` vs. `class` mit echten Verhaltens-Konsequenzen), und Generics mit Constraints sind syntaktisch anders genug, dass ich beim Lesen kurz stocke. `async`/`await` ist semantisch ähnlich, aber Threading-Modelle sind in C# expliziter: `Task.Run`, `ConfigureAwait`, Synchronization-Contexts. Das hat mich mehrere Bugs gekostet, bevor ich verstanden hatte, wann der Main-Thread (in Plugin-Welt: der Framework-Tick) wirklich kritisch ist.

### Build-Toolchain: ähnlich, aber anders

`dotnet` CLI, csproj-XML, NuGet sind funktional nicht weit weg von npm und tsconfig. Aber das XML-Format der csproj ist eine andere Sprache als JSON-Configs. Die Lock-Datei (`packages.lock.json`) musste ich erst aktiv aktivieren (`RestorePackagesWithLockFile=true`); das ist nicht Default. Im Web-Stack ist Lock-File-First Standard, im .NET-Stack offenbar nicht. Das war eine echte Überraschung.

### ImGui ist eine andere Welt

Immediate-Mode-Rendering hat mit React-Component-Trees nichts gemein. Es gibt keine virtuelle DOM, keine Reconciliation, keinen "State der Komponente". Pro Frame zeichnet der Code die UI komplett neu, und der State lebt entweder in lokalen Variablen, die ich selbst verwalten muss, oder in der ImGui-eigenen ID-Stack-Logik.

Was in React zwei Zeilen `useState` sind, ist in ImGui ein Member-Field plus manuelle ID-Stempel auf den Widgets, sonst kollidieren zwei Selectables in derselben Loop, weil sie auf die gleiche ID zurückfallen. Die ID-Stack-Kollision in `SearchSelector` (gefixt in v1.0.0) war genau dieses Symptom: Alle Selectables fielen auf dieselbe ambiguous ID zurück, bis ich den Row-Index in den Push-ID gemixt habe. Klassischer "warum klickt der falsche Eintrag"-Bug, den man nur findet, wenn man verstanden hat, wie ImGui IDs intern handhabt.

### Dalamud-Spezifika

Plugin-Lifecycle, IPC-Subscriber-Pattern, Hook-System für Game-Functions, Game-Object-Threading. Viel davon war nur durch Lesen der Upstream-Codebase und durch [dalamud.dev](https://dalamud.dev) zu verstehen. Meine Trainings- und Such-Ergebnisse für "Dalamud" liefern oft veraltete API-Beispiele aus alten Versionen. dalamud.dev ist die zuverlässige Quelle. Wenn jemand neu anfängt: dort hin, nicht zu Stack Overflow.

### Der Tag, an dem mich der DalamudPackager einen Tag gekostet hat

Dalamud SDK 15 liefert seinen eigenen Default-Packager mit, der Icons und Image-URLs ins Manifest einträgt. Ich hatte aus dem Upstream-Repo eine eigene `DalamudPackager.targets`-Datei mit `HandleImages`-Override übernommen, und die hat den SDK-Default überschrieben. Resultat: Das Manifest hatte keinen `IconUrl` mehr, und das Plugin tauchte in der Plugin-Liste ohne Icon auf.

Symptom war einfach zu sehen, Ursache hat einen Tag gekostet. Ich hatte die Override-Datei für eine Pflicht-Datei gehalten, war sie aber nicht. Removal in v0.5.2, seitdem läuft der SDK-Default. Lektion: Erstmal mit Defaults arbeiten, Overrides erst wenn der Default nachweislich nicht passt.

---

## Was ich aus dem Fork gelernt habe

### Refactor in einer fremden Codebase

Der Standalone-Cut in v1.0.0 hat die `ChatTwo.*`-Identität komplett auf `HellionChat.*` migriert. Klingt nach Find-and-Replace. War es nicht.

Konkret bedeutete das: Code-Namespace über alle 80 Source-Files plus 100 using-Direktiven plus zwei FQN-Aliases plus die Resource-Designer-Strings. Sechs IPC-Channels umbenannt (Breaking Change für Drittplugins, keine bekannten Anbindungen). Repo-Ordner-Struktur (`ChatTwo/` → `HellionChat/`) inklusive csproj, sln, allen GitHub-Workflows und der dependabot.yml. Public-Facing-Branding in README, repo.json, yaml auf Standalone-Framing umformuliert.

Das war kein Solo-Find-and-Replace, weil Unicode-String-Pfade in Workflow-YAMLs anders quotiert werden müssen als C#-Strings. Weil Resource-Designer-Files generierte Inhalte haben, die nicht jede Toolchain im Blick hat. Und weil die `ChatTwo.*`-IPC-Channel-Namen Strings in `GetIpcSubscriber`-Calls sind: kein Symbol, kein Compile-Error, wenn man einen vergisst. Da merkst du, was alles still bleibt.

### Sicherheit ist kein abstraktes Thema mehr

Vor diesem Projekt war Supply-Chain-Sicherheit für mich akademisch. Drei konkrete Lektionen haben das geändert.

**SQLite-Native-Binary.** Ich musste auf 3.50.3 pinnen (`SQLitePCLRaw.lib.e_sqlite3` Override), weil `Microsoft.Data.Sqlite` die transitiv nachgezogene Lib in einer Version mitschleppte, die CVE-2025-6965 (Memory-Corruption durch Aggregate-Term-Overflow) und CVE-2025-7709 enthielt. Der Managed-Wrapper war neu, die Native-Lib war es nicht. Lektion: Transitive Dependencies prüfen sich nicht von selbst, du musst hinschauen.

**Lock-File-Drift.** `packages.lock.json` honored bei `dotnet restore` (per `RestorePackagesWithLockFile=true` in der csproj) verhindert, dass transitive Versionen zwischen meiner Maschine und CI silent driften. Erst nach einem Build-Output-Mismatch zwischen lokal und GitHub-Actions hatte ich überhaupt verstanden, warum das nicht der Default ist.

**WrapText und der CodeQL-Alarm der drei Releases gekostet hat.** CodeQL hat in `ImGuiUtil.WrapText` einen Critical-Alert wegen "unvalidated local pointer arithmetic" geworfen. v0.5.2 hat einen Edge-Case validiert. Alert kam wieder. v0.5.3 hat den Buffer-Length via `GetByteCount` vor der Pointer-Math gecheckt. Alert kam wieder. v0.5.4 hat den ganzen Algorithmus auf `Span` und int-Offsets umgebaut, mit einem 16-KiB-Cap auf den ArrayPool-Rent. Erst da war Ruhe.

Lektion: Wenn ein statischer Analyzer drei Mal hintereinander meckert, ist nicht der Analyzer überempfindlich. Die Datenflusslogik ist es.

### CodeRabbit als externer Code-Reviewer

Der v1.0.0-Sweep hat 3 Critical und 21 Major Findings hochgespült. Drei Klassen davon waren besonders lehrreich:

- **`Equals`-Methoden die `GetHashCode()` vergleichen.** Klassisches Hash-Kollisions-Anti-Pattern. Klingt nach "ist doch egal, wenn Hashes gleich sind, sind die Objekte auch gleich", ist aber genau falsch. Hashes können kollidieren, Objekte sind dann nicht gleich.
- **`Dispose`-Methoden die nur einen Teil der Subscriptions wieder abmelden.** Leak bei jedem Plugin-Reload. Im Nutzer-Alltag merkst du das nicht sofort, im Long-Running-Test schon.
- **TOCTOU-Races.** Zwischen Bounds-Check und Read kann ein anderer Thread das Array unter dir austauschen (`GlobalParametersCache`, `AutoTranslate`).

Davon hatte ich vorher bestenfalls die Theorie gelesen, nicht selbst diagnostiziert. CodeRabbit war für mich der Moment, wo "akademisches Wissen" zu "okay, das ist mein Code, das ist mein Bug" wurde.

### Externe Tester sind ihr Gewicht in Gold wert

Carlas Feedback zur Pop-Out-Discoverability hat den Header-Button in v0.6.1 ausgelöst. Dass Pop-Outs nur per Rechtsklick erreichbar waren, hatte ich als Maintainer nicht mehr gesehen, ich kannte den Pfad blind. Carls Wunsch nach Theme-Varianten mit Helligkeits-Abstufungen hat mein Verständnis von "ein Theme = eine Farbe" auf "Theme-Familien mit Stimmungs-Varianten" verschoben. Jingliu hat TempTell-Persistence gefordert, was das Tab-System architektonisch in Frage stellt.

Solo hätte ich diese drei Dinge nicht erkannt. Punkt.

### release.yml und die Markdown-Hölle

Der `release.yml`-Workflow ist beim ersten v0.6.0-Tag-Push einfach nicht losgegangen. Ich habe Stunden in Permissions, Secret-Scopes und Tag-Trigger-Konfiguration gegraben, bevor ich verstand, was eigentlich los war: Der PowerShell-Heredoc-Footer im "Generate release body"-Step enthielt eine `---`-Markdown-Horizontal-Rule an Spalte 1, und genau das hat das YAML-Block-Scalar von `run: |` beendet. GitHub konnte die Workflow-Datei nicht parsen, also hat der Push-Tag-Trigger nie registriert.

Fix: Footer in eine externe `.github/release-footer.md` extrahiert, Workflow liest sie via `Get-Content` ein. Lektion: Wenn ein Workflow nicht triggert, verifiziere als Erstes, dass GitHub die Datei überhaupt parsen kann. Das war einer der Bugs, bei denen ich nach dem Fix kurz gelacht habe und mich dann gefragt, wie viele andere YAML-Dateien ich noch habe, die so eine Falle drin haben könnten.

---

## Was ich noch lerne

### Performance-Profiling im Game-Context

Der FPS-Drop-Bug aus Upstream Chat 2 ([#145](https://github.com/Infiziert90/ChatTwo/issues/145)) ist auch in Hellion Chat noch nicht reproduziert oder verifiziert. v1.0.0 hat mehrere Fixes auf den verdächtigen Pfaden (DbViewer O(N²) → O(N), AutoTranslate Lock-Serialisierung, EmoteCache HttpClient-Reuse), aber das systematische Vermessen unter Last fehlt mir. Ich muss noch lernen, wie man im Plugin-Kontext sauber misst, was wirklich das Frame-Budget frisst.

### Native-Interop und Pointer-Math

Auch nach dem WrapText-Span-Refactor in v0.5.4 ist mir Pointer-Math unsicher. ImGui zwingt einen an mehreren Stellen in `unsafe`-Code, und der Sicherheitsabstand zur "unbounded ArrayPool allocation"-Klasse von Bugs ist schmaler als mir lieb ist. Da will ich besser werden, bevor ich tieferes ImGui-Custom-Drawing anfasse.

### Test-Disziplin für Plugin-Code

Aktuell hat das Repo kein Test-Projekt. Das ist eine bewusste Entscheidung, keine vergessene. Plugin-Code mit FFXIV-Hooks und Dalamud-Lifecycle sauber zu testen ist nicht trivial, und ich hatte keinen Ansatz gefunden, der ohne riesiges Mocking-Gerüst sinnvoll wirkte. Privacy-Filter und Configuration-Migration wären gute Testkandidaten, weil sie isoliert sind. Steht auf der Liste, ist aber kein Quick-Win.

### Linux-Eigenheiten unter Wine

XDG-Compliance, libnotify-Integration, WireGuard-Network-Detection, alles in der [Roadmap](ROADMAP.md), und alles technisch noch nicht ganz klar. Wine und sandboxed Plugin-Code teilen nicht alle System-APIs, und ich weiß nicht, wo die Stolperfallen liegen, bevor ich sie gefunden habe.

---

## Einsatz von AI-Tools

Ich verwende Claude Code als Hilfsmittel, nicht als Ersatz für eigene Arbeit.

**Wofür ich AI einsetze:**

- Debugging von Problemen, bei denen ich nach längerer Eigenrecherche nicht weiterkomme
- Mustererkennen über große Codebasen hinweg (z. B. der ChatTwo→HellionChat-Sweep über 80 Dateien)
- Verständnisfragen zu C#- und Dalamud-Konzepten, die mir noch nicht geläufig sind
- Code-Review-Sparring, bevor ich CodeRabbit drauflasse

**Was ich selbst mache:**

- Architektur und Designentscheidungen
- Privacy-First-Defaults und das Threat-Model dahinter
- Tester-Kommunikation und Roadmap-Priorisierung
- Reviewen, Verifizieren, Pushen

Die Klassifikation und konkrete Beispiele stehen in [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md). Mir ist wichtig, dass Nutzer und potenzielle Beiträger verstehen, wie der Code zustande gekommen ist, gerade bei einem Plugin, das mit Nutzerdaten arbeitet.

Ja, AI. Ja, alleine. Beides öfter erwähnt als nötig. Willkommen im Open-Source-Plugin-Klima.

---

## Warum diese Transparenz

Wer sich den Quellcode ansieht, soll wissen:

- Ich bin kein professioneller C#- oder Plugin-Entwickler und lerne weiterhin dazu
- AI-Unterstützung ist ein Werkzeug, kein Ghostwriter
- Die Privacy-Position, die Designentscheidungen und die Roadmap sind meine
- Ich versuche, meinen Code so sauber und sicher zu halten, wie meine aktuellen Fähigkeiten es zulassen

Hellion Chat ist auch ein Lernprojekt, und das soll man dem Repository ansehen dürfen.

---

## Verlinkungen

- [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md) — KI-Pair-Disclosure mit Klassifikations-Schema
- [`CONTRIBUTORS.md`](CONTRIBUTORS.md) — wer hat dieses Plugin neben mir besser gemacht
- [`../NOTICE.md`](../NOTICE.md) — Anerkennung an Infi und Anna für das Chat-2-Fundament
- [`ROADMAP.md`](ROADMAP.md) — geplante Cycles und Themen
