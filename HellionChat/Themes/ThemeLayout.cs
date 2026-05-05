namespace HellionChat.Themes;

// Layout-Werte spiegeln die ImGuiStyleVar-Slots, die HellionStyle pusht.
public sealed record ThemeLayout(
    float WindowRounding,
    float ChildRounding,
    float PopupRounding,
    float FrameRounding,
    float GrabRounding,
    float TabRounding,
    float ScrollbarRounding,
    float WindowBorderSize,
    float FrameBorderSize
);
