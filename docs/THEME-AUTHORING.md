<p align="center">
  <img src="images/hellion-forge.png" alt="Hellion Forge" width="180" />
</p>

# Theme Authoring Guide

> Built by **Hellion Forge** — the plugin workshop arm of [Hellion Online Media](https://hellion-media.de). HellionChat ships with five built-in themes; this guide walks you through writing your own.

## TL;DR

1. Open Settings → Themes → **Open themes folder**
2. Copy `example-theme.json` to `<your-name>.json` in the same folder
3. Edit the file with any text editor
4. Reload the plugin (toggle off/on in `/xlplugins`)
5. Your theme appears in the Custom-Themes section in Settings → Themes

That's the whole loop. The rest of this document is reference.

## File location

```
%APPDATA%\XIVLauncher\pluginConfigs\HellionChat\themes\
```

(or the equivalent path on Linux/macOS — Settings → Themes → "Open themes folder" opens it directly).

Each `*.json` file in this folder is loaded as one theme. The `example-theme.json` that HellionChat seeds on first launch is your starting template.

## File format

Theme JSON has four blocks:

```json
{
  "schemaVersion": 1,
  "slug": "your-slug",
  "name": "Your Theme Name",
  "author": "You",
  "description": "One-line description shown under the theme name.",
  "colors":   { ... 21 color slots ... },
  "layout":   { ...  9 layout values ... },
  "chatChannels": { ... optional, channel-name → hex ... }
}
```

### Top-level fields

| Field | Type | Required | Notes |
|---|---|---|---|
| `schemaVersion` | int | yes | Always `1` for HellionChat 1.1.0. The plugin warns and skips themes with a different number. |
| `slug` | string | yes | Lowercase, hyphenated. Must be unique across all themes (built-in slugs are reserved). |
| `name` | string | yes | Display name in the picker. |
| `author` | string | yes | Shown small under the theme name. |
| `description` | string | yes | One short sentence. |
| `colors` | object | yes | All 21 slots required (see below). |
| `layout` | object | yes | All 9 slots required (see below). |
| `chatChannels` | object | no | Optional channel-name → hex map (see below). |

### Color slots

All values are 6-digit `#RRGGBB` or 8-digit `#RRGGBBAA` hex strings. Six-digit values get an implicit `FF` alpha.

| Slot | Role |
|---|---|
| `primary` | Brand color — used on buttons, sliders, check marks, highlighted separators. |
| `primaryDark` | Pressed-button stage. |
| `primaryLight` | Hovered-button / link-text stage. |
| `primaryGlow` | Glow / subtle accent (typically primary with ~60% alpha). |
| `accent` | Counter-accent — scrollbar grab on hover/active, resize grip, optional CTA. |
| `accentDark` / `accentLight` | Dark/light siblings of accent. |
| `identity` | Title-bar active color and active-tab color. Often equals `primaryDark`. |
| `windowBg` | Outermost window background. |
| `childBg` | Inner panel / popup background. |
| `frameBg` | Input fields, sliders, combos. |
| `surface` | Card surfaces, headers, selectables. |
| `surfaceHover` | Hovered card / header step. |
| `border` | Panel borders. Typically primary with ~40% alpha for a brand-tinted edge. |
| `textPrimary` | Body text. Soft off-white reads better than pure `#FFFFFF` on dark backgrounds. |
| `textMuted` | Captions, secondary lines. |
| `textDim` | Disabled / hint text, separators. |
| `statusSuccess` | Green-ish for success notifications. |
| `statusDanger` | Red for errors. |
| `statusWarning` | Amber for warnings. |
| `statusInfo` | Cyan-ish info. Often equals primary. |

### Layout slots

All values are floats in pixels. `BorderSize` is 0 or 1 (no thicker borders look right with ImGui's edge anti-aliasing).

| Slot | Typical range | Notes |
|---|---|---|
| `windowRounding` | 0–8 | 0 = sharp upstream look; 4–6 = softer "app" feel. |
| `childRounding` | 0–6 | Usually 1 less than `windowRounding`. |
| `popupRounding` | 0–6 | Same as `childRounding`. |
| `frameRounding` | 0–4 | For inputs, sliders. |
| `grabRounding` | 0–4 | Slider grab dot. |
| `tabRounding` | 0–4 | Tab corners. |
| `scrollbarRounding` | 0–4 | Scrollbar grab. |
| `windowBorderSize` | 0 or 1 | 1 reads better in dark themes. |
| `frameBorderSize` | 0 or 1 | Usually matches windowBorderSize. |

### Optional `chatChannels`

If present, your theme proposes its own chat-channel colors. Property names are `ChatType` enum values (case-insensitive). Unknown names are skipped silently — safe for forward-compat.

```json
"chatChannels": {
  "Say":             "#FFFFFF",
  "Yell":            "#FFE066",
  "Shout":           "#FFA040",
  "TellIncoming":    "#FF99CC",
  "TellOutgoing":    "#FF99CC",
  "Party":           "#80C0E8",
  "FreeCompany":     "#4DD9E8",
  "NoviceNetwork":   "#A8E060",
  "Linkshell1":      "#A8E060"
}
```

The user is asked **once per theme switch** whether to apply these colors — never auto-overwriting existing picks. The banner shows up only if your suggested colors differ from the user's current `Configuration.ChatColours`.

#### Channel-identity rule

**Don't break FFXIV channel identity.** Players have used these conventions for over a decade:

| Channel | Convention | Why |
|---|---|---|
| Say | white / off-white | Default-readable speech. |
| Yell | yellow | Urgent broadcast. |
| Shout | orange | Local urgent. |
| Tell | pink-magenta | Whisper, must stand out. |
| Party | light blue | Group ops. |
| FreeCompany | cyan-teal | Guild ops. |
| NoviceNetwork | lime-green | Mentor channel. |

A theme can tint these toward its brand family (e.g., a purple theme can shift Tell from `#FF99CC` to `#E090FF`), but **don't** flip them (Tell suddenly green, Yell suddenly cyan). RP groups and combat-spec setups depend on the visual hierarchy.

The four colored built-in themes (Hellion Arctic, Event Horizon, Moonlit Bloom, Mint Grove) all follow this rule — read their JSON for reference. Chat 2 Klassik intentionally ships without `chatChannels` so the user keeps their existing picks.

## Theme families

Naming convention `<color>-<modifier>` is recommended for theme families. The first member of a family is the lightest/brightest:

- `mint-grove` (current built-in, light mint)
- `forest-grove` (planned, dark emerald)
- `moss-grove` (planned, mid muted)

Code-wise families have no special handling — only the slug naming hints at the relationship. The picker may group families later, but that's not required.

## Validation and errors

When HellionChat loads your theme:

- **Schema mismatch** (`schemaVersion != 1`): theme is skipped, warning written to `/xllog`.
- **Missing required field** (e.g., no `slug`): theme is skipped, warning written.
- **Invalid hex** (e.g., `#GGHHII`): theme is skipped, warning written.
- **Unknown channel name** in `chatChannels`: that one channel is skipped silently, the rest of the theme loads normally.

Check `/xllog` after a plugin reload to see what loaded and what didn't.

## Testing your theme

1. Edit the JSON, save the file.
2. Reload the plugin: `/xlplugins` → toggle HellionChat off, then on.
3. Settings → Themes → click your theme card.
4. Watch every plugin window (chat, settings, pop-out) and pick something to fix.
5. Tweak. Reload. Repeat.

Tip: the **Settings → Themes** picker shows a mini-mockup per theme — your colors are visible before you switch.

## Sharing themes

Themes are JSON, so sharing is just a file. Drop it into someone's `pluginConfigs/HellionChat/themes/` folder and their plugin picks it up on next reload.

A community theme repository is on the Hellion Forge roadmap. Until then: share via Discord or any pastebin.

## Reference

- `docs/example-theme.json` (seeded automatically on first launch into `pluginConfigs/HellionChat/themes/`) — minimal valid theme.
- The five built-in themes live in source under `HellionChat/Themes/Builtin/`. They are a good reference for Color choices that work.
- [Hellion Online Media branding](https://hellion-media.de) — the Arctic Cyan + Ember Glow palette that drives the default Hellion Arctic theme.

---

<p align="center"><sub>HellionChat is a privacy-focused fork of <a href="https://github.com/Infiziert90/ChatTwo">Chat 2</a>, distributed under the EUPL-1.2.<br/>Theme engine and authoring guide are part of <strong>Hellion Forge</strong>.</sub></p>
