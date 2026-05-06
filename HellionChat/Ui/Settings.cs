using System.Numerics;
using HellionChat.Resources;
using HellionChat.Ui.SettingsTabs;
using HellionChat.Util;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui;

internal enum SettingsView
{
    Overview,
    Detail,
}

public sealed class SettingsWindow : Dalamud.Interface.Windowing.Window
{
    internal readonly Plugin Plugin;

    private Configuration Mutable { get; }
    private List<ISettingsTab> Tabs { get; }
    private int CurrentTab;
    private SettingsView View = SettingsView.Overview;
    private readonly SettingsOverview Overview;

    internal SettingsWindow(Plugin plugin) : base($"{Language.Settings_Title.Format(Plugin.PluginName)}###chat2-settings")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(475, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        Mutable = new Configuration();

        Overview = new SettingsOverview(this);

        Tabs =
        [
            new General(Plugin, Mutable),
            new ThemeAndLayout(Plugin, Mutable),
            new FontsAndColours(Plugin, Mutable),
            new SettingsTabs.Window(Plugin, Mutable),
            new Chat(Plugin, Mutable),
            new SettingsTabs.Tabs(Plugin, Mutable),
            new SettingsTabs.Privacy(Plugin, Mutable),
            new DataManagement(Plugin, Mutable),
            new Information(Mutable),
        ];

        RespectCloseHotkey = false;
        DisableWindowSounds = true;

        Initialise();

        Plugin.Commands.Register("/hellion", "Perform various actions with Hellion Chat.").Execute += Command;
        Plugin.Interface.UiBuilder.OpenConfigUi += Toggle;
    }

    public void Dispose()
    {
        Plugin.Interface.UiBuilder.OpenConfigUi -= Toggle;
        Plugin.Commands.Register("/hellion").Execute -= Command;
    }

    private void Command(string command, string args)
    {
        if (string.IsNullOrWhiteSpace(args))
            Toggle();
    }

    private void Initialise()
    {
        Mutable.UpdateFrom(Plugin.Config, false);
    }

    public override void Draw()
    {
        if (ImGui.IsWindowAppearing())
        {
            Initialise();
            View = SettingsView.Overview;
        }

        // ESC im Detail-View kehrt zur Overview zurück. Window-Focus-Check ist
        // Pflicht — sonst triggert ESC auch wenn der User ein anderes Fenster
        // fokussiert hat und ESC fürs Game-Menü drückt (Codebase-Pattern siehe
        // Util/SearchSelector.cs:37).
        if (View == SettingsView.Detail
            && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows)
            && ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            View = SettingsView.Overview;
            return;
        }

        if (View == SettingsView.Overview)
            Overview.Draw();
        else
            DrawDetail();

