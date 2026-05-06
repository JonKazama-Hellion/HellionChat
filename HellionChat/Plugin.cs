using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using HellionChat.Ipc;
using HellionChat.Resources;
using HellionChat.Ui;
using HellionChat.Util;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiFileDialog;

namespace HellionChat;

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
    internal Themes.ThemeRegistry ThemeRegistry { get; private set; } = null!;
    internal Ui.StatusBar StatusBar { get; private set; } = null!;
    internal Integrations.HonorificService HonorificService { get; private set; } = null!;

    internal int DeferredSaveFrames = -1;

    // Serialises retention sweeps. The 24h auto-sweep on plugin load and
    // the manual button in the Privacy tab both run on background threads;
    // without this gate, hitting the manual button moments after a fresh
    // plugin start would launch two sweeps in parallel and the second one
    // would just re-do work the first one already finished. The lock guards
    // the flag — the flag check itself bails before we touch the database.
    // Volatile because the ImGui thread reads the flag outside the lock to
    // gate the manual button; without it the JIT may cache the value in a
    // register and miss the background-thread update.
    internal readonly object RetentionSweepLock = new();
    internal volatile bool RetentionSweepRunning;

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
        // Refuse to start if upstream Chat 2 is loaded — prevents IPC
        // channel collisions and double-replacement of the in-game chat
        // window. Throwing here makes Dalamud abort the load cleanly with
        // our localized message instead of crashing FFXIV mid-frame.
        ChatTwoConflictDetector.ThrowIfChatTwoIsLoaded(Interface);

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

            // Hellion Chat v9 → v10 — wipes the configuration so the new 8-tab
            // layout starts from defaults instead of mapping every previous setting
            // to its new position. Backup-Failure ist non-fatal, der Wipe läuft
            // trotzdem; dem User fehlt dann nur das manuelle Restore-Sicherheitsnetz.
            if (Config.Version < 10)
            {
                var pluginConfigsDir = Interface.ConfigDirectory.Parent?.FullName;
                if (pluginConfigsDir is not null)
                {
                    var liveConfigPath = Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json");
                    var backupPath = Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json.pre-v10-backup");

                    try
                    {
                        if (File.Exists(liveConfigPath))
                        {
                            File.Copy(liveConfigPath, backupPath, overwrite: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "HellionChat: pre-v10 config backup failed");
                    }
                }

                Config = new Configuration
                {
                    Version = 10,
                    FirstRunCompleted = true,
                };
                SaveConfig();

                Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                {
                    Title = HellionStrings.SettingsRefactor_Migration_Title,
                    Content = HellionStrings.SettingsRefactor_Migration_Content,
                    Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(25),
                });
            }

            // Hellion Chat v10 → v11 — adds the global Configuration.PopOutInputEnabled
            // master switch and SeenPopOutInputHint flag for the v0.6.0 pop-out
            // input feature. Lightweight migration: defaults both fields,
            // no user-facing notification because the change is opt-in only.
            if (Config.Version < 11)
            {
                Config.PopOutInputEnabled = false;
                Config.SeenPopOutInputHint = false;
                Config.Version = 11;
                SaveConfig();
                Log.Information(
                    "Migrated config v10 → v11: PopOutInputEnabled added (global, default off), " +
                    "SeenPopOutInputHint added (default false)");
            }

            // Hellion Chat v11 → v12 — flips Configuration.PopOutInputEnabled from
            // the v0.6.0 opt-in default (false) to opt-out (true) per v0.6.1 UX
            // polish. Hard-flip is a deliberate design call (see Spec section 5.7);
            // users are notified via the v0.6.1 hint banner (SeenPopOutHeaderHint
            // reset). Re-toggle after migration is preserved because this block
            // only fires for Version < 12.
            if (Config.Version < 12)
            {
                Config.PopOutInputEnabled = true;
                Config.SeenPopOutHeaderHint = false;
                Config.Version = 12;
                SaveConfig();
                Log.Information(
                    "Migrated config v11 → v12: PopOutInputEnabled hard-flipped to true (v0.6.1 default), " +
                    "SeenPopOutHeaderHint reset to false (v0.6.1 banner re-armed)");
            }

            // Hellion Chat v12 → v13 — hard-resets the tab layout to the
            // sharpened v1.0.0 defaults (5 thematic tabs, see TabsUtil and
            // the default-fill block below). Existing tab state is wiped
            // because per-channel mapping from the old General preset to
            // the new General/System split would be ambiguous and would
            // produce subtly wrong results for users who tweaked the old
            // layout. A timestamped backup of the live config is written
            // alongside it as a manual restore safety net. The wipe scope
            // is intentionally narrow: only Config.Tabs is reset; Privacy,
            // Retention, Theme and every other knob keeps its current value.
            if (Config.Version < 13)
            {
                var pluginConfigsDir = Interface.ConfigDirectory.Parent?.FullName;
                if (pluginConfigsDir is not null)
                {
                    var liveConfigPath = Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json");
                    var backupPath = Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json.pre-v13-backup");

                    try
                    {
                        if (File.Exists(liveConfigPath))
                            File.Copy(liveConfigPath, backupPath, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "HellionChat: pre-v13 config backup failed");
                    }
                }

                Config.Tabs.Clear();
                Config.Version = 13;
                SaveConfig();

                Log.Information(
                    "Migrated config v12 → v13: tab layout hard-reset to v1.0.0 defaults; " +
                    "pre-v13 config backup written next to the live file. " +
                    "Default tabs will be populated by the Tabs.Count == 0 block.");

                Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                {
                    Title = HellionStrings.SettingsRefactor_Migration_Title,
                    Content = HellionStrings.SettingsRefactor_Migration_Content,
                    Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(25),
                });
            }

            // Hellion Chat v13 → v14 — theme-engine migration. Alle User landen
            // auf "hellion-arctic" als neues Default-Theme; die alte
            // HellionThemeEnabled-Flag wird deprecated und nur noch ein Release
            // als Safety-Net im JSON behalten. Window-Opacity wandert von
            // HellionThemeWindowOpacity in das neue WindowOpacity-Feld.
            if (Config.Version < 14)
            {
                Config.Theme = "hellion-arctic";
                // v1.2.0: alter Opacity-Wert wird nicht mehr migriert (Field entfernt).
                // User die direkt v13 → v15 springen bekommen den Default 0.85.
                Config.ReduceMotion = false;
                Config.UseCompactDensity = false;
                Config.Version = 14;
                SaveConfig();
                Log.Information(
                    "Migrated config v13 → v14: theme engine introduced, all users land on hellion-arctic; " +
                    "pick chat2-classic in Settings → Themes for the upstream look");
            }

            if (Config.Version < 15)
            {
                // v1.2.0 — keine Datenmigration nötig. Removal der deprecated
                // Theme-Felder ist reine Schema-Bereinigung (System.Text.Json
                // ignoriert unbekannte Felder im JSON, daher kein Crash bei
                // Configs die noch HellionThemeEnabled/HellionThemeWindowOpacity
                // serialisiert haben — die Werte verfallen einfach).
                Config.Version = 15;
                SaveConfig();
                Log.Information(
                    "Migrated config v14 → v15: legacy theme fields removed " +
                    "(HellionThemeEnabled, HellionThemeWindowOpacity)");
            }

            // Hellion Chat v15 → v16 — Settings Cleanup. Re-Sortierung der
            // Tabs auf der UI-Seite (datenneutral). 4 tote Felder verfallen
            // beim System.Text.Json-Deserialize (OverrideStyle, ChosenStyle,
            // WindowAlpha, ShowThemeQuickPicker — sind alle nicht mehr im
            // Configuration-Schema definiert). WindowAlpha wird zuvor auf
            // WindowOpacity gemappt damit User die ihn gesetzt hatten ihre
            // Transparenz-Einstellung behalten.
            if (Config.Version < 16)
            {
                var pluginConfigsDir = Interface.ConfigDirectory.Parent?.FullName;
                var liveConfigPath = pluginConfigsDir is not null
                    ? Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json")
                    : null;

                // Backup-Datei neben der live Config — Pattern aus v13 Branch.
                if (pluginConfigsDir is not null && liveConfigPath is not null)
                {
                    var backupPath = Path.Combine(pluginConfigsDir, $"{Interface.InternalName}.json.pre-v16-backup");
                    try
                    {
                        if (File.Exists(liveConfigPath))
                            File.Copy(liveConfigPath, backupPath, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "HellionChat: pre-v16 config backup failed");
                    }
                }

                // Pre-v16 Felder einmalig roh aus dem JSON lesen, da sie nicht
                // mehr im Configuration-Schema sind (und damit aus Config nicht
                // mehr abrufbar). WindowAlpha → WindowOpacity Mapping nur wenn
                // User WindowOpacity noch nicht selbst angefasst hat (Default
                // 0.85), sonst gewinnt der User-Wert.
                float oldWindowAlpha = 100f;
                bool oldOverrideStyle = false;
                if (liveConfigPath is not null)
                {
                    try
                    {
                        if (File.Exists(liveConfigPath))
                        {
                            var rawJson = File.ReadAllText(liveConfigPath);
                            using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                            if (doc.RootElement.TryGetProperty("WindowAlpha", out var alphaProp)
                                && alphaProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                oldWindowAlpha = alphaProp.GetSingle();
                            }
                            if (doc.RootElement.TryGetProperty("OverrideStyle", out var ovProp)
                                && ovProp.ValueKind is System.Text.Json.JsonValueKind.True)
                            {
                                oldOverrideStyle = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "HellionChat: pre-v16 legacy-field lookup failed, defaults assumed");
                    }
                }

                if (oldWindowAlpha != 100f
                    && Math.Abs(Config.WindowOpacity - 0.85f) < 0.001f)
                {
                    Config.WindowOpacity = Math.Clamp(oldWindowAlpha / 100f, 0.5f, 1.0f);
                    Log.Information(
                        $"Migrated WindowAlpha {oldWindowAlpha} to WindowOpacity {Config.WindowOpacity}");
                }
                else if (oldWindowAlpha != 100f)
                {
                    Log.Information(
                        $"Skipped WindowAlpha→WindowOpacity migration: WindowOpacity already user-set " +
                        $"({Config.WindowOpacity}), legacy WindowAlpha value {oldWindowAlpha} dropped.");
                }

                if (oldOverrideStyle)
                {
                    Notification.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification
                    {
                        Title = "Hellion Chat 1.2.1",
                        Content = HellionStrings.Migration_v16_OverrideStyle_Toast,
                        Type = Dalamud.Interface.ImGuiNotification.NotificationType.Info,
                        InitialDuration = TimeSpan.FromSeconds(25),
                    });
                }

                // v1.2.1 Default-Bumps für UX-Verbesserungen. Pattern: nur
                // migrieren wenn der User noch auf dem alten Default ist.
                // Bei bool-Werten ist die Erkennung pragmatisch — wer den
                // alten Default aktiv ausgeschaltet hatte, erlebt das als
                // Regression und stellt es einmal in den Settings zurück.
                // Der Trade-Off ist akzeptabel weil die alten Defaults in
                // v1.2.0 erst neu eingeführt wurden und kaum jemand aktiv
                // umgeschaltet hat.
                if (!Config.UseCompactDensity)
                {
                    Config.UseCompactDensity = true;
                    Log.Information("v16 default-bump: UseCompactDensity false → true");
                }
                if (!Config.HideInNewGamePlusMenu)
                {
                    Config.HideInNewGamePlusMenu = true;
                    Log.Information("v16 default-bump: HideInNewGamePlusMenu false → true");
                }
                if (!Config.HideSameTimestamps)
                {
                    Config.HideSameTimestamps = true;
                    Log.Information("v16 default-bump: HideSameTimestamps false → true");
                }
                if (Config.MaxLinesToRender == 5000)
                {
                    Config.MaxLinesToRender = 2500;
                    Log.Information("v16 default-bump: MaxLinesToRender 5000 → 2500");
                }
                if (Config.ChatColours.Count == 0)
                {
                    foreach (var (channel, colour) in Resources.ChatColourPresets.All["Hellion"].Colours)
                        Config.ChatColours[channel] = colour;
                    Log.Information("v16 default-bump: ChatColours empty → Hellion brand preset");
                }

                Config.Version = 16;
                SaveConfig();
                Log.Information(
                    "Migrated config v15 → v16: settings cleanup, " +
                    "OverrideStyle/ChosenStyle/WindowAlpha/ShowThemeQuickPicker dropped from schema");
            }

            // Hellion v1.0.0 default tab layout. Five thematically separated
            // tabs: General catches the immediate-surroundings public chat
            // (Say/Yell/Shout) only; System absorbs the rest of the technical
            // and gameplay-event noise; FreeCompany, Group and Linkshell each
            // own their respective channel set. Tells are not in a static
            // tab anymore — Auto-Tell-Tabs spawns dedicated per-conversation
            // tabs on demand. Novice-Network gets no preset tab; users who
            // want it can add HellionBeginner from Settings → Tabs.
            if (Config.Tabs.Count == 0)
            {
                Config.Tabs.Add(TabsUtil.VanillaGeneral);
                Config.Tabs.Add(TabsUtil.HellionSystem);
                Config.Tabs.Add(TabsUtil.HellionFreeCompany);
                Config.Tabs.Add(TabsUtil.HellionParty);
                Config.Tabs.Add(TabsUtil.HellionLinkshell);
            }

            LanguageChanged(Interface.UiLanguage);
            ImGuiUtil.Initialize(this);

            FileDialogManager = new FileDialogManager();

            Commands = new Commands();
            Functions = new GameFunctions.GameFunctions(this);
            Ipc = new IpcManager();
            TypingIpc = new TypingIpc(this);
            ExtraChat = new ExtraChat();
            FontManager = new FontManager();

            // v1.1.0 — Theme-Engine init. Custom-Themes liegen in
            // pluginConfigs/HellionChat/themes/, lazy geladen beim ersten Get.
            var customThemesDir = Path.Combine(Interface.ConfigDirectory.FullName, "themes");
            Directory.CreateDirectory(customThemesDir);
            SeedExampleThemeIfEmpty(customThemesDir);
            ThemeRegistry = new Themes.ThemeRegistry(customThemesDir);
            ThemeRegistry.Switch(Config.Theme);

            // Plugin integrations register their IPC subscribers up-front so
            // Ready/Disposing events from the target plugins are caught from
            // the very first frame, even if the user's Honorific reloads
            // mid-session. See HellionChat/Integrations/HonorificService.cs.
            HonorificService = new Integrations.HonorificService(Interface, Log, Framework);

            StatusBar = new Ui.StatusBar();

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
                _ = EmoteCache.LoadData(); // Fire-and-forget intentional, exceptions are caught inside

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

        HonorificService?.Dispose();

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

        // IsBackground = true for the same reason as PendingMessageThread:
        // a stuck sweep must never block plugin unload. RunRetentionSweepIfDue
        // guards the run-frequency, and the sweep itself uses the framework's
        // cooperative cancellation pattern. The background flag is the safety
        // net if the sweep ever takes longer than expected.
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
                    // Run the clear+refilter synchronously on the framework thread.
                    // Earlier this called FilterAllTabsAsync(), which is fire-and-forget
                    // — the .Wait() here would return as soon as the inner Task.Run was
                    // dispatched, racing the next sweep cycle against the still-running
                    // filter pass. See AUDIT-2026-05-05 [QUAL-02].
                    Framework.Run(() =>
                    {
                        MessageManager.ClearAllTabs();
                        MessageManager.FilterAllTabs();
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
        // Theme-Engine ist ab v14 immer aktiv; Klassik ist jetzt ein eigenes
        // Theme statt einem deaktivierten Hellion-Theme. Active wird einmal
        // pro Frame aus der Registry gelesen.
        using IDisposable _style = HellionStyle.PushGlobal(ThemeRegistry.Active, Config.WindowOpacity);

        ChatLogWindow.BeginFrame();

        if (Config.HideInLoadingScreens && Condition[ConditionFlag.BetweenAreas])
        {
            ChatLogWindow.FinalizeFrame();
            TypingIpc.Update();
            return;
        }

        // v1.0.2 — global skip while the New Game+ menu (QuestRedo addon) is
        // open. Hides every plugin window in one shot (chat log, pop-outs,
        // settings, db viewer, etc.), matching the LoadingScreens pattern.
        if (Config.HideInNewGamePlusMenu && GameFunctions.GameFunctions.IsAddonInteractable(GameFunctions.GameFunctions.NewGamePlusAddonName))
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

    // v1.1.0 — wenn der themes/-Ordner leer ist, schreiben wir die embedded
    // example-theme.json als Vorlage rein. Bestehende User-Customs werden
    // nicht angefasst (existing JSONs lassen den Block überspringen).
    private static void SeedExampleThemeIfEmpty(string dir)
    {
        if (Directory.EnumerateFiles(dir, "*.json").Any())
            return;

        var examplePath = Path.Combine(dir, "example-theme.json");
        var resourceStream = typeof(Plugin).Assembly.GetManifestResourceStream("HellionChat.Themes.Builtin.example-theme.json");
        if (resourceStream is null)
        {
            Log.Warning("Themes example template not found in assembly resources; skipping seed.");
            return;
        }

        try
        {
            using var fileStream = File.Create(examplePath);
            resourceStream.CopyTo(fileStream);
            Log.Information($"Seeded example-theme.json into {dir}");
        }
        catch (IOException ex)
        {
            Log.Warning(ex, "Failed to seed example-theme.json; user can create custom themes manually.");
        }
        finally
        {
            resourceStream.Dispose();
        }
    }
}
