using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class HellionArctic
{
    public const string Slug = "hellion-arctic";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Hellion Arctic",
        Author: "Hellion Online Media",
        Description: "Arctic Cyan + Ember Glow on industrial slate. Plugin default.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#0097A7"),
            Primary:        ColourUtil.HexToRgba("#00BED2"),
            PrimaryLight:   ColourUtil.HexToRgba("#4DD9E8"),
            PrimaryGlow:    ColourUtil.HexToRgba("#00BED299"),

            AccentDark:     ColourUtil.HexToRgba("#E85D04"),
            Accent:         ColourUtil.HexToRgba("#F97316"),
            AccentLight:    ColourUtil.HexToRgba("#FB923C"),

            Identity:       ColourUtil.HexToRgba("#0097A7"),

            WindowBg:       ColourUtil.HexToRgba("#070B12"),
            ChildBg:        ColourUtil.HexToRgba("#0C1220"),
            FrameBg:        ColourUtil.HexToRgba("#141E30"),
            Surface:        ColourUtil.HexToRgba("#1A2538"),
            SurfaceHover:   ColourUtil.HexToRgba("#22303F"),
            Border:         ColourUtil.HexToRgba("#00BED266"),

            TextPrimary:    ColourUtil.HexToRgba("#E6F4F1"),
            TextMuted:      ColourUtil.HexToRgba("#8FA3B5"),
            TextDim:        ColourUtil.HexToRgba("#566273"),

            StatusSuccess:  ColourUtil.HexToRgba("#5CB85C"),
            StatusDanger:   ColourUtil.HexToRgba("#D9534F"),
            StatusWarning:  ColourUtil.HexToRgba("#F0AD4E"),
            StatusInfo:     ColourUtil.HexToRgba("#00BED2")
        ),
        Layout: new ThemeLayout(
            WindowRounding: 4f, ChildRounding: 3f, PopupRounding: 3f,
            FrameRounding: 2f, GrabRounding: 2f, TabRounding: 2f,
            ScrollbarRounding: 2f, WindowBorderSize: 1f, FrameBorderSize: 1f
        ),
        Typography: new ThemeTypography(),
        IsBuiltIn: true
    );
}
