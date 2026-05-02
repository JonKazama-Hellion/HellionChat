using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Information : ISettingsTab
{
    private readonly Configuration Mutable;

    public string Name => HellionStrings.Settings_Tab_Information + "###tabs-information";

    internal Information(Configuration mutable)
    {
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        // About-Inhalt zieht in Plan-Task 10 ein.
    }
}
