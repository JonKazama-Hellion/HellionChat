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
        IsBuiltIn: true
    );
}
