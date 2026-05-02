using System.Numerics;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class About : ISettingsTab
{
    public string Name => string.Format(Language.Options_About_Tab, Plugin.PluginName) + "###tabs-about";

    private readonly List<string> Translators =
    [
        "q673135110", "Akizem", "d0tiKs",
        "Moonlight_Everlit", "Dark32", "andreycout",
        "Button_", "Cali666", "cassandra308",
        "lokinmodar", "jtabox", "AkiraYorumoto",
        "MKhayle", "elena.space", "imlisa",
        "andrei5125", "ShivaMaheshvara", "aislinn87",
        "nishinatsu051", "lichuyuan", "Risu64",
        "yummypillow", "witchymary", "Yuzumi",
        "zomsakura", "Sirayuki"
    ];

    internal About()
    {
        Translators.Sort((a, b) => string.Compare(a.ToLowerInvariant(), b.ToLowerInvariant(), StringComparison.Ordinal));
    }

    public void Draw(bool changed)
    {
        using var wrap = ImRaii.TextWrapPos(0.0f);

        ImGui.TextUnformatted(string.Format(Language.Options_About_Opening, Plugin.PluginName));

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextUnformatted(Language.Options_About_Authors);
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.Interface.Manifest.Author);

        ImGui.TextUnformatted(Language.Options_About_Discord);
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedGold, "@j.j_kazama");

        ImGui.TextUnformatted(Language.Options_About_Version);
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedOrange, Plugin.Interface.Manifest.AssemblyVersion.ToString(3));

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextUnformatted(Language.Options_About_Github_Issues);
        ImGui.SameLine();
        if (ImGuiUtil.IconButton(FontAwesomeIcon.ExternalLinkAlt, "githubIssues"))
            Dalamud.Utility.Util.OpenLink("https://github.com/JonKazama-Hellion/HellionChat/issues");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, HellionStrings.About_Maintainer_Heading);
        ImGui.TextUnformatted(HellionStrings.About_Maintainer_Body);
        ImGui.TextUnformatted(HellionStrings.About_Maintainer_Website_Label);
        ImGui.SameLine();
        if (ImGuiUtil.IconButton(FontAwesomeIcon.ExternalLinkAlt, "hellionMedia"))
            Dalamud.Utility.Util.OpenLink("https://hellion-media.de");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, HellionStrings.About_Mission_Heading);
        ImGui.TextUnformatted(HellionStrings.About_Mission_P1);
        ImGui.Spacing();
        ImGui.TextUnformatted(HellionStrings.About_Mission_P2);
        ImGui.Spacing();
        ImGui.TextUnformatted(HellionStrings.About_Mission_P3);

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, HellionStrings.About_BuiltOn_Heading);
        ImGui.TextUnformatted(HellionStrings.About_BuiltOn_P1);
        ImGui.Spacing();
        ImGui.TextUnformatted(HellionStrings.About_BuiltOn_P2);
        ImGui.Spacing();
        ImGui.TextUnformatted(HellionStrings.About_BuiltOn_Upstream_Label);
        ImGui.SameLine();
        if (ImGuiUtil.IconButton(FontAwesomeIcon.ExternalLinkAlt, "chatTwoUpstream"))
            Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/ChatTwo");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, HellionStrings.About_License_Heading);
        ImGui.TextUnformatted(HellionStrings.About_License_P1);
        ImGui.TextUnformatted(HellionStrings.About_License_P2);
        ImGui.TextUnformatted(HellionStrings.About_License_P3);

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.DalamudOrange, HellionStrings.About_SE_Heading);
        ImGui.TextUnformatted(HellionStrings.About_SE_P1);
        ImGui.TextUnformatted(HellionStrings.About_SE_P2);

        ImGui.Spacing();

        ImGui.TextColored(ImGuiColors.ParsedGold, HellionStrings.About_Localization_Heading);
        ImGui.TextUnformatted(HellionStrings.About_Localization_P1);
        ImGui.TextUnformatted(HellionStrings.About_Localization_P2);

        ImGui.Spacing();

        var height = ImGui.GetContentRegionAvail().Y - ImGui.CalcTextSize("A").Y - ImGui.GetStyle().ItemSpacing.Y * 2;
        using (var aboutChild = ImRaii.Child("about", new Vector2(-1, height)))
        {
            if (aboutChild)
            {
                using var treeNode = ImRaii.TreeNode(HellionStrings.About_Translators_TreeNode);
                if (treeNode)
                {
                    using var translatorChild = ImRaii.Child("translators");
                    if (translatorChild)
                    {
                        foreach (var translator in Translators)
                            ImGui.TextUnformatted(translator);
                    }
                }
            }
        }
        ImGui.Spacing();
    }
}
