using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.SelfTest;
using HellionChat.Themes;

namespace HellionChat.SelfTests;

// Validates the runtime theme-switch contract from the user side. The
// caller toggles the active theme via Settings -> Theme & Layout, the
// step polls ThemeRegistry.Active per frame and only passes once the
// slug has moved away from the initial value and back. The ABGR cache
// is sanity-checked on every frame: a freshly switched theme must carry
// a populated cache, otherwise Switch() forgot the recompute and the UI
// would still draw, just with all-transparent slots.
internal sealed class ThemeSwitchSelfTestStep : ISelfTestStep
{
    private readonly Plugin plugin;
    private string? initialSlug;
    private bool switchedAway;

    public ThemeSwitchSelfTestStep(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public string Name => "Hellion Chat - Theme switch";

    public SelfTestStepResult RunStep()
    {
        var registry = this.plugin.ThemeRegistry;
        if (registry is null)
            return SelfTestStepResult.Fail;

        var active = registry.Active;
        if (active is null)
            return SelfTestStepResult.Fail;

        if (!HasPopulatedCache(active))
            return SelfTestStepResult.Fail;

        if (this.initialSlug is null)
        {
            this.initialSlug = active.Slug;
            ImGui.Text($"Initial theme: \"{this.initialSlug}\". Open Settings -> Theme & Layout and pick a different theme.");
            return SelfTestStepResult.Waiting;
        }

        if (!this.switchedAway)
        {
            if (!string.Equals(active.Slug, this.initialSlug, StringComparison.OrdinalIgnoreCase))
            {
                this.switchedAway = true;
                return SelfTestStepResult.Waiting;
            }

            ImGui.Text($"Switch the active theme away from \"{this.initialSlug}\".");
            return SelfTestStepResult.Waiting;
        }

        if (!string.Equals(active.Slug, this.initialSlug, StringComparison.OrdinalIgnoreCase))
        {
            ImGui.Text($"Switch back to \"{this.initialSlug}\" to finish the test.");
            return SelfTestStepResult.Waiting;
        }

        return SelfTestStepResult.Pass;
    }

    public void CleanUp()
    {
        this.initialSlug = null;
        this.switchedAway = false;
    }

    // Any non-zero slot proves the cache was actually recomputed for the
    // current theme. We don't compare against a reference, because custom
    // themes can legitimately share slot values with a built-in.
    private static bool HasPopulatedCache(Theme theme)
    {
        var cache = theme.AbgrCache;
        return (cache.Primary
              | cache.WindowBg
              | cache.TextPrimary
              | cache.Border) != 0u;
    }
}
