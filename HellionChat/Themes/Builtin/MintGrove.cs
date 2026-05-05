using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class MintGrove
{
    public const string Slug = "mint-grove";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Mint Grove",
        Author: "Hellion Online Media",
        Description: "Mint Green + Honey Amber auf Deep Forest. Naturthemen-tauglich.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#3CB371"),
            Primary:        ColourUtil.HexToRgba("#5DD39E"),
            PrimaryLight:   ColourUtil.HexToRgba("#8FE0B8"),
            PrimaryGlow:    ColourUtil.HexToRgba("#5DD39E99"),

            AccentDark:     ColourUtil.HexToRgba("#F4C870"),
            Accent:         ColourUtil.HexToRgba("#F9D580"),
            AccentLight:    ColourUtil.HexToRgba("#FCDD93"),

            Identity:       ColourUtil.HexToRgba("#5DD39E"),

            WindowBg:       ColourUtil.HexToRgba("#0A1410"),
            ChildBg:        ColourUtil.HexToRgba("#10201A"),
            FrameBg:        ColourUtil.HexToRgba("#162B22"),
            Surface:        ColourUtil.HexToRgba("#1E372B"),
            SurfaceHover:   ColourUtil.HexToRgba("#284335"),
            Border:         ColourUtil.HexToRgba("#5DD39E55"),

            TextPrimary:    ColourUtil.HexToRgba("#E8F5EA"),
            TextMuted:      ColourUtil.HexToRgba("#9BB5A5"),
            TextDim:        ColourUtil.HexToRgba("#5C6F65"),

            StatusSuccess:  ColourUtil.HexToRgba("#5DD39E"),
            StatusDanger:   ColourUtil.HexToRgba("#D9534F"),
            StatusWarning:  ColourUtil.HexToRgba("#E8B590"),
            StatusInfo:     ColourUtil.HexToRgba("#5DA9C7")
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
            // Mint Grove — Naturthemen-Tönung: Honey-Amber in Yell-Familie,
            // Mint-Drift in NoviceNetwork und Linkshell. Tell-Pink-Identität
            // bleibt erhalten für Erkennbarkeit.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#E8F5EA"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F9D580"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#F0A050"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#F098C8"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#F098C8"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#80B8D0"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#F0B070"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#80C8B0"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#8FE0B8"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#80B8D0"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#8FE0B8"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#F0BC80"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F9D580"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#80E0A0"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#80B8D0"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#A89DC0"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#F098C8"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#D0A8C8"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E8C088"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E8C088"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#9BB5A5"),
        })
    );
}
