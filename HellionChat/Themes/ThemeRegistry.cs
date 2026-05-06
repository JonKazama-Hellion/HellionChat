using HellionChat.Themes.Builtin;

namespace HellionChat.Themes;

public sealed class ThemeRegistry
{
    public const string DefaultSlug = HellionArctic.Slug;

    private readonly Dictionary<string, Theme> _builtIns;
    private readonly Dictionary<string, (Theme Theme, DateTime Stamp)> _customCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string? _customThemesDir;
    private Theme _active;

    public ThemeRegistry(string? customThemesDir = null)
    {
        _builtIns = new Dictionary<string, Theme>(StringComparer.OrdinalIgnoreCase)
        {
            { HellionArctic.Slug,     HellionArctic.Build() },
            { Chat2Classic.Slug,      Chat2Classic.Build() },
            { EventHorizon.Slug,      EventHorizon.Build() },
            { MoonlitBloom.Slug,      MoonlitBloom.Build() },
            { NightBlue.Slug,         NightBlue.Build() },
            { MintGrove.Slug,         MintGrove.Build() },
        };
        _active = _builtIns[DefaultSlug];
        _customThemesDir = customThemesDir;
    }

    public Theme Active => _active;

    public Theme Get(string slug)
    {
        if (_builtIns.TryGetValue(slug, out var b)) return b;

        var custom = LoadCustomBySlug(slug);
        if (custom != null) return custom;

        return _builtIns[DefaultSlug];
    }

    public IEnumerable<Theme> AllBuiltIns() => _builtIns.Values;

    public IEnumerable<Theme> AllCustom() => RefreshCustomCache();

    public void Switch(string slug) => _active = Get(slug);

    // Custom-Themes werden lazy aus dem Verzeichnis geladen, Cache mit
    // LastWriteTime-Token. Eine geänderte JSON wird beim nächsten Lookup
    // neu eingelesen.
    private Theme? LoadCustomBySlug(string slug)
    {
        if (_customThemesDir is null) return null;
        if (!Directory.Exists(_customThemesDir)) return null;

        foreach (var theme in RefreshCustomCache())
            if (string.Equals(theme.Slug, slug, StringComparison.OrdinalIgnoreCase))
                return theme;
        return null;
    }

    private IEnumerable<Theme> RefreshCustomCache()
    {
        if (_customThemesDir is null || !Directory.Exists(_customThemesDir))
            yield break;

        var seenSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(_customThemesDir, "*.json"))
        {
            Theme? theme = null;
            var stamp = File.GetLastWriteTimeUtc(path);
            var key = path;
            if (_customCache.TryGetValue(key, out var cached) && cached.Stamp == stamp)
            {
                theme = cached.Theme;
            }
            else
            {
                try
                {
                    theme = ThemeJsonLoader.LoadFromFile(path);
                    _customCache[key] = (theme, stamp);
                }
                catch (Exception ex)
                {
                    // Logging passiert in Plugin.cs durch den Aufrufer; hier still
                    // ignorieren, damit ein einzelnes kaputtes JSON nicht alle
                    // Custom-Themes blockt.
                    _ = ex;
                    continue;
                }
            }

            if (theme is not null && seenSlugs.Add(theme.Slug))
                yield return theme;
        }
    }
}
