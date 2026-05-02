using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

// Information-Tab vereint die früheren About- und Changelog-Tabs in
// drei kollabierbaren Sektionen. Der About-Inhalt ist 1:1 aus About.cs
// übernommen, die Changelog-Render-Logik aus Changelog.cs.
internal sealed class Information : ISettingsTab
{
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Information + "###tabs-information";

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

    internal Information(Configuration mutable)
    {
        Mutable = mutable;
        Translators.Sort((a, b) => string.Compare(a.ToLowerInvariant(), b.ToLowerInvariant(), StringComparison.Ordinal));
    }

    public void Draw(bool changed)
    {
        using var wrap = ImRaii.TextWrapPos(0.0f);

        DrawVersionInfoSection();
        ImGui.Spacing();
        DrawAboutSection();
        ImGui.Spacing();
        DrawChangelogSection();
    }

    private void DrawVersionInfoSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Information_VersionInfo_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
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
        }
    }

    private void DrawAboutSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Information_About_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
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

            using (var translatorTree = ImRaii.TreeNode(HellionStrings.About_Translators_TreeNode))
            {
                if (translatorTree)
                {
                    using var indent = ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false);
                    foreach (var translator in Translators)
                        ImGui.TextUnformatted(translator);
                }
            }
        }
    }

    private void DrawChangelogSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Information_Changelog_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_PrintChangelog_Name, ref Mutable.PrintChangelog);
            ImGuiUtil.HelpMarker(Language.Options_PrintChangelog_Description);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            var changelog = Plugin.Interface.Manifest.Changelog;
            if (changelog == null)
                return;

            ImGui.TextUnformatted(Language.Options_Changelog_Header);
            ImGui.TextUnformatted($"Version {Plugin.Interface.Manifest.AssemblyVersion.ToString(3)}");
            ImGui.Spacing();
            foreach (var sentence in changelog.Split("\n"))
            {
                if (sentence == string.Empty)
                {
                    ImGui.NewLine();
                    continue;
                }

                var indented = sentence.StartsWith('-') || sentence.StartsWith("  -");
                using var indent = ImRaii.PushIndent(10.0f, true, indented);
                ImGui.TextUnformatted(sentence);
            }
        }
    }
}
