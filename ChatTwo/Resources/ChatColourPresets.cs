using System.Collections.Generic;
using ChatTwo.Code;
using ChatTwo.Util;

namespace ChatTwo.Resources;

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
}
