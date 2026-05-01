using ChatTwo.Util;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ChatTwo.Ui;

/// <summary>
/// ImGui style override for Hellion Chat. Industrial HUD palette with three
/// distinct accents — cyan-teal as the primary action color, industrial
/// amber for active state highlights, slate-violet for title bars and
/// active tabs — on a deep-slate frame background with steel borders.
///
/// Two entry points:
///   Push       — local color stack, scoped via using-block. Use inside
///                Hellion-only surfaces (Privacy tab, first-run wizard).
///   PushGlobal — full color + style variable stack. Pushed once per frame
///                in Plugin.Draw so every Hellion-rendered window inherits
///                the look. Cheap to pop because ImGui keeps its own stack.
/// </summary>
internal static class HellionStyle
{
    // Encoded as 0xRRGGBBAA, matching ChatTwo convention (see Settings.cs
    // Ko-fi buttons). RgbaToAbgr handles the byte swap to the format ImGui
    // expects.

    // Primary — cyan-teal for actionable controls (buttons, checks, sliders).
    private const uint PrimaryRgba = 0x00B8D4FF;
    private const uint PrimaryHoverRgba = 0x26C6DAFF;
    private const uint PrimaryActiveRgba = 0x00838FFF;

    // Secondary — industrial amber, used as a warm highlight for active
    // states (tab borders, resize grips, scrollbar grabs).
    private const uint SecondaryRgba = 0xFFB300FF;
    private const uint SecondaryHoverRgba = 0xFFC940FF;
    private const uint SecondaryActiveRgba = 0xC68400FF;

    // Tertiary — slate violet, reserved for title bars and the active tab
    // background so identity beats out the cyan accent without competing
    // with it on action controls.
    private const uint TertiaryRgba = 0x7B61FFFF;
    private const uint TertiaryHoverRgba = 0x9580FFFF;
    private const uint TertiaryActiveRgba = 0x5E45D9FF;

    // Surfaces — deep slate window/frame backgrounds, steel borders.
    private const uint WindowBgRgba = 0x0E1A20FF;
    private const uint ChildBgRgba = 0x102027FF;
    private const uint PopupBgRgba = 0x102027FF;
    private const uint FrameBgRgba = 0x162831FF;
    private const uint FrameBgHoverRgba = 0x1F3540FF;
    private const uint FrameBgActiveRgba = 0x274250FF;
    private const uint BorderRgba = 0x37474FFF;
    private const uint BorderShadowRgba = 0x00000000;

    // Headers / collapsing-headers / tree nodes / selectables.
    private const uint HeaderRgba = 0x1B2C36FF;
    private const uint HeaderHoverRgba = 0x263A45FF;
    private const uint HeaderActiveRgba = 0x324A57FF;

    // Title bars — tertiary identity for the active state.
    private const uint TitleBgRgba = 0x0E1A20FF;
    private const uint TitleBgActiveRgba = 0x5E45D9FF;
    private const uint TitleBgCollapsedRgba = 0x0A1318FF;

    // Tabs — tertiary tint, secondary highlight while hovered/unfocused.
    private const uint TabRgba = 0x162831FF;
    private const uint TabHoveredRgba = 0x9580FFFF;
    private const uint TabActiveRgba = 0x7B61FFFF;
    private const uint TabUnfocusedRgba = 0x12222AFF;
    private const uint TabUnfocusedActiveRgba = 0x5E45D9FF;

    // Scrollbar — slate base, secondary amber on grab.
    private const uint ScrollbarBgRgba = 0x0E1A20FF;
    private const uint ScrollbarGrabRgba = 0x37474FFF;
    private const uint ScrollbarGrabHoveredRgba = 0xFFC940FF;
    private const uint ScrollbarGrabActiveRgba = 0xFFB300FF;

    // Resize grip — secondary amber for the active corner pull.
    private const uint ResizeGripRgba = 0x37474FFF;
    private const uint ResizeGripHoveredRgba = 0xFFC940FF;
    private const uint ResizeGripActiveRgba = 0xFFB300FF;

    // Separator and check mark / slider follow the primary cyan.

    /// <summary>
    /// Local color stack for Hellion-only surfaces. Cheap. Use inside a
    /// `using var _ = HellionStyle.Push();` block.
    /// </summary>
    internal static IDisposable Push()
    {
        var stack = new StackHandle();
        stack.PushColor(ImGuiCol.Button, PrimaryRgba);
        stack.PushColor(ImGuiCol.ButtonHovered, PrimaryHoverRgba);
        stack.PushColor(ImGuiCol.ButtonActive, PrimaryActiveRgba);
        stack.PushColor(ImGuiCol.FrameBg, FrameBgRgba);
        stack.PushColor(ImGuiCol.FrameBgHovered, FrameBgHoverRgba);
        stack.PushColor(ImGuiCol.FrameBgActive, FrameBgActiveRgba);
        stack.PushColor(ImGuiCol.Border, BorderRgba);
        stack.PushColor(ImGuiCol.Header, HeaderRgba);
        stack.PushColor(ImGuiCol.HeaderHovered, HeaderHoverRgba);
        stack.PushColor(ImGuiCol.HeaderActive, HeaderActiveRgba);
        stack.PushColor(ImGuiCol.CheckMark, PrimaryRgba);
        stack.PushColor(ImGuiCol.SliderGrab, PrimaryRgba);
        stack.PushColor(ImGuiCol.SliderGrabActive, PrimaryHoverRgba);
        return stack;
    }

