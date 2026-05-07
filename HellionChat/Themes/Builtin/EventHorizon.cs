using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class EventHorizon
{
    public const string Slug = "event-horizon";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Event Horizon",
        Author: "Hellion Forge",
        Description: "Cosmic Purple auf Near-Black. Deep-Space-Stimmung.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#7B3FCF"),
            Primary:        ColourUtil.HexToRgba("#9D5CFF"),
            PrimaryLight:   ColourUtil.HexToRgba("#B585FF"),
            PrimaryGlow:    ColourUtil.HexToRgba("#9D5CFF99"),

            AccentDark:     ColourUtil.HexToRgba("#C9982E"),
            Accent:         ColourUtil.HexToRgba("#E0AB36"),
            AccentLight:    ColourUtil.HexToRgba("#F2C25C"),

            Identity:       ColourUtil.HexToRgba("#9D5CFF"),

            WindowBg:       ColourUtil.HexToRgba("#040308"),
            ChildBg:        ColourUtil.HexToRgba("#0A081A"),
            FrameBg:        ColourUtil.HexToRgba("#140F23"),
            Surface:        ColourUtil.HexToRgba("#1B1530"),
            SurfaceHover:   ColourUtil.HexToRgba("#251D40"),
            Border:         ColourUtil.HexToRgba("#9D5CFF44"),

            TextPrimary:    ColourUtil.HexToRgba("#E6E0F5"),
            TextMuted:      ColourUtil.HexToRgba("#9890B5"),
            TextDim:        ColourUtil.HexToRgba("#5A5570"),

            StatusSuccess:  ColourUtil.HexToRgba("#26A269"),
            StatusDanger:   ColourUtil.HexToRgba("#ED333B"),
            StatusWarning:  ColourUtil.HexToRgba("#E0AB36"),
            StatusInfo:     ColourUtil.HexToRgba("#9D5CFF")
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
            // Event Horizon — Cosmic-Purple-Drift: helle Pastelle bekommen
            // Lavender-Tinte, Akzent-Channels (Tell) ziehen Richtung Magenta-
            // Lila. Channel-Identität bleibt klar erkennbar.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#E6E0F5"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F2C25C"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#FF9050"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#90A0FF"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#FFAA80"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#9090E8"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#A0E090"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#90A0FF"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#A0E090"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#F0B070"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F2C25C"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#80E0B0"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#90A0FF"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#B585FF"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#D0A0F0"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E0B870"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E0B870"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#9890B5"),
        })
    );
}
