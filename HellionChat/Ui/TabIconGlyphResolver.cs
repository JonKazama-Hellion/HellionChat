namespace HellionChat.Ui;

/// <summary>
/// Reine String-Resolver-Logik ohne Dalamud-Dependency. Bewusst in
/// eigener Datei (Dependency-Boundary auf File-Level sichtbar), damit
/// Tests (HellionChat.Tests, Microsoft.NET.Sdk ohne Dalamud-Reference)
/// sie aufrufen können, ohne dass die JIT beim Methodenaufruf die
/// Dalamud-Assembly laden muss.
///
/// Wird im Settings-UI (T7) für die Glyph-Picker-Combobox und im
/// Render-Code indirekt über <see cref="TabIconMapping.Resolve(Tab)"/>
/// verwendet.
/// </summary>
internal static class TabIconGlyphResolver
{
    /// <summary>
    /// Picker-Options-Pool — Single Source of Truth für das Glyph-Set.
    /// Reihenfolge ist die UI-Reihenfolge im Settings-Tab Icon-Combobox.
    /// </summary>
    public static readonly IReadOnlyList<string> PickerOptions =
        ["comment", "comments", "cog", "users", "user-friends", "link",
         "envelope", "clock", "hashtag", "star", "heart", "bell",
         "bookmark", "flag", "fire"];

    /// <summary>
    /// Glyph-Set, das überhaupt als Override akzeptiert wird. Aus
    /// <see cref="PickerOptions"/> abgeleitet — KnownGlyphs nie
    /// manuell pflegen.
    /// </summary>
    private static readonly HashSet<string> KnownGlyphs =
        new(PickerOptions, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tab-Name → Default-Glyph-Name. Tab.Name wird per Lokalisierung
    /// gesetzt; wir matchen daher gegen einen Pool aus DE/EN-Synonymen.
    /// </summary>
    private static readonly Dictionary<string, string> NameDefaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["allgemein"] = "comment",
        ["general"] = "comment",
        ["system"] = "cog",
        ["free company"] = "users",
        ["fc"] = "users",
        ["gruppe"] = "user-friends",
        ["group"] = "user-friends",
        ["party"] = "user-friends",
        ["linkshell"] = "link",
        ["ls"] = "link",
        ["cwls"] = "link",
        ["tells"] = "envelope",
        ["tell"] = "envelope",
    };

    /// <summary>
    /// Test-Surface: Glyph-Name-Resolver ohne Dalamud-Dependency.
    /// Reihenfolge:
    /// 1. Tab.Icon-Override (falls gesetzt und nicht nur Whitespace):
    ///    a) bekannter Glyph → diesen Glyph
    ///    b) unbekannter Glyph → harter Fallback "hashtag" (User hat
    ///       bewusst etwas gesetzt, also überstimmt das die Defaults)
    /// 2. Auto-Tell-Tab → <paramref name="autoTellGlyph"/> falls
    ///    übergeben, sonst "clock".
    /// 3. Tab-Name-Default (<see cref="NameDefaults"/>-Lookup)
    /// 4. Fallback "hashtag"
    /// </summary>
    public static string ResolveGlyphName(Tab tab, string? autoTellGlyph = null)
    {
        if (!string.IsNullOrWhiteSpace(tab.Icon))
            return KnownGlyphs.Contains(tab.Icon) ? tab.Icon : "hashtag";

        if (tab.IsTempTab)
            return autoTellGlyph ?? "clock";

        if (tab.Name is { } name && NameDefaults.TryGetValue(name, out var byName))
            return byName;

        return "hashtag";
    }
}
