using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class SynthwaveSunset
{
    public const string Slug = "synthwave-sunset";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Synthwave Sunset",
        Author: "Hellion Forge",
        Description: "Hot Magenta + Cyan on midnight violet. 80s neon-grid vibes for late-night raids.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#C71585"),
            Primary:        ColourUtil.HexToRgba("#FF2D95"),
            PrimaryLight:   ColourUtil.HexToRgba("#FF6BB6"),
            PrimaryGlow:    ColourUtil.HexToRgba("#FF2D9599"),

            AccentDark:     ColourUtil.HexToRgba("#0098B8"),
            Accent:         ColourUtil.HexToRgba("#00F0FF"),
            AccentLight:    ColourUtil.HexToRgba("#5CFFFE"),

            Identity:       ColourUtil.HexToRgba("#FF2D95"),

            WindowBg:       ColourUtil.HexToRgba("#13041F"),
            ChildBg:        ColourUtil.HexToRgba("#1E0A35"),
            FrameBg:        ColourUtil.HexToRgba("#2A1247"),
            Surface:        ColourUtil.HexToRgba("#3A1860"),
            SurfaceHover:   ColourUtil.HexToRgba("#4A2475"),
            Border:         ColourUtil.HexToRgba("#FF2D9566"),

            TextPrimary:    ColourUtil.HexToRgba("#F0DFFF"),
            TextMuted:      ColourUtil.HexToRgba("#A88BC4"),
            TextDim:        ColourUtil.HexToRgba("#6F4D8E"),

            StatusSuccess:  ColourUtil.HexToRgba("#39FF14"),
            StatusDanger:   ColourUtil.HexToRgba("#FF3838"),
            StatusWarning:  ColourUtil.HexToRgba("#FFD700"),
            StatusInfo:     ColourUtil.HexToRgba("#00F0FF")
        ),
        Layout: new ThemeLayout(
            WindowRounding: 5f, ChildRounding: 4f, PopupRounding: 4f,
            FrameRounding: 3f, GrabRounding: 3f, TabRounding: 3f,
            ScrollbarRounding: 3f, WindowBorderSize: 1f, FrameBorderSize: 1f
        ),
        Typography: new ThemeTypography(),
        IsBuiltIn: true,
        ChatColors: new ThemeChatColors(new Dictionary<HellionChat.Code.ChatType, uint>
        {
            // Synthwave Sunset — Magenta dominiert die warmen Channels (Yell/Shout/FC),
            // Cyan dominiert die kühlen (Tell/Party). Neon-Akzente für Status-nahe Channels.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#F0DFFF"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#FF2D95"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#FF6BB6"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#00F0FF"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#5CFFFE"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#5CFFFE"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#FF8C00"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#FF2D95"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#39FF14"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#5CFFFE"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#39FF14"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#FF8C00"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#FFD700"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#00F0FF"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#FF6BB6"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#FF2D95"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#A88BC4"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#5CFFFE"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#FF6BB6"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#A88BC4"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#A88BC4"),
        })
    );
}
