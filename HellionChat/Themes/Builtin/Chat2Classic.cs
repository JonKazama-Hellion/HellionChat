using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class Chat2Classic
{
    public const string Slug = "chat2-classic";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Chat 2 Klassik",
        Author: "Upstream (Infi & Anna)",
        Description: "Steel-blue accents on neutral dark grey, eckige Kanten. Vertraut für ChatTwo-Veteranen.",
        Colors: new ThemeColors(
            PrimaryDark:    ColourUtil.HexToRgba("#3D6E92"),
            Primary:        ColourUtil.HexToRgba("#4682B4"),
            PrimaryLight:   ColourUtil.HexToRgba("#5C9DC8"),
            PrimaryGlow:    ColourUtil.HexToRgba("#4682B466"),

            AccentDark:     ColourUtil.HexToRgba("#3D6E92"),
            Accent:         ColourUtil.HexToRgba("#4682B4"),
            AccentLight:    ColourUtil.HexToRgba("#5C9DC8"),

            Identity:       ColourUtil.HexToRgba("#4682B4"),

            WindowBg:       ColourUtil.HexToRgba("#0F0F0FF2"),
            ChildBg:        ColourUtil.HexToRgba("#141414"),
            FrameBg:        ColourUtil.HexToRgba("#1A1A1A"),
            Surface:        ColourUtil.HexToRgba("#202020"),
            SurfaceHover:   ColourUtil.HexToRgba("#2C2C2C"),
            Border:         ColourUtil.HexToRgba("#404040"),

            TextPrimary:    ColourUtil.HexToRgba("#E6E6E6"),
            TextMuted:      ColourUtil.HexToRgba("#999999"),
            TextDim:        ColourUtil.HexToRgba("#666666"),

            StatusSuccess:  ColourUtil.HexToRgba("#5CB85C"),
            StatusDanger:   ColourUtil.HexToRgba("#D9534F"),
            StatusWarning:  ColourUtil.HexToRgba("#F0AD4E"),
            StatusInfo:     ColourUtil.HexToRgba("#4682B4")
        ),
        Layout: new ThemeLayout(
            WindowRounding: 0f, ChildRounding: 0f, PopupRounding: 0f,
            FrameRounding: 0f, GrabRounding: 0f, TabRounding: 0f,
            ScrollbarRounding: 0f, WindowBorderSize: 1f, FrameBorderSize: 1f
        ),
        Typography: new ThemeTypography(),
        IsBuiltIn: true
    );
}
