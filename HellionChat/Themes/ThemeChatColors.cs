using HellionChat.Code;

namespace HellionChat.Themes;

// Optional pro Theme. Wenn ein Theme ChatColors mitliefert, kann der
// User sie per Klick im Themes-Tab auf Configuration.ChatColours anwenden.
// Ein Theme ohne ChatColors (z.B. chat2-classic) lässt die User-Channel-
// Farben unverändert.
public sealed record ThemeChatColors(
    IReadOnlyDictionary<ChatType, uint> Channels
);
