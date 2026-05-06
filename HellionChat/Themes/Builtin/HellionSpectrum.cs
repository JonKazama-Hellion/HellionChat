using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

// Hellion Spectrum: Deuteran/Protan-safe channel colours.
// Palette derived from Bang Wong, "Points of view: Color blindness",
// Nature Methods 8, 441 (2011). Channel identity (Tell pink, Yell yellow,
// Shout orange, Party blue, FC green) is preserved per Channel-Identity-
// Rule in docs/THEME-AUTHORING.md; tones are chosen so every channel
// stays distinguishable under red-green colour-vision deficiency.
internal static class HellionSpectrum
{
    public const string Slug = "hellion-spectrum";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Hellion Spectrum",
        Author: "Hellion Online Media",
        Description: "Deuteran/Protan-safe channels — Wong palette tones, channel identity preserved.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#005983"),
            Primary:        ColourUtil.HexToRgba("#0072B2"),
            PrimaryLight:   ColourUtil.HexToRgba("#3E9BD0"),
            PrimaryGlow:    ColourUtil.HexToRgba("#0072B299"),

            AccentDark:     ColourUtil.HexToRgba("#B07F00"),
            Accent:         ColourUtil.HexToRgba("#E69F00"),
            AccentLight:    ColourUtil.HexToRgba("#F0B73A"),

            Identity:       ColourUtil.HexToRgba("#005983"),

            WindowBg:       ColourUtil.HexToRgba("#0A0F14"),
            ChildBg:        ColourUtil.HexToRgba("#101620"),
            FrameBg:        ColourUtil.HexToRgba("#1A222E"),
            Surface:        ColourUtil.HexToRgba("#22303F"),
            SurfaceHover:   ColourUtil.HexToRgba("#2D3E51"),
            Border:         ColourUtil.HexToRgba("#0072B266"),

            TextPrimary:    ColourUtil.HexToRgba("#F0F4F8"),
            TextMuted:      ColourUtil.HexToRgba("#9AA8B5"),
            TextDim:        ColourUtil.HexToRgba("#5E6B78"),

            StatusSuccess:  ColourUtil.HexToRgba("#009E73"),
            StatusDanger:   ColourUtil.HexToRgba("#D55E00"),
            StatusWarning:  ColourUtil.HexToRgba("#F0E442"),
            StatusInfo:     ColourUtil.HexToRgba("#56B4E9")
        ),
        Layout: new ThemeLayout(
            WindowRounding: 6f, ChildRounding: 5f, PopupRounding: 5f,
            FrameRounding: 4f, GrabRounding: 4f, TabRounding: 4f,
            ScrollbarRounding: 4f, WindowBorderSize: 1f, FrameBorderSize: 1f
        ),
        Typography: new ThemeTypography(),
        IsBuiltIn: true,
        ChatColors: new ThemeChatColors(new Dictionary<HellionChat.Code.ChatType, uint>
        {
            // Hellion Spectrum — Wong/Okabe-Ito tones within FFXIV channel
            // identity. FC pulled slightly greener than vanilla cyan-teal so
            // Party-blue and FC-green stay separable under deuteran sim.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#FFFFFF"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F0E442"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#D55E00"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#CC79A7"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#CC79A7"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#56B4E9"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#E69F00"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#009E73"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#94CC4A"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#56B4E9"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#94CC4A"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#E69F00"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F0E442"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#66D9A8"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#56B4E9"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#8B7DD0"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#E0A0C0"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#DAA0DA"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#C9A56F"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#C9A56F"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#C0C0C0"),
        })
    );
}
