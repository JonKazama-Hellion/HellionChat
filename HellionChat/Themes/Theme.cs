namespace HellionChat.Themes;

public sealed record Theme(
    string Slug,
    string Name,
    string Author,
    string Description,
    ThemeColors Colors,
    ThemeLayout Layout,
    ThemeTypography Typography,
    bool IsBuiltIn
);
