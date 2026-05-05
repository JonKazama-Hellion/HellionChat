using HellionChat.Util;

namespace HellionChat.Themes.Builtin;

internal static class EventHorizon
{
    public const string Slug = "event-horizon";

    public static Theme Build() => new(
        Slug: Slug,
        Name: "Event Horizon",
        Author: "Hellion Online Media",
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
        IsBuiltIn: true
    );
}
