using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Appearance : ISettingsTab
{
    private readonly Plugin Plugin;
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Appearance + "###tabs-appearance";

    internal Appearance(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        // Settings ziehen in Plan-Task 4 ein.
    }
}
