using HellionChat.Util;

namespace HellionChat.Themes;

public sealed record Theme(
    string Slug,
    string Name,
    string Author,
    string Description,
    ThemeColors Colors,
    ThemeLayout Layout,
    ThemeTypography Typography,
    bool IsBuiltIn,
    ThemeChatColors? ChatColors = null
)
{
    // Pre-computed ABGR mirror of ThemeColors so PushGlobal can skip the
    // RgbaToAbgr conversion per slot per frame.
    public ThemeAbgrCache AbgrCache { get; private set; }

    public void RecomputeAbgrCache()
    {
        AbgrCache = new ThemeAbgrCache(
            PrimaryDark:   ColourUtil.RgbaToAbgr(Colors.PrimaryDark),
            Primary:       ColourUtil.RgbaToAbgr(Colors.Primary),
            PrimaryLight:  ColourUtil.RgbaToAbgr(Colors.PrimaryLight),
            PrimaryGlow:   ColourUtil.RgbaToAbgr(Colors.PrimaryGlow),
            AccentDark:    ColourUtil.RgbaToAbgr(Colors.AccentDark),
            Accent:        ColourUtil.RgbaToAbgr(Colors.Accent),
            AccentLight:   ColourUtil.RgbaToAbgr(Colors.AccentLight),
            Identity:      ColourUtil.RgbaToAbgr(Colors.Identity),
            WindowBg:      ColourUtil.RgbaToAbgr(Colors.WindowBg),
            ChildBg:       ColourUtil.RgbaToAbgr(Colors.ChildBg),
            FrameBg:       ColourUtil.RgbaToAbgr(Colors.FrameBg),
            Surface:       ColourUtil.RgbaToAbgr(Colors.Surface),
            SurfaceHover:  ColourUtil.RgbaToAbgr(Colors.SurfaceHover),
            Border:        ColourUtil.RgbaToAbgr(Colors.Border),
            TextPrimary:   ColourUtil.RgbaToAbgr(Colors.TextPrimary),
            TextMuted:     ColourUtil.RgbaToAbgr(Colors.TextMuted),
            TextDim:       ColourUtil.RgbaToAbgr(Colors.TextDim),
            StatusSuccess: ColourUtil.RgbaToAbgr(Colors.StatusSuccess),
            StatusDanger:  ColourUtil.RgbaToAbgr(Colors.StatusDanger),
            StatusWarning: ColourUtil.RgbaToAbgr(Colors.StatusWarning),
            StatusInfo:    ColourUtil.RgbaToAbgr(Colors.StatusInfo));
    }
}

// Mirrors ThemeColors slot-for-slot. The FillsAll21Slots test pins the
// contract — a new slot without its mirror fails the build.
public readonly record struct ThemeAbgrCache(
    uint PrimaryDark, uint Primary, uint PrimaryLight, uint PrimaryGlow,
    uint AccentDark, uint Accent, uint AccentLight,
    uint Identity,
    uint WindowBg, uint ChildBg, uint FrameBg,
    uint Surface, uint SurfaceHover, uint Border,
    uint TextPrimary, uint TextMuted, uint TextDim,
    uint StatusSuccess, uint StatusDanger, uint StatusWarning, uint StatusInfo
);
