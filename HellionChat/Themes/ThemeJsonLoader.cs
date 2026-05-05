using System.Text.Json;
using HellionChat.Util;

namespace HellionChat.Themes;

internal static class ThemeJsonLoader
{
    public const int SupportedSchemaVersion = 1;

    public static Theme LoadFromString(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new FormatException("Theme JSON is empty");

        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException ex) { throw new FormatException("Theme JSON is not valid JSON", ex); }

        using (doc)
        {
            var root = doc.RootElement;

            var schemaVersion = ReadInt(root, "schemaVersion");
            if (schemaVersion != SupportedSchemaVersion)
                throw new FormatException($"Unsupported schemaVersion {schemaVersion}; expected {SupportedSchemaVersion}");

            var slug        = ReadString(root, "slug");
            var name        = ReadString(root, "name");
            var author      = ReadString(root, "author");
            var description = ReadString(root, "description");

            var colors = ReadColors(root.GetProperty("colors"));
            var layout = ReadLayout(root.GetProperty("layout"));

            return new Theme(slug, name, author, description, colors, layout, new ThemeTypography(), IsBuiltIn: false);
        }
    }

    public static Theme LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return LoadFromString(json);
    }

    private static ThemeColors ReadColors(JsonElement el) => new(
        PrimaryDark:   ColourUtil.HexToRgba(ReadString(el, "primaryDark")),
        Primary:       ColourUtil.HexToRgba(ReadString(el, "primary")),
        PrimaryLight:  ColourUtil.HexToRgba(ReadString(el, "primaryLight")),
        PrimaryGlow:   ColourUtil.HexToRgba(ReadString(el, "primaryGlow")),

        AccentDark:    ColourUtil.HexToRgba(ReadString(el, "accentDark")),
        Accent:        ColourUtil.HexToRgba(ReadString(el, "accent")),
        AccentLight:   ColourUtil.HexToRgba(ReadString(el, "accentLight")),

        Identity:      ColourUtil.HexToRgba(ReadString(el, "identity")),

        WindowBg:      ColourUtil.HexToRgba(ReadString(el, "windowBg")),
        ChildBg:       ColourUtil.HexToRgba(ReadString(el, "childBg")),
        FrameBg:       ColourUtil.HexToRgba(ReadString(el, "frameBg")),
        Surface:       ColourUtil.HexToRgba(ReadString(el, "surface")),
        SurfaceHover:  ColourUtil.HexToRgba(ReadString(el, "surfaceHover")),
        Border:        ColourUtil.HexToRgba(ReadString(el, "border")),

        TextPrimary:   ColourUtil.HexToRgba(ReadString(el, "textPrimary")),
        TextMuted:     ColourUtil.HexToRgba(ReadString(el, "textMuted")),
        TextDim:       ColourUtil.HexToRgba(ReadString(el, "textDim")),

        StatusSuccess: ColourUtil.HexToRgba(ReadString(el, "statusSuccess")),
        StatusDanger:  ColourUtil.HexToRgba(ReadString(el, "statusDanger")),
        StatusWarning: ColourUtil.HexToRgba(ReadString(el, "statusWarning")),
        StatusInfo:    ColourUtil.HexToRgba(ReadString(el, "statusInfo"))
    );

    private static ThemeLayout ReadLayout(JsonElement el) => new(
        WindowRounding:    ReadFloat(el, "windowRounding"),
        ChildRounding:     ReadFloat(el, "childRounding"),
        PopupRounding:     ReadFloat(el, "popupRounding"),
        FrameRounding:     ReadFloat(el, "frameRounding"),
        GrabRounding:      ReadFloat(el, "grabRounding"),
        TabRounding:       ReadFloat(el, "tabRounding"),
        ScrollbarRounding: ReadFloat(el, "scrollbarRounding"),
        WindowBorderSize:  ReadFloat(el, "windowBorderSize"),
        FrameBorderSize:   ReadFloat(el, "frameBorderSize")
    );

    private static string ReadString(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.String)
            throw new FormatException($"Theme JSON missing string property '{name}'");
        return v.GetString() ?? throw new FormatException($"Theme JSON property '{name}' is null");
    }

    private static int ReadInt(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Number)
            throw new FormatException($"Theme JSON missing number property '{name}'");
        return v.GetInt32();
    }

    private static float ReadFloat(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Number)
            throw new FormatException($"Theme JSON missing number property '{name}'");
        return (float)v.GetDouble();
    }
}
