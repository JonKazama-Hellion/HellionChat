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
}
