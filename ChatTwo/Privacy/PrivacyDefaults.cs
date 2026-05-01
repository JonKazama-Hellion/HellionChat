using ChatTwo.Code;

namespace ChatTwo.Privacy;

internal static class PrivacyDefaults
{
    // Privacy-First default whitelist (DSGVO Art. 25 - Privacy by Default).
    // Only the player's own conversations are persisted out-of-the-box.
    // Public chat (Say/Shout/Yell), Novice Network, NPC dialogue, system
    // logs and battle messages are NOT persisted unless the user opts in.
    internal static readonly IReadOnlySet<ChatType> PrivacyFirstWhitelist = new HashSet<ChatType>
    {
        ChatType.TellIncoming,
        ChatType.TellOutgoing,
        ChatType.Party,
        ChatType.CrossParty,
        ChatType.Alliance,
        ChatType.FreeCompany,
        ChatType.Linkshell1,
        ChatType.Linkshell2,
        ChatType.Linkshell3,
        ChatType.Linkshell4,
        ChatType.Linkshell5,
        ChatType.Linkshell6,
        ChatType.Linkshell7,
        ChatType.Linkshell8,
        ChatType.CrossLinkshell1,
        ChatType.CrossLinkshell2,
        ChatType.CrossLinkshell3,
        ChatType.CrossLinkshell4,
        ChatType.CrossLinkshell5,
        ChatType.CrossLinkshell6,
        ChatType.CrossLinkshell7,
        ChatType.CrossLinkshell8,
        ChatType.ExtraChatLinkshell1,
        ChatType.ExtraChatLinkshell2,
        ChatType.ExtraChatLinkshell3,
        ChatType.ExtraChatLinkshell4,
        ChatType.ExtraChatLinkshell5,
        ChatType.ExtraChatLinkshell6,
        ChatType.ExtraChatLinkshell7,
        ChatType.ExtraChatLinkshell8,
    };

    // Default retention windows per channel (in days). Channels not listed
    // here fall back to Configuration.RetentionDefaultDays. Reflects the
    // design spec: Tells 365, own-conversation channels 90, everything else
    // shorter via the global default.
    internal static readonly IReadOnlyDictionary<ChatType, int> DefaultRetentionDays = new Dictionary<ChatType, int>
    {
        [ChatType.TellIncoming] = 365,
        [ChatType.TellOutgoing] = 365,

        [ChatType.Party] = 90,
        [ChatType.CrossParty] = 90,
        [ChatType.Alliance] = 90,
        [ChatType.PvpTeam] = 90,
        [ChatType.FreeCompany] = 90,

        [ChatType.Linkshell1] = 90,
        [ChatType.Linkshell2] = 90,
        [ChatType.Linkshell3] = 90,
        [ChatType.Linkshell4] = 90,
        [ChatType.Linkshell5] = 90,
        [ChatType.Linkshell6] = 90,
        [ChatType.Linkshell7] = 90,
        [ChatType.Linkshell8] = 90,

        [ChatType.CrossLinkshell1] = 90,
        [ChatType.CrossLinkshell2] = 90,
        [ChatType.CrossLinkshell3] = 90,
        [ChatType.CrossLinkshell4] = 90,
        [ChatType.CrossLinkshell5] = 90,
        [ChatType.CrossLinkshell6] = 90,
        [ChatType.CrossLinkshell7] = 90,
        [ChatType.CrossLinkshell8] = 90,

        [ChatType.ExtraChatLinkshell1] = 90,
        [ChatType.ExtraChatLinkshell2] = 90,
        [ChatType.ExtraChatLinkshell3] = 90,
        [ChatType.ExtraChatLinkshell4] = 90,
        [ChatType.ExtraChatLinkshell5] = 90,
        [ChatType.ExtraChatLinkshell6] = 90,
        [ChatType.ExtraChatLinkshell7] = 90,
        [ChatType.ExtraChatLinkshell8] = 90,
    };

    // Casual profile = Privacy-First plus public chat (Say/Shout/Yell, both
    // emote types, Novice Network), kept for a short 24-hour window so the
    // last RP scene or shout trade is still searchable but third-party data
    // doesn't accumulate forever.
    internal static readonly IReadOnlySet<ChatType> CasualWhitelist = new HashSet<ChatType>(PrivacyFirstWhitelist)
    {
        ChatType.Say,
        ChatType.Shout,
        ChatType.Yell,
        ChatType.CustomEmote,
        ChatType.StandardEmote,
        ChatType.NoviceNetwork,
    };

    internal static readonly IReadOnlyDictionary<ChatType, int> CasualRetentionOverrides = new Dictionary<ChatType, int>
    {
        [ChatType.Say] = 1,
        [ChatType.Shout] = 1,
        [ChatType.Yell] = 1,
        [ChatType.CustomEmote] = 1,
        [ChatType.StandardEmote] = 1,
        [ChatType.NoviceNetwork] = 1,
    };
}
