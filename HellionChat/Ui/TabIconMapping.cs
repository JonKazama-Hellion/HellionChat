using Dalamud.Interface;

namespace HellionChat.Ui;

/// <summary>
/// Default-Icon-Mapping für Tabs. v1.2.0 Layout-Refresh nutzt das
/// in Top-Tabs (Icon-Prefix) und Sidebar (Icon-only mit Tooltip).
/// User können in Settings → Tabs per Tab.Icon-Override eigene
/// FontAwesome-Glyphen setzen.
///
/// Diese Klasse ist Dalamud-abhängig (FontAwesomeIcon-Enum). Die
/// reine String-Resolver-Logik liegt bewusst in
/// <see cref="TabIconGlyphResolver"/> (eigene Datei, ohne
/// Dalamud-Imports), damit Tests sie ohne Dalamud-Reference aufrufen
/// können.
/// </summary>
internal static class TabIconMapping
{
    /// <summary>
    /// FontAwesome-Glyph-Name → Icon-Enum-Lookup. Wird für die
    /// Production-Resolve-API benötigt.
    ///
    /// INVARIANTE: Jeder Key in <see cref="GlyphLookup"/> muss auch in
    /// <see cref="TabIconGlyphResolver.PickerOptions"/> stehen. Wird
    /// ein Glyph zu PickerOptions hinzugefügt, aber nicht hier, fällt
    /// die Override-Auflösung still auf <see cref="FontAwesomeIcon.Hashtag"/>
    /// zurück (degraded, kein Crash). Build-Time-Enforcement ist nicht
    /// möglich, weil PickerOptions ohne Dalamud-Reference auskommt.
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
    /// Production-Surface: liefert das Icon für einen Tab. Wrapper um
    /// <see cref="TabIconGlyphResolver.ResolveGlyphName(Tab)"/> plus
    /// Enum-Lookup. Wird von Render-Code (T3, T5) verwendet.
    /// </summary>
    public static FontAwesomeIcon Resolve(Tab tab)
    {
        // v1.2.0 — Auto-Tell-Tabs bekommen ein per-Partner gehashtes
        // Icon aus dem Tell-Pool. Damit unterscheiden sich parallele
        // Tells nicht nur über die Color (For), sondern auch über die
        // Glyph-Form. Berechnung bleibt hier (Dalamud-bound), weil
        // TellTarget Dalamud-Imports hat.
        string? autoTellGlyph = null;
        if (tab.IsTempTab && tab.TellTarget != null && tab.TellTarget.IsSet())
        {
            autoTellGlyph = TabTintCache.GetIcon(tab);
        }

        var glyph = TabIconGlyphResolver.ResolveGlyphName(tab, autoTellGlyph);
        return GlyphLookup.TryGetValue(glyph, out var icon)
            ? icon
            : FontAwesomeIcon.Hashtag;
    }
}
