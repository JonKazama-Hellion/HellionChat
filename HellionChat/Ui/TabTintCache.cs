namespace HellionChat.Ui;

// Per-Tab cache wrapper around the pure AutoTellTabTint hash helpers.
// Each cache (tint, icon) carries its own name+world validation key so
// neither read path mutates the other's state — refilling one never
// invalidates the other. No string allocation in the steady-state lookup.
internal static class TabTintCache
{
    public static uint GetTint(Tab tab)
    {
        var name = tab.TellTarget.Name;
        var world = tab.TellTarget.World;
        if (tab._cachedTintTellName != name || tab._cachedTintTellWorld != world)
        {
            tab._cachedTintTellName = name;
            tab._cachedTintTellWorld = world;
            tab._cachedTellTint = AutoTellTabTint.For(name, world);
        }
        return tab._cachedTellTint;
    }

    public static string GetIcon(Tab tab)
    {
        var name = tab.TellTarget.Name;
        var world = tab.TellTarget.World;
        if (tab._cachedTellIcon is null
            || tab._cachedIconTellName != name
            || tab._cachedIconTellWorld != world)
        {
            tab._cachedIconTellName = name;
            tab._cachedIconTellWorld = world;
            tab._cachedTellIcon = AutoTellTabTint.IconFor(name, world);
        }
        return tab._cachedTellIcon;
    }
}
