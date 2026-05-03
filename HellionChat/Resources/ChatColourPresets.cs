using System.Collections.Generic;
using HellionChat.Code;
using HellionChat.Util;

namespace HellionChat.Resources;

// Hellion Chat — v0.6.0 built-in colour presets for the ChatColours
// settings section. Read-only static data; users apply a preset via the
// settings UI which overwrites Configuration.ChatColours immediately.
// Battle-channel types are intentionally NOT covered by the stylistic
// presets so that combat-log tuning the user has done stays intact.
public sealed record ChatColourPreset(
    string DisplayName,
    string LocalizationKey,
    bool IsBrandPreset,
    IReadOnlyDictionary<ChatType, uint> Colours);

public static class ChatColourPresets
{
    public static IReadOnlyDictionary<string, ChatColourPreset> All { get; } = BuildAll();

    private static Dictionary<string, ChatColourPreset> BuildAll()
    {
        return new Dictionary<string, ChatColourPreset>
        {
            ["Default"] = new(
                DisplayName: "ChatTwo Default",
                LocalizationKey: "ChatColourPresets_Default",
                IsBrandPreset: false,
                Colours: BuildDefault()),
            ["HighContrast"] = new(
                DisplayName: "High-Contrast",
                LocalizationKey: "ChatColourPresets_HighContrast",
                IsBrandPreset: false,
                Colours: BuildHighContrast()),
            ["Pastell"] = new(
                DisplayName: "Pastell",
                LocalizationKey: "ChatColourPresets_Pastell",
                IsBrandPreset: false,
                Colours: BuildPastell()),
            ["DarkModeTuned"] = new(
                DisplayName: "Dark-Mode-Tuned",
                LocalizationKey: "ChatColourPresets_DarkModeTuned",
                IsBrandPreset: false,
                Colours: BuildDarkModeTuned()),
            ["Hellion"] = new(
                DisplayName: "Hellion",
                LocalizationKey: "ChatColourPresets_Hellion",
                IsBrandPreset: true,
                Colours: BuildHellion()),
            ["NightBlue"] = new(
                DisplayName: "Night Blue",
                LocalizationKey: "ChatColourPresets_NightBlue",
                IsBrandPreset: false,
                Colours: BuildNightBlue()),
            ["IndigoViolet"] = new(
                DisplayName: "Indigo Violet",
                LocalizationKey: "ChatColourPresets_IndigoViolet",
                IsBrandPreset: false,
                Colours: BuildIndigoViolet()),
        };
    }

    // The Default preset spiegelt 1:1 die Werte aus ChatTypeExt.DefaultColor.
    // Channels ohne Default-Wert (return null) werden ausgelassen — wer sie
    // anwenden will, behält seine aktuelle Farbe.
    private static IReadOnlyDictionary<ChatType, uint> BuildDefault()
    {
        var dict = new Dictionary<ChatType, uint>();
        foreach (var (_, types) in ChatTypeExt.SortOrder)
        {
            foreach (var type in types)
            {
                var def = type.DefaultColor();
                if (def.HasValue)
                    dict[type] = def.Value;
            }
        }
        return dict;
    }

    private static IReadOnlyDictionary<ChatType, uint> BuildHighContrast()
    {
        return new Dictionary<ChatType, uint>
        {
            [ChatType.Say] = ColourUtil.ComponentsToRgba(255, 255, 255),
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(255, 192, 0),
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(255, 96, 0),
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(255, 128, 255),
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(255, 128, 255),
            [ChatType.Party] = ColourUtil.ComponentsToRgba(128, 192, 255),
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(255, 128, 64),
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(96, 192, 255),
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(192, 255, 64),
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(255, 128, 128),
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(255, 192, 128),
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(255, 255, 128),
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(192, 255, 128),
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(128, 255, 192),
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(128, 192, 255),
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(192, 128, 255),
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(255, 128, 192),
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(255, 96, 96),
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(255, 160, 96),
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(255, 255, 96),
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(160, 255, 96),
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(96, 255, 160),
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(96, 160, 255),
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(160, 96, 255),
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(255, 96, 160),
        };
    }

    private static IReadOnlyDictionary<ChatType, uint> BuildPastell()
    {
        return new Dictionary<ChatType, uint>
        {
            [ChatType.Say] = ColourUtil.ComponentsToRgba(232, 232, 232),
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(245, 216, 155),
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(245, 176, 155),
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(224, 176, 224),
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(224, 176, 224),
            [ChatType.Party] = ColourUtil.ComponentsToRgba(176, 204, 224),
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(224, 192, 160),
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(168, 200, 224),
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(200, 224, 176),
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(224, 176, 176),
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(224, 200, 176),
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(224, 224, 176),
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(200, 224, 176),
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(176, 224, 200),
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(176, 200, 224),
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(200, 176, 224),
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(224, 176, 200),
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(224, 160, 160),
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(224, 192, 160),
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(224, 224, 160),
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(192, 224, 160),
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(160, 224, 192),
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(160, 192, 224),
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(192, 160, 224),
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(224, 160, 192),
        };
    }

