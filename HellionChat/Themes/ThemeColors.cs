namespace HellionChat.Themes;

// Color-Werte als 0xRRGGBBAA, RgbaToAbgr handled den Byte-Swap zu ImGui.
public sealed record ThemeColors(
    uint PrimaryDark,
    uint Primary,
    uint PrimaryLight,
    uint PrimaryGlow,

    uint AccentDark,
    uint Accent,
    uint AccentLight,

    uint Identity,

    uint WindowBg,
    uint ChildBg,
    uint FrameBg,
    uint Surface,
    uint SurfaceHover,
    uint Border,

    uint TextPrimary,
    uint TextMuted,
    uint TextDim,

    uint StatusSuccess,
    uint StatusDanger,
    uint StatusWarning,
    uint StatusInfo
);
