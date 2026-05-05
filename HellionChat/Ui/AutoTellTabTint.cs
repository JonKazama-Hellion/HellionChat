namespace HellionChat.Ui;

/// <summary>
/// Hash-Color-Tinting für Auto-Tell-Tabs in der Sidebar (v1.2.0).
/// Differenziert Tells visuell ohne dass User pro Tab manuell ein
/// Custom-Icon setzen muss. Gleicher Tell-Partner (Name+World) liefert
/// konsistent dieselbe Farbe über Sessions hinweg.
///
/// Kuratierte 12-Farb-Palette aus dem Hellion-Theme-Pool: alle saturiert
/// mid-bright, lesbar gegen Dark-Theme-Backgrounds. Bei realistischen
/// 1-5 parallelen Tells ist Kollisions-Wahrscheinlichkeit gering.
///
/// Reine String-Logik (kein Dalamud-Dep) — testbar im HellionChat.Tests-
/// Projekt das ohne Dalamud-Reference baut.
/// </summary>
internal static class AutoTellTabTint
{
    /// <summary>
    /// Fallback bei ungültigem Input (leerer Name, World=0). Standard-
    /// Text-Color (weiß) — passt mit existierendem TextPrimary-Default
    /// zusammen, sodass die Sidebar visuell konsistent bleibt.
    /// </summary>
    public const uint Fallback = 0xFFFFFFFFu;

    /// <summary>
    /// 12 saturierte mid-bright Farben aus den 5 Built-In-Themes
    /// (Hellion-Arctic, Chat2-Klassik, Event-Horizon, Moonlit-Bloom,
    /// Mint-Grove). Reihenfolge ist deterministisch — Hash-Index wählt
    /// Farbe per Modulo. RGBA-Format (passt zu ColourUtil.RgbaToAbgr-
    /// Konvention im restlichen Code).
    /// </summary>
    public static readonly IReadOnlyList<uint> Palette = new uint[]
    {
        0x00BED2FFu, // Arctic Cyan
        0xF97316FFu, // Ember Orange
        0xB585FFFFu, // Light Cosmic Purple
        0xE374E8FFu, // Bloom Magenta
        0x5DD39EFFu, // Mint Green
        0xF0AD4EFFu, // Warning Yellow
        0xE85C6AFFu, // Coral
        0x5CB85CFFu, // Status Green
        0x6278FFFFu, // Bloom Blue
        0xC9982EFFu, // Warm Gold
        0x9CCB7CFFu, // Soft Sage
        0xE85D04FFu, // Deep Ember
    };

    /// <summary>
    /// Liefert eine konsistente Tint-Color für einen Tell-Partner.
    /// Hash basiert auf "Name@World" — Cross-World-Namen kollidieren
    /// nur bei Hash-Bucket-Kollision, nicht durch Identitäts-Annahme.
    /// </summary>
    public static uint For(string name, uint world)
    {
        if (string.IsNullOrEmpty(name) || world == 0)
            return Fallback;

        // GetHashCode kann negativ sein; Bitmaske auf positive Range
        // damit Modulo-Division immer einen validen Index liefert.
        var key = $"{name}@{world}";
        var hash = (uint)(key.GetHashCode() & 0x7FFFFFFF);
        return Palette[(int)(hash % Palette.Count)];
    }

    /// <summary>
    /// Tell-spezifischer Icon-Pool. 7 visuell distinkte FontAwesome-Glyphen
    /// die im Tell-Kontext sinnvoll wirken (envelope = Tell-Default, star/
    /// heart/bell = personalisiert, bookmark/flag/fire = markiert/wichtig).
    /// Bewusst kein cog/comment/users — die wären für System-/Group-Tabs
    /// reserviert und würden im Tell-Bereich verwirrend wirken.
    /// </summary>
    public static readonly IReadOnlyList<string> IconPool = new[]
    {
        "envelope",
        "star",
        "heart",
        "bell",
        "bookmark",
        "flag",
        "fire",
    };

    /// <summary>
    /// Fallback-Icon bei ungültigem Input. "envelope" passt semantisch zum
    /// Tell-Kontext besser als das alte hardcoded "clock".
    /// </summary>
    public const string IconFallback = "envelope";

    /// <summary>
    /// Liefert ein konsistentes Icon-Glyph für einen Tell-Partner.
    /// Nutzt einen anderen Hash-Bias als For() (Color), damit Icon und
    /// Color unabhängig variieren — gibt 7 × 12 = 84 distinct Combinations.
    /// </summary>
    public static string IconFor(string name, uint world)
    {
        if (string.IsNullOrEmpty(name) || world == 0)
            return IconFallback;

        // Anderer Hash-Bias als For() (verschiedene Modulo-Basis): wir
        // nutzen "world@name" statt "name@world" damit Icon und Color
        // nicht synchron variieren. Ohne Bias-Trennung würden alle Tells
        // mit derselben Color auch dasselbe Icon haben.
        var key = $"{world}@{name}";
        var hash = (uint)(key.GetHashCode() & 0x7FFFFFFF);
        return IconPool[(int)(hash % IconPool.Count)];
    }
}
