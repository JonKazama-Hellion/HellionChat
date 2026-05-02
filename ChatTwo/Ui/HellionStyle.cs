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
    // expects. Hex values are sourced from the Hellion Online Media brand
    // guide ("Arctic Cyan + Ember Glow", BRANDING.md in the website repo).

    // Primary — Arctic Cyan, used for every interactive control (buttons,
    // checks, sliders, separators when hovered). Three brand stages plus a
    // hover that lifts to brand-color-light and a press that drops to
    // brand-color-dark.
    private const uint PrimaryRgba = 0x00BED2FF;       // brand-color
    private const uint PrimaryHoverRgba = 0x4DD9E8FF;  // brand-color-light
    private const uint PrimaryActiveRgba = 0x0097A7FF; // brand-color-dark

    // Identity — brand-color-dark teal for window title bars and the
    // active tab. Sits visibly below the primary cyan on buttons so the
    // user sees "where am I" (deep teal) versus "what can I click"
    // (brand cyan) without leaving the cyan family.
    private const uint IdentityRgba = 0x0097A7FF;       // brand-color-dark
    private const uint IdentityHoverRgba = 0x4DD9E8FF;  // brand-color-light
    private const uint IdentityDeepRgba = 0x005670FF;   // dimmer teal for unfocused-active tab

    // Accent — Ember Orange for warm highlights on grips and scrollbar
    // pulls. Replaces the previous industrial amber so the plugin matches
    // the website's CTA palette. AccentActive is reserved for any future
    // pressed-state on accent surfaces; the current slots only need
    // AccentRgba and AccentHoverRgba.
    private const uint AccentRgba = 0xF97316FF;        // accent-color
    private const uint AccentHoverRgba = 0xFB923CFF;   // accent-color-light

    // Surfaces — Hellion brand background ladder. Window darkest, frame
    // hover ladder climbs into surface tones. Matches the website's
    // background / background-medium / background-light / surface vars.
    private const uint WindowBgRgba = 0x070B12FF;       // background
    private const uint ChildBgRgba = 0x0C1220FF;        // background-medium
    private const uint PopupBgRgba = 0x0C1220FF;        // background-medium
    private const uint FrameBgRgba = 0x141E30FF;        // background-light
    private const uint FrameBgHoverRgba = 0x1A2538FF;   // surface
    private const uint FrameBgActiveRgba = 0x22303FFF;  // surface-hover
    // Cyan-tinted border — matches website --border-brand (cyan @ 40% α).
    private const uint BorderRgba = 0x00BED266;
    private const uint BorderShadowRgba = 0x00000000;

    // Headers / collapsing-headers / tree nodes / selectables — same
    // surface ladder as frames so panels feel consistent.
    private const uint HeaderRgba = 0x141E30FF;
    private const uint HeaderHoverRgba = 0x1A2538FF;
    private const uint HeaderActiveRgba = 0x22303FFF;

    // Title bars — Identity teal on active so the focused window reads
    // as "yours" without using accent or primary slots.
    private const uint TitleBgRgba = 0x070B12FF;
    private const uint TitleBgActiveRgba = IdentityRgba;
    private const uint TitleBgCollapsedRgba = 0x05080EFF;

    // Tabs — neutral inactive, Identity-light on hover, Identity teal on
    // active. Unfocused-active uses the deeper Identity stage so an
    // unfocused window's active tab still reads but does not pull focus.
    private const uint TabRgba = 0x141E30FF;
    private const uint TabHoveredRgba = IdentityHoverRgba;
    private const uint TabActiveRgba = IdentityRgba;
    private const uint TabUnfocusedRgba = 0x0C1220FF;
    private const uint TabUnfocusedActiveRgba = IdentityDeepRgba;

    // Scrollbar — Ember on grab so the pull stands out without competing
    // with the cyan action buttons. Idle grab is a subtle surface tone,
    // hover/active climb into accent.
    private const uint ScrollbarBgRgba = 0x070B12FF;
    private const uint ScrollbarGrabRgba = 0x22303FFF;       // surface-hover
    private const uint ScrollbarGrabHoveredRgba = AccentHoverRgba;
    private const uint ScrollbarGrabActiveRgba = AccentRgba;

    // Resize grip — same Ember treatment as the scrollbar.
    private const uint ResizeGripRgba = 0x141E30FF;
    private const uint ResizeGripHoveredRgba = AccentHoverRgba;
    private const uint ResizeGripActiveRgba = AccentRgba;

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
    /// <param name="windowOpacity">Window background alpha (0.5–1.0). Lower
    /// values let the game shine through the plugin panes.</param>
    internal static IDisposable PushGlobal(float windowOpacity = 1.0f)
    {
        var stack = new StackHandle();

        // Mix the configured opacity into both the outer window and the
        // inner content child backgrounds — without ChildBg following the
        // slider the chat log stays opaque inside even when the user
        // wants to see the game behind it during combat. Form fields and
        // popups (FrameBg, PopupBg) still stay opaque so input is readable.
        var alphaByte = (uint)Math.Clamp((int)(windowOpacity * 255f), 0x55, 0xFF);
        var windowBgWithAlpha = (WindowBgRgba & 0xFFFFFF00u) | alphaByte;
        var childBgWithAlpha = (ChildBgRgba & 0xFFFFFF00u) | alphaByte;

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
        stack.PushColor(ImGuiCol.WindowBg, windowBgWithAlpha);
        stack.PushColor(ImGuiCol.ChildBg, childBgWithAlpha);
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
