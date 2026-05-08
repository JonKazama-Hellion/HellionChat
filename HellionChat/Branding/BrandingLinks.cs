// HellionChat/Branding/BrandingLinks.cs
namespace HellionChat.Branding;

// Centralised so a future invite rotation only touches one file. The same
// link is currently hard-coded in repo.json, README.md, SUPPORT.md,
// CONTRIBUTORS.md and HellionChat.yaml — those will be migrated to consume
// this constant in a separate housekeeping sweep
internal static class BrandingLinks
{
    public const string HellionForgeDiscordInvite = "https://discord.gg/X9V7Kcv5gR";
}