    private static IReadOnlyDictionary<ChatType, uint> BuildDarkModeTuned()
    {
        return new Dictionary<ChatType, uint>
        {
            [ChatType.Say] = ColourUtil.ComponentsToRgba(240, 240, 240),
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(255, 208, 64),
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(255, 128, 64),
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(255, 160, 255),
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(255, 160, 255),
            [ChatType.Party] = ColourUtil.ComponentsToRgba(160, 208, 255),
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(255, 160, 96),
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(128, 200, 255),
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(192, 255, 96),
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(255, 160, 160),
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(255, 192, 160),
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(255, 255, 160),
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(192, 255, 160),
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(160, 255, 192),
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(160, 192, 255),
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(192, 160, 255),
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(255, 160, 192),
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(255, 128, 128),
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(255, 160, 128),
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(255, 255, 128),
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(160, 255, 128),
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(128, 255, 160),
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(128, 160, 255),
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(160, 128, 255),
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(255, 128, 160),
        };
    }

    // Hellion brand preset — Arctic Cyan + Ember Orange palette aus
    // /mnt/ssd-fast/Projekte/hellion-media/hellion-media-website/BRANDING.md
    // (Schema-Stand 2026-04-16). Channels sind über das ganze Brand-Spektrum
    // verteilt damit jede Zeile auf einen Glance unterscheidbar ist:
    // Cyan-Familie für Standard/Tell, Ember + Warning für laute Channels,
    // Status-Farben (Success, Danger) für Linkshells. CrossLinkshells
    // nutzen die dunkleren/sattersten Varianten derselben Hue-Familien.
    private static IReadOnlyDictionary<ChatType, uint> BuildHellion()
    {
        return new Dictionary<ChatType, uint>
        {
            // Standard / Tell — Cyan-Familie (Brand-Primary)
            [ChatType.Say] = ColourUtil.ComponentsToRgba(77, 217, 232),         // Cyan-light #4DD9E8
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(0, 190, 210), // Brand Cyan #00BED2
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(0, 151, 167), // Cyan-dark #0097A7

            // Laute Channels — Ember/Warning
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(240, 173, 78),    // Warning #F0AD4E
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(249, 115, 22),   // Brand Ember #F97316

            // Gruppen-Channels — Success/Ember-dark/Cyan
            [ChatType.Party] = ColourUtil.ComponentsToRgba(92, 184, 92),         // Success #5CB85C
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(232, 93, 4),       // Ember-dark #E85D04
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(0, 190, 210),   // Brand Cyan
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(77, 217, 232),// Cyan-light

            // Linkshells 1-8 — über das ganze Brand-Spektrum verteilt
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(251, 146, 60),  // Ember-light #FB923C
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(240, 173, 78),  // Warning
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(92, 184, 92),   // Success
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(77, 217, 232),  // Cyan-light
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(0, 190, 210),   // Brand Cyan
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(0, 151, 167),   // Cyan-dark
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(249, 115, 22),  // Brand Ember
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(217, 83, 79),   // Danger #D9534F

            // CrossWorld-Linkshells 1-8 — dunklere/sattersere Varianten
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(232, 93, 4),    // Ember-dark
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(200, 140, 50),  // Warning-dark
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(60, 140, 60),   // Success-dark
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(0, 190, 210),   // Brand Cyan
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(0, 151, 167),   // Cyan-dark
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(0, 110, 130),   // Cyan-darker
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(220, 90, 30),   // Ember-medium
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(170, 60, 60),   // Danger-dark
        };
    }

