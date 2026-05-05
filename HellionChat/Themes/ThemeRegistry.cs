using HellionChat.Themes.Builtin;

namespace HellionChat.Themes;

public sealed class ThemeRegistry
{
    public const string DefaultSlug = HellionArctic.Slug;

    private readonly Dictionary<string, Theme> _builtIns;
    private Theme _active;

    public ThemeRegistry()
    {
        _builtIns = new Dictionary<string, Theme>(StringComparer.OrdinalIgnoreCase)
        {
            { HellionArctic.Slug, HellionArctic.Build() },
            { Chat2Classic.Slug,  Chat2Classic.Build() },
            { EventHorizon.Slug,  EventHorizon.Build() },
            { MoonlitBloom.Slug,  MoonlitBloom.Build() },
            { MintGrove.Slug,     MintGrove.Build() },
        };
        _active = _builtIns[DefaultSlug];
    }

    public Theme Active => _active;

    // Active-Lookup: liefert das angeforderte Theme oder fällt auf Hellion Arctic
    // zurück, wenn der Slug unbekannt ist.
    public Theme Get(string slug)
    {
        return _builtIns.TryGetValue(slug, out var theme)
            ? theme
            : _builtIns[DefaultSlug];
    }

    public IEnumerable<Theme> AllBuiltIns() => _builtIns.Values;

    public void Switch(string slug)
    {
        _active = Get(slug);
    }
}
