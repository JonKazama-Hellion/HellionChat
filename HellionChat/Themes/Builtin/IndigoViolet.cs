using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class IndigoViolet
{
    public const string Slug = "indigo-violet";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Indigo Violet",
        Author: "Florian Wathling",
        Description: "Royal Violet auf Deep Indigo — Glitter-Galaxy mit Türkis-Mint-Aurora.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#6B3AB0"),
            Primary:        ColourUtil.HexToRgba("#8B4DDE"),
            PrimaryLight:   ColourUtil.HexToRgba("#B07CFF"),
            PrimaryGlow:    ColourUtil.HexToRgba("#8B4DDE99"),

            AccentDark:     ColourUtil.HexToRgba("#36A89C"),
            Accent:         ColourUtil.HexToRgba("#4FC9B8"),
            AccentLight:    ColourUtil.HexToRgba("#7AE0CF"),

            Identity:       ColourUtil.HexToRgba("#6B3AB0"),

            WindowBg:       ColourUtil.HexToRgba("#0D061F"),
            ChildBg:        ColourUtil.HexToRgba("#1A0D3D"),
            FrameBg:        ColourUtil.HexToRgba("#2A1556"),
            Surface:        ColourUtil.HexToRgba("#3D1F78"),
            SurfaceHover:   ColourUtil.HexToRgba("#5B2A9A"),
            Border:         ColourUtil.HexToRgba("#8B4DDE66"),

            TextPrimary:    ColourUtil.HexToRgba("#F0E6FF"),
            TextMuted:      ColourUtil.HexToRgba("#A890D0"),
            TextDim:        ColourUtil.HexToRgba("#7560A0"),

            StatusSuccess:  ColourUtil.HexToRgba("#3DDC97"),
            StatusDanger:   ColourUtil.HexToRgba("#FF5C7A"),
            StatusWarning:  ColourUtil.HexToRgba("#FFB84A"),
            StatusInfo:     ColourUtil.HexToRgba("#8B4DDE")
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
            // Indigo Violet — Lavender-Pink-Drift in Tell und LS6/7. Türkis-
            // Mint-Aurora-Counter in Party/FC und LS4. Glitter-Gold in Yell.
            // Differenzierung zu Event Horizon: dunkler, dichter, Türkis statt Gold.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#F0E6FF"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F0D880"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#F09A60"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#6AB8D0"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#F0A878"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#4FC9B8"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#A0E090"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#6AB8D0"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#A0E090"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#F0BC92"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F0D880"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#80E0C0"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#6AB8D0"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#B07CFF"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#E090FF"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#C098D8"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E8B590"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E8B590"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#A890D0"),
        })
    );
}
