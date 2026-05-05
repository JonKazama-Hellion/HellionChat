using Dalamud.Interface;

namespace HellionChat.Ui;

/// <summary>
/// Default-Icon-Mapping für Tabs. v1.2.0 Layout-Refresh nutzt das
/// in Top-Tabs (Icon-Prefix) und Sidebar (Icon-only mit Tooltip).
/// User können in Settings → Tabs per Tab.Icon-Override eigene
/// FontAwesome-Glyphen setzen.
///
/// Aufteilung:
/// - <see cref="ResolveGlyphName(Tab)"/> + <see cref="PickerOptions"/>
///   sind reine String-Logik und stehen aus Test-Sicht ohne
///   Dalamud-Dependency zur Verfügung (siehe Hilfsklasse
///   <see cref="TabIconGlyphResolver"/> in derselben Datei, die ohne
///   Dalamud-Imports auskommt).
/// - <see cref="Resolve(Tab)"/> liefert den FontAwesomeIcon-Enum-Wert
///   für Render-Code (T3/T5/T7) und ist daher Dalamud-abhängig.
/// </summary>
internal static class TabIconMapping
{
    /// <summary>
    /// FontAwesome-Glyph-Name → Icon-Enum-Lookup. Wird für die
    /// Production-Resolve-API benötigt. Enthält nur Glyphen aus
    /// <see cref="TabIconGlyphResolver.PickerOptions"/>.
    /// </summary>
    private static readonly Dictionary<string, FontAwesomeIcon> GlyphLookup = new(StringComparer.OrdinalIgnoreCase)
    {
        ["comment"] = FontAwesomeIcon.Comment,
        ["comments"] = FontAwesomeIcon.Comments,
        ["cog"] = FontAwesomeIcon.Cog,
        ["users"] = FontAwesomeIcon.Users,
        ["user-friends"] = FontAwesomeIcon.UserFriends,
        ["link"] = FontAwesomeIcon.Link,
        ["envelope"] = FontAwesomeIcon.Envelope,
        ["clock"] = FontAwesomeIcon.Clock,
        ["hashtag"] = FontAwesomeIcon.Hashtag,
        ["star"] = FontAwesomeIcon.Star,
        ["heart"] = FontAwesomeIcon.Heart,
        ["bell"] = FontAwesomeIcon.Bell,
        ["bookmark"] = FontAwesomeIcon.Bookmark,
        ["flag"] = FontAwesomeIcon.Flag,
        ["fire"] = FontAwesomeIcon.Fire,
    };

    /// <summary>
    /// Picker-Options-Pool — Pass-through zu <see cref="TabIconGlyphResolver"/>.
    /// </summary>
    public static IReadOnlyList<string> PickerOptions => TabIconGlyphResolver.PickerOptions;

    /// <summary>
    /// Pass-through zu <see cref="TabIconGlyphResolver.ResolveGlyphName(Tab)"/>.
    /// Liegt hier nochmal als Convenience-Alias, damit Aufrufer nur
    /// <see cref="TabIconMapping"/> kennen müssen.
    /// </summary>
    public static string ResolveGlyphName(Tab tab) => TabIconGlyphResolver.ResolveGlyphName(tab);

    /// <summary>
    /// Production-Surface: liefert das Icon für einen Tab. Wrapper um
    /// <see cref="TabIconGlyphResolver.ResolveGlyphName(Tab)"/> plus
    /// Enum-Lookup. Wird von Render-Code (T3, T5, T7) verwendet.
    /// </summary>
    public static FontAwesomeIcon Resolve(Tab tab)
    {
        var glyph = TabIconGlyphResolver.ResolveGlyphName(tab);
        return GlyphLookup.TryGetValue(glyph, out var icon)
            ? icon
            : FontAwesomeIcon.Hashtag;
    }
}

/// <summary>
/// Reine String-Resolver-Logik ohne Dalamud-Dependency. Bewusst
/// separat, damit Tests (HellionChat.Tests, Microsoft.NET.Sdk ohne
/// Dalamud-Reference) sie aufrufen können, ohne dass die JIT beim
/// Methodenaufruf die Dalamud-Assembly laden muss.
/// </summary>
internal static class TabIconGlyphResolver
{
    /// <summary>
    /// Glyph-Set, das überhaupt als Override akzeptiert wird. Spiegelt
    /// die Keys aus <see cref="TabIconMapping.GlyphLookup"/>.
    /// </summary>
    private static readonly HashSet<string> KnownGlyphs = new(StringComparer.OrdinalIgnoreCase)
    {
        "comment", "comments", "cog", "users", "user-friends", "link",
        "envelope", "clock", "hashtag", "star", "heart", "bell",
        "bookmark", "flag", "fire",
    };

    /// <summary>
    /// Picker-Options-Pool. Wird im Settings-Tab Icon-Combobox angezeigt.
    /// Reihenfolge ist die UI-Reihenfolge.
    /// </summary>
    public static readonly IReadOnlyList<string> PickerOptions =
        ["comment", "comments", "cog", "users", "user-friends", "link",
         "envelope", "clock", "hashtag", "star", "heart", "bell",
         "bookmark", "flag", "fire"];

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
    /// 1. Tab.Icon-Override (falls gesetzt):
    ///    a) bekannter Glyph → diesen Glyph
    ///    b) unbekannter Glyph → harter Fallback "hashtag" (User hat
    ///       bewusst etwas gesetzt, also überstimmt das die Defaults)
    /// 2. Auto-Tell-Tab → "clock"
    /// 3. Tab-Name-Default (<see cref="NameDefaults"/>-Lookup)
    /// 4. Fallback "hashtag"
    /// </summary>
    public static string ResolveGlyphName(Tab tab)
    {
        if (!string.IsNullOrEmpty(tab.Icon))
            return KnownGlyphs.Contains(tab.Icon) ? tab.Icon : "hashtag";

        if (tab.IsTempTab)
            return "clock";

        if (NameDefaults.TryGetValue(tab.Name, out var byName))
            return byName;

        return "hashtag";
    }
}
