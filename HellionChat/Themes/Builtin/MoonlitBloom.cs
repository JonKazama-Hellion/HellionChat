using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class MoonlitBloom
{
    public const string Slug = "moonlit-bloom";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Moonlit Bloom",
        Author: "Hellion Online Media",
        Description: "Bloom Magenta + Soft Sage auf Deep Violet Night.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#C957D0"),
            Primary:        ColourUtil.HexToRgba("#E374E8"),
            PrimaryLight:   ColourUtil.HexToRgba("#EF8AF4"),
            PrimaryGlow:    ColourUtil.HexToRgba("#E374E899"),

            AccentDark:     ColourUtil.HexToRgba("#7AAC5C"),
            Accent:         ColourUtil.HexToRgba("#9CCB7C"),
            AccentLight:    ColourUtil.HexToRgba("#B6E297"),

            Identity:       ColourUtil.HexToRgba("#E374E8"),

            WindowBg:       ColourUtil.HexToRgba("#0E0C1F"),
            ChildBg:        ColourUtil.HexToRgba("#15122B"),
            FrameBg:        ColourUtil.HexToRgba("#1F1A38"),
            Surface:        ColourUtil.HexToRgba("#28224A"),
            SurfaceHover:   ColourUtil.HexToRgba("#332B5B"),
            Border:         ColourUtil.HexToRgba("#E374E844"),

            TextPrimary:    ColourUtil.HexToRgba("#ECE6F5"),
            TextMuted:      ColourUtil.HexToRgba("#9A8BB0"),
            TextDim:        ColourUtil.HexToRgba("#554B6E"),

            StatusSuccess:  ColourUtil.HexToRgba("#7AAC5C"),
            StatusDanger:   ColourUtil.HexToRgba("#E85C6A"),
            StatusWarning:  ColourUtil.HexToRgba("#E8B590"),
            StatusInfo:     ColourUtil.HexToRgba("#6278FF")
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
            // Moonlit Bloom — Bloom-Magenta-Tönung. Sage-Drift in NoviceNetwork
            // und Linkshell4. Tell-Pink-Identität bleibt sichtbar.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#ECE6F5"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F0D080"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#F09A60"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#EF8AF4"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#EF8AF4"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#A0B0F0"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#F0B090"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#A8C8E8"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#9CCB7C"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#A0B0F0"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#9CCB7C"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#F0BC92"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F0D080"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#B6E297"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#A0B0F0"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#C098D8"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#EF8AF4"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#E8B0E8"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E8B590"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E8B590"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#9A8BB0"),
        })
    );
}
