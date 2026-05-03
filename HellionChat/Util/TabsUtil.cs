using HellionChat.Code;
using HellionChat.Resources;

namespace HellionChat.Util;

public static class TabsUtil
{
    public static Dictionary<ChatType, (ChatSource, ChatSource)> AllChannels()
    {
        var channels = new Dictionary<ChatType, (ChatSource, ChatSource)>();
        foreach (var chatType in Enum.GetValues<ChatType>())
            channels[chatType] = (ChatSourceExt.All, ChatSourceExt.All);

        return channels;
    }

    // Hellion-tuned General preset (v1.0.0 — sharpened defaults).
    // Public-chat-only, the bare three channels you encounter in open
    // world. Group/FC/Linkshell traffic moves to dedicated tabs, gameplay
    // events (loot, crafting, gathering, NPC dialogue, PF pings) move to
    // the System tab where they belong — keeps the General view focused
    // on actual conversation in the immediate surroundings.
    public static Tab VanillaGeneral => new()
    {
        Name = Language.Tabs_Presets_General,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.Say] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Yell] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Shout] = (ChatSourceExt.All, ChatSourceExt.All),
        }
    };

    public static Tab VanillaEvent => new()
    {
        Name = Language.Tabs_Presets_Event,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)> { [ChatType.NpcDialogue] = (ChatSourceExt.All, ChatSourceExt.All), },
    };

    public static Tab VanillaTellExclusive => new()
    {
        Name = Language.Tabs_Presets_Tell,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.TellIncoming] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.TellOutgoing] = (ChatSourceExt.All, ChatSourceExt.All),
        },
        Channel = InputChannel.Tell,
        AllSenderMessages = true,
    };

    // Hellion default-tab presets used by the v10 wipe migration. Names are
    // kept in HellionStrings (EN+DE) instead of Language.* so the upstream
    // resource files stay untouched. Channel selections cover the channels
    // a typical Eorzea raider uses without forcing the user to hand-tick
    // each box on first start.
    public static Tab HellionFreeCompany => new()
    {
        Name = HellionStrings.Tabs_Presets_FreeCompany,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.FreeCompany] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
        },
        Channel = InputChannel.FreeCompany,
    };

    public static Tab HellionParty => new()
    {
        Name = HellionStrings.Tabs_Presets_Party,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.Party] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossParty] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Alliance] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeam] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.LootNotice] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.LootRoll] = (ChatSourceExt.All, ChatSourceExt.All),
        },
        // No automatic input-channel switch; the Gruppe tab is a read
        // surface that pulls in Party, CrossParty, Alliance and PvpTeam
        // together. Auto-routing /party into this tab would surprise the
        // user when they actually wanted /alliance or /pvpteam.
    };

    public static Tab HellionBeginner => new()
    {
        Name = HellionStrings.Tabs_Presets_Beginner,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.NoviceNetwork] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.NoviceNetworkSystem] = (ChatSourceExt.All, ChatSourceExt.All),
        },
        Channel = InputChannel.NoviceNetwork,
    };

    public static Tab HellionSystem => new()
    {
        Name = HellionStrings.Tabs_Presets_System,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            // Plain system noise
            [ChatType.Debug] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Urgent] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Notice] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.System] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Error] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Echo] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.GatheringSystem] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.NoviceNetworkSystem] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.BattleSystem] = (ChatSourceExt.All, ChatSourceExt.All),
            // Login / logout / announcement noise
            [ChatType.NpcAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.RetainerSale] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PeriodicRecruitmentNotification] = (ChatSourceExt.All, ChatSourceExt.All),
            // Gameplay-event streams (moved out of General in v1.0.0)
            [ChatType.NpcDialogue] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.LootNotice] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.LootRoll] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Crafting] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Gathering] = (ChatSourceExt.All, ChatSourceExt.All),
            // Misc
            [ChatType.Progress] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.RandomNumber] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Orchestrion] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.MessageBook] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Alarm] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Sign] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.GlamourNotifications] = (ChatSourceExt.All, ChatSourceExt.All),
        },
    };

    public static Tab HellionLinkshell => new()
    {
        Name = HellionStrings.Tabs_Presets_Linkshell,
        SelectedChannels = new Dictionary<ChatType, (ChatSource, ChatSource)>
        {
            [ChatType.Linkshell1] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell2] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell3] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell4] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell5] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell6] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell7] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell8] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell1] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell2] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell3] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell4] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell5] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell6] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell7] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell8] = (ChatSourceExt.All, ChatSourceExt.All),
        },
    };

    public static Dictionary<ChatType, (ChatSource, ChatSource)> MostlyPlayer => new()
    {
            // Special
            [ChatType.Debug] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Urgent] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Notice] = (ChatSourceExt.All, ChatSourceExt.All),
            // Chat
            [ChatType.Say] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Yell] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Shout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.TellIncoming] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.TellOutgoing] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Party] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossParty] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Alliance] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompany] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeam] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell1] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell2] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell3] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell4] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell5] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell6] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell7] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CrossLinkshell8] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell1] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell2] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell3] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell4] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell5] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell6] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell7] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Linkshell8] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.NoviceNetwork] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.StandardEmote] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.CustomEmote] = (ChatSourceExt.All, ChatSourceExt.All),
            // Announcements
            [ChatType.System] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Error] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.Echo] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.NoviceNetworkSystem] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamAnnouncement] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.FreeCompanyLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.PvpTeamLoginLogout] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.RandomNumber] = (ChatSourceExt.All, ChatSourceExt.All),
            [ChatType.MessageBook] = (ChatSourceExt.All, ChatSourceExt.All),
    };
}
