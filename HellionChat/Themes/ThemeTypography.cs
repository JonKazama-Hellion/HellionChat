namespace HellionChat.Themes;

// Optional pro Theme. v1.1.0 nutzt das nicht aktiv; ist als Erweiterungspunkt
// für zukünftige Theme-Slots vorbereitet.
public sealed record ThemeTypography(
    float? OverrideGlobalFontSizePt = null,
    float? OverrideSymbolsFontSizePt = null
);
