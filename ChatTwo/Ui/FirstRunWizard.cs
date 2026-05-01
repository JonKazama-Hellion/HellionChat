using System.Numerics;
using ChatTwo.Code;
using ChatTwo.Privacy;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui;

public sealed class FirstRunWizard : Window
{
    private readonly Plugin Plugin;

    internal FirstRunWizard(Plugin plugin) : base($"{HellionStrings.Wizard_Title}###hellion-firstrun")
    {
        Plugin = plugin;

        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking;
        SizeCondition = ImGuiCond.Appearing;
        Size = new Vector2(900, 560);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(720, 480),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void OnClose()
    {
        // Closing the wizard without picking anything = the user accepts
        // whatever defaults are already in place. Mark as complete so we
        // don't pester them again on the next launch.
        if (!Plugin.Config.FirstRunCompleted)
        {
            Plugin.Config.FirstRunCompleted = true;
            Plugin.SaveConfig();
        }
    }

    public override void Draw()
    {
        ImGui.TextWrapped(HellionStrings.Wizard_Intro);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var avail = ImGui.GetContentRegionAvail();
        var cardWidth = (avail.X - ImGui.GetStyle().ItemSpacing.X * 2) / 3f;
        var cardHeight = avail.Y - ImGui.GetTextLineHeightWithSpacing();

        DrawCard("privacy-first", cardWidth, cardHeight,
            HellionStrings.Wizard_Profile_PrivacyFirst_Heading,
            HellionStrings.Wizard_Profile_PrivacyFirst_Description,
            null,
            HellionStrings.Wizard_Profile_PrivacyFirst_Apply,
            ApplyPrivacyFirst);

        ImGui.SameLine();

        DrawCard("casual", cardWidth, cardHeight,
            HellionStrings.Wizard_Profile_Casual_Heading,
            HellionStrings.Wizard_Profile_Casual_Description,
            null,
            HellionStrings.Wizard_Profile_Casual_Apply,
            ApplyCasual);

        ImGui.SameLine();

        DrawCard("full-history", cardWidth, cardHeight,
            HellionStrings.Wizard_Profile_FullHistory_Heading,
            HellionStrings.Wizard_Profile_FullHistory_Description,
            HellionStrings.Wizard_Profile_FullHistory_GdprWarning,
            HellionStrings.Wizard_Profile_FullHistory_Apply,
            ApplyFullHistory);
    }

    private void DrawCard(string id, float width, float height, string heading, string description, string? warning, string buttonLabel, Action onApply)
    {
        using var child = ImRaii.Child($"##wizard-card-{id}", new Vector2(width, height), true);
        if (!child.Success)
            return;

        ImGui.TextUnformatted(heading);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(description);

        if (warning is not null)
        {
            ImGui.Spacing();
            ImGuiUtil.WarningText(warning);
        }

        // Push the button to the bottom of the card.
        var lineHeight = ImGui.GetFrameHeightWithSpacing();
        var remaining = ImGui.GetContentRegionAvail().Y - lineHeight;
        if (remaining > 0)
            ImGui.Dummy(new Vector2(0, remaining));

        if (ImGui.Button($"{buttonLabel}##{id}", new Vector2(-1, 0)))
        {
            onApply();
            Plugin.Config.FirstRunCompleted = true;
            Plugin.SaveConfig();
            IsOpen = false;
        }
    }

    private void ApplyPrivacyFirst()
    {
        Plugin.Config.PrivacyFilterEnabled = true;
        Plugin.Config.PrivacyPersistChannels = [..PrivacyDefaults.PrivacyFirstWhitelist];
        Plugin.Config.PrivacyPersistUnknownChannels = false;

        Plugin.Config.RetentionEnabled = true;
        Plugin.Config.RetentionDefaultDays = 30;
        Plugin.Config.RetentionPerChannelDays =
            PrivacyDefaults.DefaultRetentionDays.ToDictionary(p => p.Key, p => p.Value);
    }

    private void ApplyCasual()
    {
        Plugin.Config.PrivacyFilterEnabled = true;
        Plugin.Config.PrivacyPersistChannels = [..PrivacyDefaults.CasualWhitelist];
        Plugin.Config.PrivacyPersistUnknownChannels = false;

        Plugin.Config.RetentionEnabled = true;
        Plugin.Config.RetentionDefaultDays = 30;
        var policy = PrivacyDefaults.DefaultRetentionDays.ToDictionary(p => p.Key, p => p.Value);
        foreach (var (type, days) in PrivacyDefaults.CasualRetentionOverrides)
            policy[type] = days;
        Plugin.Config.RetentionPerChannelDays = policy;
    }

    private void ApplyFullHistory()
    {
        // Full history = upstream Chat 2 behavior. Filter off, retention off,
        // everything (except battle messages, which Chat 2 itself controls)
        // accumulates indefinitely.
        Plugin.Config.PrivacyFilterEnabled = false;
        Plugin.Config.PrivacyPersistUnknownChannels = true;

        Plugin.Config.RetentionEnabled = false;
        Plugin.Config.RetentionPerChannelDays.Clear();
    }
}
