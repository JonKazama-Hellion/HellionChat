using System.Numerics;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui;

internal class Popout : Window
{
    private readonly ChatLogWindow ChatLogWindow;
    private readonly Tab Tab;
    private readonly int Idx;

    private long FrameTime; // set every frame
    private long LastActivityTime = Environment.TickCount64;

    // v0.6.0 — optional input bar inside the pop-out window. Lazy-allocated
    // when the user enables Tab.PopOutInputEnabled and torn down when the
    // toggle is turned off (independent text buffer is intentionally
    // discarded — see v0.6.0 spec edge-case P1).
    public ChatInputBar? InputBar { get; private set; }
    public bool HasFocusedInputBar => InputBar?.IsFocused ?? false;

    public Popout(ChatLogWindow chatLogWindow, Tab tab, int idx) : base($"{tab.Name}##popout")
    {
        ChatLogWindow = chatLogWindow;
        Tab = tab;
        Idx = idx;

        Size = new Vector2(350, 350);
        SizeCondition = ImGuiCond.FirstUseEver;

        IsOpen = true;
        RespectCloseHotkey = false;
        DisableWindowSounds = true;
    }

    public override void PreOpenCheck()
    {
        if (!Tab.PopOut)
            IsOpen = false;
    }

    public override bool DrawConditions()
    {
        FrameTime = Environment.TickCount64;
        if (Tab.IndependentHide ? HideStateCheck() : ChatLogWindow.IsHidden)
            return false;

        if (!Plugin.Config.HideWhenInactive || (!Plugin.Config.InactivityHideActiveDuringBattle && Plugin.InBattle) || !Tab.UnhideOnActivity)
        {
            LastActivityTime = FrameTime;
            return true;
        }

        // Activity in the tab, this popout window, or the main chat log window.
        var lastActivityTime = Math.Max(Tab.LastActivity, LastActivityTime);
        lastActivityTime = Math.Max(lastActivityTime, ChatLogWindow.LastActivityTime);
        return FrameTime - lastActivityTime <= 1000 * Plugin.Config.InactivityHideTimeout;
    }

    public override void PreDraw()
    {
        if (Plugin.Config is { OverrideStyle: true, ChosenStyle: not null })
            StyleModel.GetConfiguredStyles()?.FirstOrDefault(style => style.Name == Plugin.Config.ChosenStyle)?.Push();

        Flags = ImGuiWindowFlags.None;
        if (!Plugin.Config.ShowPopOutTitleBar)
            Flags |= ImGuiWindowFlags.NoTitleBar;

        if (!Tab.CanMove)
            Flags |= ImGuiWindowFlags.NoMove;

        if (!Tab.CanResize)
            Flags |= ImGuiWindowFlags.NoResize;

        if (!ChatLogWindow.PopOutDocked[Idx])
        {
            if (Tab.IndependentOpacity)
            {
                BgAlpha = Tab.Opacity / 100f;
            }
            else
            {
                BgAlpha = Plugin.Config.HellionThemeEnabled
                    ? Plugin.Config.HellionThemeWindowOpacity
                    : Plugin.Config.WindowAlpha / 100f;
            }
        }
    }

    public override void Draw()
    {
        using var id = ImRaii.PushId($"popout-{Tab.Identifier}");

        if (!Plugin.Config.ShowPopOutTitleBar)
        {
            ImGui.TextUnformatted(Tab.Name);
            ImGui.Separator();
        }

        // v0.6.0 — one-time hint banner explaining the new pop-out input
        // feature. Shown once per user; "Got it" or "Open settings"
        // dismisses it and persists the flag.
        var hintBannerHeight = DrawHintBannerIfNeeded();

        // v0.6.0 — pop-out optional input bar. Reserve height first so the
        // message log draws into the right region; only shown when the
        // global master switch is on. Toggle-OFF resets InputBar so the
        // next toggle-ON gives a fresh buffer (no stale text persists).
        var inputEnabled = Plugin.Config.PopOutInputEnabled;
        if (!inputEnabled && InputBar != null)
        {
            InputBar = null;
        }
        if (inputEnabled)
        {
            InputBar ??= new ChatInputBar(ChatLogWindow.Plugin, ChatLogWindow, () => Tab);
        }

        var inputBarHeight = inputEnabled
            ? ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y
            : 0f;

        var handler = ChatLogWindow.HandlerLender.Borrow();
        var logHeight = ImGui.GetContentRegionAvail().Y - inputBarHeight - hintBannerHeight;
        ChatLogWindow.DrawMessageLog(Tab, handler, logHeight, false);

        if (inputEnabled && InputBar != null)
        {
            ImGui.Separator();
            InputBar.RenderCompact();
        }

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            LastActivityTime = FrameTime;
    }