    // Bonus preset — Night Blue, KAZAMA-Stimmungs-Theme aus
    // /mnt/HDD-Data1/Obsidian/Vault/Systeme/KAZAMA/Theming/Night Blue + Indigo Violet Themes.md
    // Klassisch, kühl, technisch — Marineblau-Tiefe ohne Lila-Anteil.
    // Bewusst NICHT als Brand-Preset markiert (Vault-Boundary): die KAZAMA-Themes
    // sind persönliche Stimmungs-Themes, nicht Teil des Hellion-Brand-Systems.
    private static IReadOnlyDictionary<ChatType, uint> BuildNightBlue()
    {
        return new Dictionary<ChatType, uint>
        {
            // Standard / Tell — Royal Blue Akzent-Familie
            [ChatType.Say] = ColourUtil.ComponentsToRgba(230, 237, 247),         // text-primary
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(106, 176, 255),// akzent-hot
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(74, 144, 226), // akzent-primary

            // Laute Channels — Warning/Danger Status-Töne
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(255, 184, 74),         // warning
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(255, 92, 122),        // danger

            // Gruppen — Success/Akzent-Variations
            [ChatType.Party] = ColourUtil.ComponentsToRgba(61, 220, 151),        // success
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(255, 144, 100),    // warm-orange-light
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(74, 144, 226),  // akzent-primary
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(140, 160, 191),// text-dim

            // Linkshells 1-8 — über Spektrum verteilt
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(255, 184, 74),
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(255, 144, 100),
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(255, 220, 130),
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(130, 220, 100),
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(61, 220, 151),
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(100, 200, 220),
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(106, 176, 255),
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(140, 160, 191),

            // CrossWorld-Linkshells — gedämpfte Variants
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(200, 130, 50),
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(220, 110, 80),
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(200, 180, 60),
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(90, 180, 80),
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(30, 170, 110),
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(50, 130, 170),
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(50, 110, 180),
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(90, 100, 130),
        };
    }

    // Bonus preset — Indigo Violet, KAZAMA-Stimmungs-Theme aus demselben
    // Vault-Doc. Warm-mystisch, "Galaxy/Glitter/Nordlicht" — tiefes Indigo
    // mit kräftigem Violet-Akzent. Persönlicher Favorit (siehe Vault).
    // Auch nicht als Brand-Preset (siehe NightBlue-Note oben).
    private static IReadOnlyDictionary<ChatType, uint> BuildIndigoViolet()
    {
        return new Dictionary<ChatType, uint>
        {
            // Standard / Tell — Royal Violet Akzent-Familie
            [ChatType.Say] = ColourUtil.ComponentsToRgba(240, 230, 255),         // text-primary (light lavender)
            [ChatType.TellIncoming] = ColourUtil.ComponentsToRgba(176, 124, 255),// akzent-hot
            [ChatType.TellOutgoing] = ColourUtil.ComponentsToRgba(139, 77, 222), // akzent-primary

            // Laute Channels — geteilt mit Night Blue (Status-Farben)
            [ChatType.Yell] = ColourUtil.ComponentsToRgba(255, 184, 74),
            [ChatType.Shout] = ColourUtil.ComponentsToRgba(255, 92, 122),

            // Gruppen
            [ChatType.Party] = ColourUtil.ComponentsToRgba(61, 220, 151),
            [ChatType.Alliance] = ColourUtil.ComponentsToRgba(255, 144, 100),
            [ChatType.FreeCompany] = ColourUtil.ComponentsToRgba(139, 77, 222),  // akzent-primary
            [ChatType.NoviceNetwork] = ColourUtil.ComponentsToRgba(168, 144, 208),// text-dim

            // Linkshells 1-8
            [ChatType.Linkshell1] = ColourUtil.ComponentsToRgba(255, 184, 74),
            [ChatType.Linkshell2] = ColourUtil.ComponentsToRgba(255, 144, 100),
            [ChatType.Linkshell3] = ColourUtil.ComponentsToRgba(255, 220, 130),
            [ChatType.Linkshell4] = ColourUtil.ComponentsToRgba(200, 124, 255),
            [ChatType.Linkshell5] = ColourUtil.ComponentsToRgba(176, 124, 255),
            [ChatType.Linkshell6] = ColourUtil.ComponentsToRgba(139, 77, 222),
            [ChatType.Linkshell7] = ColourUtil.ComponentsToRgba(130, 90, 200),
            [ChatType.Linkshell8] = ColourUtil.ComponentsToRgba(168, 144, 208),

            // CrossWorld-Linkshells
            [ChatType.CrossLinkshell1] = ColourUtil.ComponentsToRgba(200, 130, 50),
            [ChatType.CrossLinkshell2] = ColourUtil.ComponentsToRgba(220, 110, 80),
            [ChatType.CrossLinkshell3] = ColourUtil.ComponentsToRgba(200, 180, 60),
            [ChatType.CrossLinkshell4] = ColourUtil.ComponentsToRgba(130, 80, 180),
            [ChatType.CrossLinkshell5] = ColourUtil.ComponentsToRgba(100, 60, 160),
            [ChatType.CrossLinkshell6] = ColourUtil.ComponentsToRgba(91, 42, 154),
            [ChatType.CrossLinkshell7] = ColourUtil.ComponentsToRgba(80, 50, 130),
            [ChatType.CrossLinkshell8] = ColourUtil.ComponentsToRgba(117, 96, 160),
        };
    }
}
