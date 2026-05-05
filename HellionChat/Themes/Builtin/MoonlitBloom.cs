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
        IsBuiltIn: true
    );
}