        ImGui.Separator();
        DrawSaveButtons();
    }

    internal void OpenSection(int tabIndex)
    {
        CurrentTab = tabIndex;
        View = SettingsView.Detail;
    }

    internal void OpenOverview()
    {
        View = SettingsView.Overview;
    }

    private void DrawDetail()
    {
        // Breadcrumb-Header — Akzent-Cyan, klickbar, führt zurück zur Overview
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00BED2u))
        using (ImRaii.PushColor(ImGuiCol.Button, 0u))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, 0x33FFFFFFu))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, 0x55FFFFFFu))
        {
            if (ImGui.SmallButton("← Settings"))
            {
                View = SettingsView.Overview;
                return;
            }
        }
        ImGui.SameLine();
        ImGui.TextUnformatted("·");
        ImGui.SameLine();
        ImGui.TextUnformatted(Tabs[CurrentTab].Name.Split("###")[0]);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Section-Content in voller Breite. Die Tab-Liste links ist überholt:
        // der User ist bereits über die Card-Übersicht navigiert, eine zweite
        // Tab-Liste daneben würde nur den Vanilla-Look zurückbringen. Falls
        // der User in eine andere Section will, geht er zurück zur Overview
        // (Breadcrumb / ESC).
        var style = ImGui.GetStyle();
        var height = ImGui.GetContentRegionAvail().Y - style.FramePadding.Y * 2 - style.ItemSpacing.Y - style.ItemInnerSpacing.Y * 2 - ImGui.CalcTextSize("A").Y;

        using var child = ImRaii.Child("##chat2-settings-detail", new Vector2(-1, height));
        if (child.Success)
            Tabs[CurrentTab].Draw(false);
    }

    private void DrawSaveButtons()
    {
        var save = ImGui.Button(Language.Settings_Save);

        ImGui.SameLine();

        if (ImGui.Button(Language.Settings_SaveAndClose))
        {
            save = true;
            IsOpen = false;
        }

        ImGui.SameLine();

        if (ImGui.Button(Language.Settings_Discard))
        {
            IsOpen = false;
        }

        const string buttonLabel = "Anna's Ko-fi";
        const string buttonLabel2 = "Infi's Ko-fi";

        using (ImRaii.PushColor(ImGuiCol.Button, ColourUtil.RgbaToAbgr(0xFF5E5BFF)))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, ColourUtil.RgbaToAbgr(0xFF7775FF)))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, ColourUtil.RgbaToAbgr(0xFF4542FF)))
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFFFFFFFF))
        {
            var buttonWidth = ImGui.CalcTextSize(buttonLabel).X + ImGui.GetStyle().FramePadding.X * 2;
            var buttonWidth2 = ImGui.CalcTextSize(buttonLabel2).X + ImGui.GetStyle().FramePadding.X * 2;
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - buttonWidth - buttonWidth2 - ImGui.GetStyle().ItemSpacing.X);

            if (ImGui.Button(buttonLabel2))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");

            ImGui.SameLine();

            if (ImGui.Button(buttonLabel))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/lojewalo");
        }

        if (!save)
            return;

        // calculate all conditions before updating config
        var hideChanged = !Mutable.HideChat && Mutable.HideChat != Plugin.Config.HideChat;
        var languageChanged = Mutable.LanguageOverride != Plugin.Config.LanguageOverride;
        var fontChanged = Mutable.GlobalFontV2 != Plugin.Config.GlobalFontV2
                          || Mutable.JapaneseFontV2 != Plugin.Config.JapaneseFontV2
                          || Mutable.ItalicFontV2 != Plugin.Config.ItalicFontV2
                          || Mutable.ExtraGlyphRanges != Plugin.Config.ExtraGlyphRanges
                          || Mutable.UseHellionFont != Plugin.Config.UseHellionFont;
        var fontSizeChanged = Math.Abs(Mutable.SymbolsFontSizeV2 - Plugin.Config.SymbolsFontSizeV2) > 0.001
                          || Math.Abs(Mutable.FontSizeV2 - Plugin.Config.FontSizeV2) > 0.001;
        var italicStateChanged = Mutable.ItalicEnabled != Plugin.Config.ItalicEnabled;
        // v1.2.0 — Refilter only if a filter-relevant setting actually
        // changed. The Clear+Refilter cycle reloads messages from the DB,
        // which silently wipes any in-session message that wasn't
        // persisted (Privacy-First config blocks most channels from DB).
        // Cosmetic changes (theme, tab icons, layout flags) trigger no
        // refilter — chat history stays intact.
        var filtersChanged = HasFilterRelevantChanges();

        Plugin.Config.UpdateFrom(Mutable, true);

        // save after 60 frames have passed, which should hopefully not
        // commit any changes that cause a crash
        Plugin.DeferredSaveFrames = 60;
        if (filtersChanged)
        {
            Plugin.MessageManager.ClearAllTabs();
            Plugin.MessageManager.FilterAllTabsAsync();
        }

        if (fontChanged || fontSizeChanged || italicStateChanged)
            Plugin.FontManager.BuildFonts();

        if (languageChanged)
            Plugin.LanguageChanged(Plugin.Interface.UiLanguage);

        if (hideChanged)
            GameFunctions.GameFunctions.SetChatInteractable(true);

        if (Plugin.Config.ShowEmotes)
            _ = EmoteCache.LoadData(); // Fire-and-forget intentional, exceptions are caught inside

        Initialise();
    }

    /// <summary>
    /// v1.2.0 — Detects whether any setting that influences message
    /// filtering changed between Plugin.Config and the Mutable working
    /// copy. Used to gate the heavy ClearAllTabs+FilterAllTabsAsync cycle
    /// in Save: cosmetic changes (theme, tab icons, layout flags) do not
    /// touch the chat log, only filter-relevant changes do. Without this
    /// gate, every settings save wipes the chat history of any channel
    /// the Privacy filter blocks from being persisted to the DB —
    /// reported by Flo from in-game testing 2026-05-05/06.
    /// </summary>
    private bool HasFilterRelevantChanges()
    {
        // Top-level privacy controls.
        if (Mutable.PrivacyFilterEnabled != Plugin.Config.PrivacyFilterEnabled) return true;
        if (Mutable.PrivacyPersistUnknownChannels != Plugin.Config.PrivacyPersistUnknownChannels) return true;
        if (!Mutable.PrivacyPersistChannels.SetEquals(Plugin.Config.PrivacyPersistChannels)) return true;

        // FilterIncludePreviousSessions changes the GetMostRecentMessages
        // window in MessageManager.FilterAllTabs and is therefore filter-
        // relevant even though it lives outside the Privacy block.
        if (Mutable.FilterIncludePreviousSessions != Plugin.Config.FilterIncludePreviousSessions) return true;

        // Per-tab channel selection. Compare persistent tabs only —
        // TempTabs are session-only and never refiltered anyway.
        var origPersistent = Plugin.Config.Tabs.Where(t => !t.IsTempTab).ToList();
        var newPersistent = Mutable.Tabs.Where(t => !t.IsTempTab).ToList();

        if (origPersistent.Count != newPersistent.Count) return true; // add or delete

        for (var i = 0; i < origPersistent.Count; i++)
        {
            var orig = origPersistent[i];
            var neu = newPersistent[i];

            // Identifier mismatch at the same index means reorder or
            // a slot got swapped — treat as filter-relevant so the new
            // channel-selection layout actually applies.
            if (orig.Identifier != neu.Identifier) return true;

            if (orig.ExtraChatAll != neu.ExtraChatAll) return true;
            if (!orig.ExtraChatChannels.SetEquals(neu.ExtraChatChannels)) return true;

            // SelectedChannels is a Dictionary<ChatType, (ChatSource, ChatSource)>
            // — value-tuple equality already does the right thing per-pair.
            if (orig.SelectedChannels.Count != neu.SelectedChannels.Count) return true;
            foreach (var pair in orig.SelectedChannels)
            {
                if (!neu.SelectedChannels.TryGetValue(pair.Key, out var nv)) return true;
                if (!pair.Value.Equals(nv)) return true;
            }
        }

        return false;
    }
}