    /// <summary>
    /// Global color and style-variable stack pushed once per frame in
    /// Plugin.Draw. Covers every ImGui surface the plugin renders so the
    /// Hellion look is consistent across upstream and Hellion tabs.
    /// </summary>
    internal static IDisposable PushGlobal()
    {
        var stack = new StackHandle();

        // Layout — geometric edges, modest rounding, single-pixel borders.
        stack.PushStyleVar(ImGuiStyleVar.WindowRounding, 4f);
        stack.PushStyleVar(ImGuiStyleVar.ChildRounding, 3f);
        stack.PushStyleVar(ImGuiStyleVar.PopupRounding, 3f);
        stack.PushStyleVar(ImGuiStyleVar.FrameRounding, 2f);
        stack.PushStyleVar(ImGuiStyleVar.GrabRounding, 2f);
        stack.PushStyleVar(ImGuiStyleVar.TabRounding, 2f);
        stack.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 2f);
        stack.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
        stack.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);

        // Surfaces.
        stack.PushColor(ImGuiCol.WindowBg, WindowBgRgba);
        stack.PushColor(ImGuiCol.ChildBg, ChildBgRgba);
        stack.PushColor(ImGuiCol.PopupBg, PopupBgRgba);
        stack.PushColor(ImGuiCol.Border, BorderRgba);
        stack.PushColor(ImGuiCol.BorderShadow, BorderShadowRgba);

        // Frames (input fields, combos, sliders).
        stack.PushColor(ImGuiCol.FrameBg, FrameBgRgba);
        stack.PushColor(ImGuiCol.FrameBgHovered, FrameBgHoverRgba);
        stack.PushColor(ImGuiCol.FrameBgActive, FrameBgActiveRgba);

        // Title bars — tertiary identity on active.
        stack.PushColor(ImGuiCol.TitleBg, TitleBgRgba);
        stack.PushColor(ImGuiCol.TitleBgActive, TitleBgActiveRgba);
        stack.PushColor(ImGuiCol.TitleBgCollapsed, TitleBgCollapsedRgba);

        // Buttons — primary cyan.
        stack.PushColor(ImGuiCol.Button, PrimaryRgba);
        stack.PushColor(ImGuiCol.ButtonHovered, PrimaryHoverRgba);
        stack.PushColor(ImGuiCol.ButtonActive, PrimaryActiveRgba);

        // Headers / selectables — slate with subtle steps.
        stack.PushColor(ImGuiCol.Header, HeaderRgba);
        stack.PushColor(ImGuiCol.HeaderHovered, HeaderHoverRgba);
        stack.PushColor(ImGuiCol.HeaderActive, HeaderActiveRgba);

        // Tabs — tertiary identity for the active tab.
        stack.PushColor(ImGuiCol.Tab, TabRgba);
        stack.PushColor(ImGuiCol.TabHovered, TabHoveredRgba);
        stack.PushColor(ImGuiCol.TabActive, TabActiveRgba);
        stack.PushColor(ImGuiCol.TabUnfocused, TabUnfocusedRgba);
        stack.PushColor(ImGuiCol.TabUnfocusedActive, TabUnfocusedActiveRgba);

        // Scrollbar.
        stack.PushColor(ImGuiCol.ScrollbarBg, ScrollbarBgRgba);
        stack.PushColor(ImGuiCol.ScrollbarGrab, ScrollbarGrabRgba);
        stack.PushColor(ImGuiCol.ScrollbarGrabHovered, ScrollbarGrabHoveredRgba);
        stack.PushColor(ImGuiCol.ScrollbarGrabActive, ScrollbarGrabActiveRgba);

        // Resize grip — secondary amber on active.
        stack.PushColor(ImGuiCol.ResizeGrip, ResizeGripRgba);
        stack.PushColor(ImGuiCol.ResizeGripHovered, ResizeGripHoveredRgba);
        stack.PushColor(ImGuiCol.ResizeGripActive, ResizeGripActiveRgba);

        // Check mark + slider grab — primary cyan.
        stack.PushColor(ImGuiCol.CheckMark, PrimaryRgba);
        stack.PushColor(ImGuiCol.SliderGrab, PrimaryRgba);
        stack.PushColor(ImGuiCol.SliderGrabActive, PrimaryHoverRgba);

        // Separator — primary cyan when hovered/active so the eye
        // immediately sees that splitters are interactive.
        stack.PushColor(ImGuiCol.Separator, BorderRgba);
        stack.PushColor(ImGuiCol.SeparatorHovered, PrimaryHoverRgba);
        stack.PushColor(ImGuiCol.SeparatorActive, PrimaryRgba);

        return stack;
    }

    private sealed class StackHandle : IDisposable
    {
        private readonly List<IDisposable> _items = new(64);

        internal void PushColor(ImGuiCol slot, uint rgba)
            => _items.Add(ImRaii.PushColor(slot, ColourUtil.RgbaToAbgr(rgba)));

        internal void PushStyleVar(ImGuiStyleVar var, float value)
            => _items.Add(ImRaii.PushStyle(var, value));

        public void Dispose()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
                _items[i].Dispose();
            _items.Clear();
        }
    }
}