    // Returns the vertical space the banner consumed (0 when not shown)
    // so the message log can shrink accordingly.
    private float DrawHintBannerIfNeeded()
    {
        if (Plugin.Config.SeenPopOutInputHint)
            return 0f;

        var hintText = Resources.HellionStrings.Popout_v060_HintText;
        var ackLabel = Resources.HellionStrings.Popout_v060_HintAck;
        var openLabel = Resources.HellionStrings.Popout_v060_HintOpenSettings;

        var startY = ImGui.GetCursorPosY();

        var bg = new System.Numerics.Vector4(0.16f, 0.20f, 0.28f, 1f);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, bg);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);

        var dismiss = false;
        var openSettings = false;
        using (var child = ImRaii.Child("##v060-pop-out-hint", new System.Numerics.Vector2(0f, 64f), true))
        {
            if (child)
            {
                ImGui.TextWrapped(hintText);
                if (ImGui.Button(ackLabel))
                    dismiss = true;
                ImGui.SameLine();
                if (ImGui.Button(openLabel))
                {
                    dismiss = true;
                    openSettings = true;
                }
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
        ImGui.Spacing();

        if (dismiss)
        {
            Plugin.Config.SeenPopOutInputHint = true;
            ChatLogWindow.Plugin.SaveConfig();
            Plugin.Log.Debug("Pop-Out input hint dismissed");
            if (openSettings)
                ChatLogWindow.Plugin.SettingsWindow.Toggle();
        }

        return ImGui.GetCursorPosY() - startY;
    }

    public override void PostDraw()
    {
        ChatLogWindow.PopOutDocked[Idx] = ImGui.IsWindowDocked();

        if (Plugin.Config is { OverrideStyle: true, ChosenStyle: not null })
            StyleModel.GetConfiguredStyles()?.FirstOrDefault(style => style.Name == Plugin.Config.ChosenStyle)?.Pop();
    }

    public override void OnClose()
    {
        ChatLogWindow.PopOutWindows.Remove(Tab.Identifier);
        ChatLogWindow.Plugin.WindowSystem.RemoveWindow(this);

        Tab.PopOut = false;
        ChatLogWindow.Plugin.SaveConfig();
    }

    private enum HideState
    {
        None,
        Cutscene,
        CutsceneOverride,
        User,
        Battle
    }

    private HideState CurrentHideState = HideState.None;

    private bool HideStateCheck()
    {
        // if the chat has no hide state set, and the player has entered battle, we hide chat if they have configured it
        if (Tab.HideInBattle && CurrentHideState == HideState.None && Plugin.InBattle)
            CurrentHideState = HideState.Battle;

        // If the chat is hidden because of battle, we reset it here
        if (CurrentHideState is HideState.Battle && !Plugin.InBattle)
            CurrentHideState = HideState.None;

        // if the chat has no hide state and in a cutscene, set the hide state to cutscene
        if (Tab.HideDuringCutscenes && CurrentHideState == HideState.None && (Plugin.CutsceneActive || Plugin.GposeActive))
        {
            if (ChatLogWindow.Plugin.Functions.Chat.CheckHideFlags())
                CurrentHideState = HideState.Cutscene;
        }

        // if the chat is hidden because of a cutscene and no longer in a cutscene, set the hide state to none
        if (CurrentHideState is HideState.Cutscene or HideState.CutsceneOverride && !Plugin.CutsceneActive && !Plugin.GposeActive)
            CurrentHideState = HideState.None;

        // if the chat is hidden because of a cutscene and the chat has been activated, show chat
        if (CurrentHideState == HideState.Cutscene && ChatLogWindow.Activate)
            CurrentHideState = HideState.CutsceneOverride;

        // if the user hid the chat and is now activating chat, reset the hide state
        if (CurrentHideState == HideState.User && ChatLogWindow.Activate)
            CurrentHideState = HideState.None;

        return CurrentHideState is HideState.Cutscene or HideState.User or HideState.Battle || (Tab.HideWhenNotLoggedIn && !Plugin.ClientState.IsLoggedIn);
    }
}
