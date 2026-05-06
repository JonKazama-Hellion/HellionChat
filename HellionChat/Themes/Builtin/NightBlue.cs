using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class NightBlue
{
    public const string Slug = "night-blue";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Night Blue",
        Author: "Julia Moon",
        Description: "Royal Blue auf Marineblau — kühles Tech-Dashboard-Mood.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#3576C0"),
            Primary:        ColourUtil.HexToRgba("#4A90E2"),
            PrimaryLight:   ColourUtil.HexToRgba("#6AB0FF"),
            PrimaryGlow:    ColourUtil.HexToRgba("#4A90E299"),

            AccentDark:     ColourUtil.HexToRgba("#C97A2E"),
            Accent:         ColourUtil.HexToRgba("#E8A040"),
            AccentLight:    ColourUtil.HexToRgba("#F4B968"),

            Identity:       ColourUtil.HexToRgba("#3576C0"),

            WindowBg:       ColourUtil.HexToRgba("#050B18"),
            ChildBg:        ColourUtil.HexToRgba("#0A1628"),
            FrameBg:        ColourUtil.HexToRgba("#122039"),
            Surface:        ColourUtil.HexToRgba("#1A2D4F"),
            SurfaceHover:   ColourUtil.HexToRgba("#234070"),
            Border:         ColourUtil.HexToRgba("#4A90E266"),

            TextPrimary:    ColourUtil.HexToRgba("#E6EDF7"),
            TextMuted:      ColourUtil.HexToRgba("#8CA0BF"),
            TextDim:        ColourUtil.HexToRgba("#5A6F8F"),

            StatusSuccess:  ColourUtil.HexToRgba("#3DDC97"),
            StatusDanger:   ColourUtil.HexToRgba("#FF5C7A"),
            StatusWarning:  ColourUtil.HexToRgba("#FFB84A"),
            StatusInfo:     ColourUtil.HexToRgba("#4A90E2")
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
            // Night Blue — Royal-Blue-Tinte in Party/FC, Bronze-Gold in Yell/
            // Alliance. Channel-identity (Tell-Pink, NN-Lime) bleibt erhalten.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#FFFFFF"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#FFD060"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#FFA040"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#6AA8E8"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#E8B070"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#4FA8E8"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#A8E060"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#6AA8E8"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#A8E060"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#E8B070"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#FFD060"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#80E8A8"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#6AA8E8"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#A8A0F0"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#E8B0F0"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E8B070"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E8B070"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#8CA0BF"),
        })
    );
}
