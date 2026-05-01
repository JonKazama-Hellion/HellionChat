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

        // Hellion-specific maintainer / attribution / license / SE-
        // disclaimer block. Hand-rolled in English here rather than via
        // HellionStrings — the legal-ish copy stays close to the EUPL-1.2
        // wording and the SE disclaimer is the same in every locale.
        ImGui.TextColored(ImGuiColors.ParsedGold, "Maintainer");
        ImGui.TextUnformatted("Hellion Chat is maintained by Hellion Online Media (Florian Wathling).");
        ImGui.TextUnformatted("Website:");
        ImGui.SameLine();
        if (ImGuiUtil.IconButton(FontAwesomeIcon.ExternalLinkAlt, "hellionMedia"))
            Dalamud.Utility.Util.OpenLink("https://hellion-media.de");
        ImGui.TextUnformatted("For licensing, legal or contact inquiries please reach out via the website above.");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, "Built on Chat 2");
        ImGui.TextUnformatted("Hellion Chat is a fork of Chat 2 by Infi and Anna (ascclemens).");
        ImGui.TextUnformatted("Every chat replacement feature, the IPC integration, the rendering engine and the storage core come from upstream Chat 2.");
        ImGui.TextUnformatted("Upstream repository:");
        ImGui.SameLine();
        if (ImGuiUtil.IconButton(FontAwesomeIcon.ExternalLinkAlt, "chatTwoUpstream"))
            Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/ChatTwo");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.ParsedGold, "License");
        ImGui.TextUnformatted("Hellion Chat and Chat 2 are licensed under the European Union Public License v1.2 (EUPL-1.2).");
        ImGui.TextUnformatted("© 2023–2026 the Chat 2 authors (Infi, Anna and the upstream contributors).");
        ImGui.TextUnformatted("© 2026 Hellion Online Media — for the Hellion Chat additions.");

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextColored(ImGuiColors.DalamudOrange, "FINAL FANTASY XIV disclaimer");
        ImGui.TextUnformatted("FINAL FANTASY XIV © SQUARE ENIX CO., LTD. All rights reserved.");
        ImGui.TextUnformatted("Hellion Chat is an unofficial, fan-made plugin and is not affiliated with, endorsed, sponsored or approved by Square Enix.");

        ImGui.Spacing();

        var height = ImGui.GetContentRegionAvail().Y - ImGui.CalcTextSize("A").Y - ImGui.GetStyle().ItemSpacing.Y * 2;
        using (var aboutChild = ImRaii.Child("about", new Vector2(-1, height)))
        {
            if (aboutChild)
            {
                using var treeNode = ImRaii.TreeNode(Language.Options_About_Translators);
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
