using System.Text.Json;

namespace HellionChat.Themes;

internal static class ThemeJsonWriter
{
    public static string Serialize(Theme theme)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", ThemeJsonLoader.SupportedSchemaVersion);
            writer.WriteString("slug", theme.Slug);
            writer.WriteString("name", theme.Name);
            writer.WriteString("author", theme.Author);
            writer.WriteString("description", theme.Description);

            writer.WriteStartObject("colors");
            WriteColor(writer, "primaryDark",   theme.Colors.PrimaryDark);
            WriteColor(writer, "primary",       theme.Colors.Primary);
            WriteColor(writer, "primaryLight",  theme.Colors.PrimaryLight);
            WriteColor(writer, "primaryGlow",   theme.Colors.PrimaryGlow);
            WriteColor(writer, "accentDark",    theme.Colors.AccentDark);
            WriteColor(writer, "accent",        theme.Colors.Accent);
            WriteColor(writer, "accentLight",   theme.Colors.AccentLight);
            WriteColor(writer, "identity",      theme.Colors.Identity);
            WriteColor(writer, "windowBg",      theme.Colors.WindowBg);
            WriteColor(writer, "childBg",       theme.Colors.ChildBg);
            WriteColor(writer, "frameBg",       theme.Colors.FrameBg);
            WriteColor(writer, "surface",       theme.Colors.Surface);
            WriteColor(writer, "surfaceHover",  theme.Colors.SurfaceHover);
            WriteColor(writer, "border",        theme.Colors.Border);
            WriteColor(writer, "textPrimary",   theme.Colors.TextPrimary);
            WriteColor(writer, "textMuted",     theme.Colors.TextMuted);
            WriteColor(writer, "textDim",       theme.Colors.TextDim);
            WriteColor(writer, "statusSuccess", theme.Colors.StatusSuccess);
            WriteColor(writer, "statusDanger",  theme.Colors.StatusDanger);
            WriteColor(writer, "statusWarning", theme.Colors.StatusWarning);
            WriteColor(writer, "statusInfo",    theme.Colors.StatusInfo);
            writer.WriteEndObject();

            writer.WriteStartObject("layout");
            writer.WriteNumber("windowRounding",    theme.Layout.WindowRounding);
            writer.WriteNumber("childRounding",     theme.Layout.ChildRounding);
            writer.WriteNumber("popupRounding",     theme.Layout.PopupRounding);
            writer.WriteNumber("frameRounding",     theme.Layout.FrameRounding);
            writer.WriteNumber("grabRounding",      theme.Layout.GrabRounding);
            writer.WriteNumber("tabRounding",       theme.Layout.TabRounding);
            writer.WriteNumber("scrollbarRounding", theme.Layout.ScrollbarRounding);
            writer.WriteNumber("windowBorderSize",  theme.Layout.WindowBorderSize);
            writer.WriteNumber("frameBorderSize",   theme.Layout.FrameBorderSize);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
    }

    private static void WriteColor(Utf8JsonWriter writer, string key, uint rgba)
    {
        writer.WriteString(key, $"#{rgba:X8}");
    }
}
