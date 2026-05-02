using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ChatTwo.Ipc;
using ChatTwo.Resources;
using ChatTwo.Ui;
using ChatTwo.Util;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiFileDialog;

namespace ChatTwo;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    public const string PluginName = "Hellion Chat";

    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static INotificationManager Notification { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator Evaluator { get; private set; } = null!;

    public static Configuration Config = null!;
    public static FileDialogManager FileDialogManager { get; private set; } = null!;

    public readonly WindowSystem WindowSystem = new(PluginName);
    public SettingsWindow SettingsWindow { get; }
    public ChatLogWindow ChatLogWindow { get; }
    public DbViewer DbViewer { get; }
    public InputPreview InputPreview { get; }
    public CommandHelpWindow CommandHelpWindow { get; }
    public SeStringDebugger SeStringDebugger { get; }
    public FirstRunWizard FirstRunWizard { get; }
    public DebuggerWindow DebuggerWindow { get; }

    internal Commands Commands { get; }
    internal GameFunctions.GameFunctions Functions { get; }
    internal MessageManager MessageManager { get; }
    internal AutoTellTabsService AutoTellTabsService { get; }
    internal IpcManager Ipc { get; }
    internal ExtraChat ExtraChat { get; }
    internal TypingIpc TypingIpc { get; }
    internal FontManager FontManager { get; }

    internal int DeferredSaveFrames = -1;

    // Serialises retention sweeps. The 24h auto-sweep on plugin load and
    // the manual button in the Privacy tab both run on background threads;
    // without this gate, hitting the manual button moments after a fresh
    // plugin start would launch two sweeps in parallel and the second one
    // would just re-do work the first one already finished. The lock guards
    // the flag — the flag check itself bails before we touch the database.
    internal readonly object RetentionSweepLock = new();
    internal bool RetentionSweepRunning;

    internal DateTime GameStarted { get; }

    // Tab management needs to happen outside the chatlog window class for access reasons
    internal int LastTab { get; set; }
    internal int? WantedTab { get; set; }
    internal Tab CurrentTab
    {
        get
        {
            var i = LastTab;
            return i > -1 && i < Config.Tabs.Count ? Config.Tabs[i] : new Tab();
        }
    }

    public Plugin()
    {
        try
        {
            GameStarted = Process.GetCurrentProcess().StartTime.ToUniversalTime();

            // Hellion Chat: take over config + database from upstream ChatTwo
            // before Dalamud loads our plugin config. Idempotent: only acts on
            // the first start where the legacy paths exist and ours don't.
            MigrateFromChatTwoLayout();

            Config = Interface.GetPluginConfig() as Configuration ?? new Configuration();

            // Hellion Chat — Auto-Tell-Tabs Defense-in-Depth. SaveConfig
            // already strips temp tabs before persistence, but a previous
            // crash or external write could have left them in the JSON.
            // Drop them on load to guarantee the session-only invariant.
            Config.Tabs.RemoveAll(t => t.IsTempTab);

#pragma warning disable CS0618 // Type or member is obsolete
            // TODO Remove after 01.07.2026
            // Migrate old channel values
            if (Config.Version <= 5)
            {
                foreach (var tab in Config.Tabs)
                {
                    if (tab.ChatCodes.Count > 0)
                    {
                        tab.SelectedChannels = tab.ChatCodes.ToDictionary(pair => pair.Key, pair => (pair.Value, pair.Value));
                        tab.ChatCodes.Clear();
                    }

                    if (Config.InactivityHideChannels.Count > 0)
                    {
                        Config.InactivityHideChannelsV2 = Config.InactivityHideChannels.ToDictionary(pair => pair.Key, pair => (pair.Value, pair.Value));
                        Config.InactivityHideChannels.Clear();
                    }

                    Config.Version = 6;
                    SaveConfig();
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // Hellion Chat v6→v7: seed Privacy-First defaults.
            if (Config.Version <= 6)
            {
                Config.PrivacyFilterEnabled = true;
                Config.PrivacyPersistChannels = [..Privacy.PrivacyDefaults.PrivacyFirstWhitelist];
                Config.PrivacyPersistUnknownChannels = false;
                // Existing ChatTwo users skip the first-run wizard — the
                // migration toast already explains what changed and they
                // can reopen the wizard from Settings → Privacy if they
                // want to pick a different profile.
                Config.FirstRunCompleted = true;
                Config.Version = 7;
                SaveConfig();

                Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                {
                    Title = HellionStrings.Migration_Notification_Title,
                    Content = HellionStrings.Migration_Notification_Content,
                    Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(15),
                });
            }

            // Hellion Chat v7→v8: webinterface removed in 0.2.0. Old config
            // entries (WebinterfacePassword, AuthStore, etc.) get dropped on
            // the next save because their properties no longer exist on the
            // Configuration class. The bump is recorded so the notification
            // only fires once.
            if (Config.Version <= 7)
            {
                Config.Version = 8;
                SaveConfig();

                Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                {
                    Title = HellionStrings.Migration_Webinterface_Removed_Title,
                    Content = HellionStrings.Migration_Webinterface_Removed_Content,
                    Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(20),
                });
            }

            // Hellion Chat v8→v9: Auto-Tell-Tabs feature seeded with
            // property-initializer defaults (enabled, limit 15, history 20,
            // section header on). No data migration needed — just bump the
            // version and notify the user once so the feature does not
            // surprise them.
            if (Config.Version <= 8)
            {
                Config.Version = 9;
                SaveConfig();

                Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                {
                    Title = HellionStrings.AutoTellTabs_Migration_Title,
                    Content = HellionStrings.AutoTellTabs_Migration_Content,
                    Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(20),
                });
            }

            if (Config.Tabs.Count == 0)
                Config.Tabs.Add(TabsUtil.VanillaGeneral);

            LanguageChanged(Interface.UiLanguage);
            ImGuiUtil.Initialize(this);

            FileDialogManager = new FileDialogManager();

            Commands = new Commands();
            Functions = new GameFunctions.GameFunctions(this);
            Ipc = new IpcManager();
            TypingIpc = new TypingIpc(this);
            ExtraChat = new ExtraChat();
            FontManager = new FontManager();

            MessageManager = new MessageManager(this); // Does it require UI?

            // Hellion Chat — Auto-Tell-Tabs service. Subscribes to the
            // MessageManager's MessageProcessed event for live tells and
            // to ClientState.Logout for the cleanup pass. Created after
            // MessageManager so the constructor can hand off the live
            // store and event source.
            AutoTellTabsService = new AutoTellTabsService(this, MessageManager, MessageManager.Store);
            AutoTellTabsService.Initialize();

            // Hellion Chat — daily retention sweep, off-thread so it never
            // blocks plugin load. Skips itself when disabled or already ran
            // within the past 24 hours.
            RunRetentionSweepIfDue();

            ChatLogWindow = new ChatLogWindow(this);
            SettingsWindow = new SettingsWindow(this);
            DbViewer = new DbViewer(this);
            InputPreview = new InputPreview(ChatLogWindow);
            CommandHelpWindow = new CommandHelpWindow(ChatLogWindow);
            SeStringDebugger = new SeStringDebugger(this);
            DebuggerWindow = new DebuggerWindow(this);
            FirstRunWizard = new FirstRunWizard(this);

            WindowSystem.AddWindow(ChatLogWindow);
            WindowSystem.AddWindow(SettingsWindow);
            WindowSystem.AddWindow(DbViewer);
            WindowSystem.AddWindow(InputPreview);
            WindowSystem.AddWindow(CommandHelpWindow);
            WindowSystem.AddWindow(SeStringDebugger);
            WindowSystem.AddWindow(DebuggerWindow);
            WindowSystem.AddWindow(FirstRunWizard);

            // Open the wizard on a fresh install. Existing ChatTwo users have
            // FirstRunCompleted set to true by the v6→v7 migration above.
            if (!Config.FirstRunCompleted)
                FirstRunWizard.IsOpen = true;

            FontManager.BuildFonts();

            Interface.UiBuilder.DisableCutsceneUiHide = true;
            Interface.UiBuilder.DisableGposeUiHide = true;

            // let all the other components register, then initialize commands
            Commands.Initialise();

            if (Interface.Reason is not PluginLoadReason.Boot)
                MessageManager.FilterAllTabsAsync();

            Framework.Update += FrameworkUpdate;
            Interface.UiBuilder.Draw += Draw;
            Interface.LanguageChanged += LanguageChanged;
            // Hellion Chat — surface a "main UI" entry point so Dalamud's
            // plugin list shows the Open-Plugin button. Settings is the
            // most useful landing place; OpenConfigUi is already wired to
            // the same toggle inside SettingsWindow.
            Interface.UiBuilder.OpenMainUi += OpenMainUi;

            if (Config.ShowEmotes)
                Task.Run(EmoteCache.LoadData);

            #if !DEBUG
            // Avoid 300ms hitch when sending first message by preloading the
            // auto-translate cache. Don't do this in debug because it makes
            // profiling difficult.
            AutoTranslate.PreloadCache();
            #endif
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Plugin load threw an error, turning off plugin");
            Dispose();

            // Re-throw the exception to fail the plugin load.
            throw;
        }
    }

    // Suppressing this warning because Dispose() is called in Plugin() if the
    // load fails, so some values may not be initialized.
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public void Dispose()
    {
        Interface.UiBuilder.OpenMainUi -= OpenMainUi;
        Interface.LanguageChanged -= LanguageChanged;
        Interface.UiBuilder.Draw -= Draw;
        Framework.Update -= FrameworkUpdate;
        GameFunctions.GameFunctions.SetChatInteractable(true);

        WindowSystem?.RemoveAllWindows();
        ChatLogWindow?.Dispose();
        DbViewer?.Dispose();
        InputPreview?.Dispose();
        SettingsWindow?.Dispose();
        DebuggerWindow?.Dispose();
        SeStringDebugger?.Dispose();

        TypingIpc?.Dispose();
        ExtraChat?.Dispose();
        Ipc?.Dispose();
        // Dispose the Auto-Tell-Tabs service before MessageManager so it
        // can cleanly unsubscribe from the MessageProcessed event before
        // its source goes away.
        AutoTellTabsService?.Dispose();
        MessageManager?.DisposeAsync().AsTask().Wait();
        Functions?.Dispose();
        Commands?.Dispose();

        EmoteCache.Dispose();
    }

    private static void MigrateFromChatTwoLayout()
    {
        var pluginConfigsDir = Interface.ConfigDirectory.Parent?.FullName;
        if (pluginConfigsDir is null)
            return;

        var legacyConfigFile = Path.Combine(pluginConfigsDir, "ChatTwo.json");
        var legacyConfigDir = Path.Combine(pluginConfigsDir, "ChatTwo");
        var ourConfigFile = Path.Combine(pluginConfigsDir, "HellionChat.json");
        var ourConfigDir = Interface.ConfigDirectory.FullName;

        // Track whether anything legitimately blocked us. The most common
        // cause is upstream Chat 2 still being loaded — its SQLite handle
        // keeps chat-sqlite.db locked and File.Move throws IOException.
        var lockedBlocker = false;

        try
        {
            if (!File.Exists(ourConfigFile) && File.Exists(legacyConfigFile))
            {
                File.Move(legacyConfigFile, ourConfigFile);
                Log.Information($"HellionChat: migrated config file {legacyConfigFile} → {ourConfigFile}");
            }
        }
        catch (IOException e)
        {
            Log.Warning(e, $"HellionChat: config file move blocked, leaving {legacyConfigFile} in place");
            lockedBlocker = true;
        }

        // The plugin's ConfigDirectory may already exist on first load
        // (Dalamud creates it), so check at the file level instead of
        // skipping when the directory is present. Move every legacy
        // entry whose target name is not occupied yet, then remove the
        // source dir if it ends up empty. Each move is wrapped on its
        // own so a single locked file (the SQLite db while ChatTwo still
        // runs) does not abandon the rest of the migration.
        if (!Directory.Exists(legacyConfigDir))
            return;

        try
        {
            Directory.CreateDirectory(ourConfigDir);

            foreach (var file in Directory.EnumerateFiles(legacyConfigDir))
            {
                var target = Path.Combine(ourConfigDir, Path.GetFileName(file));
                if (File.Exists(target))
                    continue;
                try
                {
                    File.Move(file, target);
                    Log.Information($"HellionChat: migrated file {file} → {target}");
                }
                catch (IOException e)
                {
                    Log.Warning(e, $"HellionChat: file move blocked for {file}, will retry on next load");
                    lockedBlocker = true;
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(legacyConfigDir))
            {
                var target = Path.Combine(ourConfigDir, Path.GetFileName(dir));
                if (Directory.Exists(target))
                    continue;
                try
                {
                    Directory.Move(dir, target);
                    Log.Information($"HellionChat: migrated subdir {dir} → {target}");
                }
                catch (IOException e)
                {
                    Log.Warning(e, $"HellionChat: subdir move blocked for {dir}, will retry on next load");
                    lockedBlocker = true;
                }
            }

            if (!Directory.EnumerateFileSystemEntries(legacyConfigDir).Any())
            {
                Directory.Delete(legacyConfigDir);
                Log.Information($"HellionChat: removed empty legacy dir {legacyConfigDir}");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "HellionChat: layout migration failed, continuing with whatever exists");
        }

        if (lockedBlocker)
        {
            // Surface the most common cause to the user as a notification
            // so they don't think Hellion Chat lost their history when in
            // fact upstream Chat 2 was still holding the database file.
            Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
            {
                Title = "Hellion Chat",
                Content = "Could not migrate the Chat 2 database — the file appears to be in use. " +
                          "Disable Chat 2, fully close the game, then start it again. " +
                          "See the README troubleshooting section if the issue persists.",
                Type = Dalamud.Interface.ImGuiNotification.NotificationType.Warning,
                InitialDuration = TimeSpan.FromSeconds(30),
            });
        }
    }

    private void OpenMainUi()
    {
        // Settings is the most useful landing surface — same target as the
        // Configure button. SettingsWindow.Toggle is internal and already
        // wired to OpenConfigUi, so re-using IsOpen keeps both entry points
        // behaviourally identical.
        SettingsWindow.IsOpen = !SettingsWindow.IsOpen;
    }

    private void RunRetentionSweepIfDue()
    {
        if (!Config.RetentionEnabled)
            return;
        if (DateTimeOffset.UtcNow - Config.RetentionLastRunAt < TimeSpan.FromHours(24))
            return;

        // Snapshot the policy so the user can edit settings while we run.
        // Spec defaults form the baseline; explicit user overrides win.
        var policy = new Dictionary<int, int>();
        foreach (var (type, days) in Privacy.PrivacyDefaults.DefaultRetentionDays)
            policy[(int)(ushort)type] = days;
        foreach (var (type, days) in Config.RetentionPerChannelDays)
            policy[(int)(ushort)type] = days;
        var defaultDays = Config.RetentionDefaultDays;

        new Thread(() =>
        {
            // Bail out cheaply if a manual sweep is already in flight; the
            // lock around the actual work would queue us up otherwise and
            // we would just re-do whatever the manual run already did.
            lock (RetentionSweepLock)
            {
                if (RetentionSweepRunning)
                    return;
                RetentionSweepRunning = true;
            }

            try
            {
                var deleted = MessageManager.Store.DeleteByRetentionPolicy(policy, defaultDays);
                Config.RetentionLastRunAt = DateTimeOffset.UtcNow;
                SaveConfig();

                if (deleted > 0)
                {
                    Log.Information($"Retention sweep deleted {deleted} expired messages.");
                    Framework.Run(() =>
                    {
                        MessageManager.ClearAllTabs();
                        MessageManager.FilterAllTabsAsync();
                    }).Wait();
                }
                else
                {
                    Log.Information("Retention sweep ran, nothing expired.");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Retention sweep failed");
            }
            finally
            {
                lock (RetentionSweepLock)
                    RetentionSweepRunning = false;
            }
        }) { IsBackground = true }.Start();
    }

    private void Draw()
    {
        // Hellion theme is pushed once per frame here so every plugin window
        // (chat log, settings, viewers, wizard, file dialog) renders with
        // the same palette. Skipping the push leaves the upstream Dalamud
        // look untouched for users who flipped the toggle off.
        using IDisposable? _style = Config.HellionThemeEnabled
            ? HellionStyle.PushGlobal(Config.HellionThemeWindowOpacity)
            : null;

        ChatLogWindow.BeginFrame();

        if (Config.HideInLoadingScreens && Condition[ConditionFlag.BetweenAreas])
        {
            ChatLogWindow.FinalizeFrame();
            TypingIpc.Update();
            return;
        }

        ChatLogWindow.HideStateCheck();

        Interface.UiBuilder.DisableUserUiHide = !Config.HideWhenUiHidden;
        ChatLogWindow.DefaultText = ImGui.GetStyle().Colors[(int) ImGuiCol.Text];

        using ((Config.FontsEnabled ? FontManager.RegularFont : FontManager.Axis).Push())
            WindowSystem.Draw();

        ChatLogWindow.FinalizeFrame();
        TypingIpc.Update();

        FileDialogManager.Draw();
    }

    internal void SaveConfig()
    {
        // Hellion Chat — Auto-Tell-Tabs are session-only. Strip them out
        // before serialization so a crash mid-session can never persist
        // them. We snapshot the full tab list first and restore it after
        // the save, preserving the user's order and open conversations.
        var snapshot = Config.Tabs.ToList();
        Config.Tabs.RemoveAll(t => t.IsTempTab);

        Interface.SavePluginConfig(Config);

        Config.Tabs.Clear();
        Config.Tabs.AddRange(snapshot);
    }

    internal void LanguageChanged(string langCode)
    {
        var info = Config.LanguageOverride is LanguageOverride.None
            ? new CultureInfo(langCode)
            : new CultureInfo(Config.LanguageOverride.Code());

        Language.Culture = info;
        HellionStrings.Culture = info;
    }

    private static readonly string[] ChatAddonNames =
    [
        "ChatLog",
        "ChatLogPanel_0",
        "ChatLogPanel_1",
        "ChatLogPanel_2",
        "ChatLogPanel_3"
    ];

    private void FrameworkUpdate(IFramework framework)
    {
        if (DeferredSaveFrames >= 0 && DeferredSaveFrames-- == 0)
            SaveConfig();

        if (!Config.HideChat)
            return;

        foreach (var name in ChatAddonNames)
            if (GameFunctions.GameFunctions.IsAddonInteractable(name))
                GameFunctions.GameFunctions.SetAddonInteractable(name, false);
    }

    public static bool InBattle => Condition[ConditionFlag.InCombat];
    public static bool GposeActive => Condition[ConditionFlag.WatchingCutscene];
    public static bool CutsceneActive => Condition[ConditionFlag.OccupiedInCutSceneEvent] || Condition[ConditionFlag.WatchingCutscene78];
}
