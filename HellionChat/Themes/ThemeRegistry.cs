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
            { HellionSpectrum.Slug,   HellionSpectrum.Build() },
            { Chat2Classic.Slug,      Chat2Classic.Build() },
            { EventHorizon.Slug,      EventHorizon.Build() },
            { MoonlitBloom.Slug,      MoonlitBloom.Build() },
            { NightBlue.Slug,         NightBlue.Build() },
            { IndigoViolet.Slug,      IndigoViolet.Build() },
            { ForgeMerchantman.Slug,  ForgeMerchantman.Build() },
            { MintGrove.Slug,         MintGrove.Build() },
            { SynthwaveSunset.Slug,   SynthwaveSunset.Build() },
        };

        // Centralised so the ten .Build() factories stay free of cache plumbing.
        foreach (var theme in _builtIns.Values)
            theme.RecomputeAbgrCache();

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

    public void Switch(string slug)
    {
        var theme = Get(slug);
        // Defensive — idempotent and cheap, so any future theme source
        // that forgets the cache fill still ends up with a populated one.
        theme.RecomputeAbgrCache();
        _active = theme;
    }

    // 0x80070020 = SHARING_VIOLATION, 0x80070021 = LOCK_VIOLATION. Other
    // IO failures are permanent and get the theme dropped instead of retried.
    internal static bool IsRecoverableFileLock(Exception? ex)
    {
        if (ex is not IOException io)
            return false;
        var code = (uint)io.HResult;
        return code == 0x80070020u || code == 0x80070021u;
    }

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
                    theme.RecomputeAbgrCache();
                    _customCache[key] = (theme, stamp);
                }
                catch (Exception ex) when (IsRecoverableFileLock(ex))
                {
                    // Editor mid-save: keep the cached snapshot, leave the stamp
                    // alone so the next refresh retries automatically.
                    Plugin.Log.Debug($"Custom theme {Path.GetFileName(path)} is locked, keeping last known good");
                    if (cached.Theme is not null)
                        theme = cached.Theme;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (theme is not null && seenSlugs.Add(theme.Slug))
                yield return theme;
        }
    }
}
