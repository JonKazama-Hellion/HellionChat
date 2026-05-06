using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class ForgeMerchantman
{
    public const string Slug = "forge-merchantman";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Forge Merchantman",
        Author: "Hellion Online Media",
        Description: "Patina Bronze auf Workshop-Slate — Hellion Forge im Plugin.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#1F8A82"),
            Primary:        ColourUtil.HexToRgba("#2DB39E"),
            PrimaryLight:   ColourUtil.HexToRgba("#4FC9B0"),
            PrimaryGlow:    ColourUtil.HexToRgba("#2DB39E99"),

            AccentDark:     ColourUtil.HexToRgba("#B86A20"),
            Accent:         ColourUtil.HexToRgba("#D9892C"),
            AccentLight:    ColourUtil.HexToRgba("#E8A04A"),

            Identity:       ColourUtil.HexToRgba("#1F8A82"),

            WindowBg:       ColourUtil.HexToRgba("#050B0A"),
            ChildBg:        ColourUtil.HexToRgba("#0B1413"),
            FrameBg:        ColourUtil.HexToRgba("#11201D"),
            Surface:        ColourUtil.HexToRgba("#182925"),
            SurfaceHover:   ColourUtil.HexToRgba("#213631"),
            Border:         ColourUtil.HexToRgba("#2DB39E66"),

            TextPrimary:    ColourUtil.HexToRgba("#D8EFE8"),
            TextMuted:      ColourUtil.HexToRgba("#8FA39B"),
            TextDim:        ColourUtil.HexToRgba("#5A6E66"),

            StatusSuccess:  ColourUtil.HexToRgba("#5CB85C"),
            StatusDanger:   ColourUtil.HexToRgba("#D9534F"),
            StatusWarning:  ColourUtil.HexToRgba("#F0AD4E"),
            StatusInfo:     ColourUtil.HexToRgba("#2DB39E")
        ),
        Layout: new ThemeLayout(
            WindowRounding: 4f, ChildRounding: 3f, PopupRounding: 3f,
            FrameRounding: 2f, GrabRounding: 2f, TabRounding: 2f,
            ScrollbarRounding: 2f, WindowBorderSize: 1f, FrameBorderSize: 1f
        ),
        Typography: new ThemeTypography(),
        IsBuiltIn: true,
        ChatColors: new ThemeChatColors(new Dictionary<HellionChat.Code.ChatType, uint>
        {
            // Forge Merchantman — Patina-Tinte in Party/FC, Bernstein-Tinte in
            // Yell/Alliance/CustomEmote. Channel-identity bleibt voll erhalten.
            [HellionChat.Code.ChatType.Say]              = ColourUtil.HexToRgba("#FFFFFF"),
            [HellionChat.Code.ChatType.Yell]             = ColourUtil.HexToRgba("#F0C060"),
            [HellionChat.Code.ChatType.Shout]            = ColourUtil.HexToRgba("#E8902C"),
            [HellionChat.Code.ChatType.TellIncoming]     = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.TellOutgoing]     = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.Party]            = ColourUtil.HexToRgba("#6AC9B0"),
            [HellionChat.Code.ChatType.Alliance]         = ColourUtil.HexToRgba("#E8A04A"),
            [HellionChat.Code.ChatType.FreeCompany]      = ColourUtil.HexToRgba("#4FB8A0"),
            [HellionChat.Code.ChatType.NoviceNetwork]    = ColourUtil.HexToRgba("#A8E060"),
            [HellionChat.Code.ChatType.CrossParty]       = ColourUtil.HexToRgba("#6AC9B0"),
            [HellionChat.Code.ChatType.Linkshell1]       = ColourUtil.HexToRgba("#A8E060"),
            [HellionChat.Code.ChatType.Linkshell2]       = ColourUtil.HexToRgba("#E8A04A"),
            [HellionChat.Code.ChatType.Linkshell3]       = ColourUtil.HexToRgba("#F0C060"),
            [HellionChat.Code.ChatType.Linkshell4]       = ColourUtil.HexToRgba("#80E8B0"),
            [HellionChat.Code.ChatType.Linkshell5]       = ColourUtil.HexToRgba("#6AC9B0"),
            [HellionChat.Code.ChatType.Linkshell6]       = ColourUtil.HexToRgba("#A8A0F0"),
            [HellionChat.Code.ChatType.Linkshell7]       = ColourUtil.HexToRgba("#FF99CC"),
            [HellionChat.Code.ChatType.Linkshell8]       = ColourUtil.HexToRgba("#E8B0F0"),
            [HellionChat.Code.ChatType.CustomEmote]      = ColourUtil.HexToRgba("#E8C880"),
            [HellionChat.Code.ChatType.StandardEmote]    = ColourUtil.HexToRgba("#E8C880"),
            [HellionChat.Code.ChatType.Echo]             = ColourUtil.HexToRgba("#8FA39B"),
        })
    );
}
