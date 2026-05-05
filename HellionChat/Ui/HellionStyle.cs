using HellionChat.Themes;
using HellionChat.Util;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace HellionChat.Ui;

/// <summary>
/// ImGui style override for Hellion Chat. v1.1.0 ist die Engine
/// theme-getrieben: PushGlobal nimmt eine Theme-Instance + Window-
/// Opacity, die gesamten Color- und Style-Slots werden aus dem Theme
/// gelesen statt aus einer fixen Konstanten-Tabelle.
/// </summary>
internal static class HellionStyle
{
    /// <summary>
    /// Local color stack auf Basis des aktiven Themes. Cheap. Use inside a
    /// `using var _ = HellionStyle.Push(theme);` block.
    /// </summary>
    internal static IDisposable Push(Theme theme)
    {
        var c = theme.Colors;
        var stack = new StackHandle();
        stack.PushColor(ImGuiCol.Button,           c.Primary);
        stack.PushColor(ImGuiCol.ButtonHovered,    c.PrimaryLight);
        stack.PushColor(ImGuiCol.ButtonActive,     c.PrimaryDark);
        stack.PushColor(ImGuiCol.FrameBg,          c.FrameBg);
        stack.PushColor(ImGuiCol.FrameBgHovered,   c.SurfaceHover);
        stack.PushColor(ImGuiCol.FrameBgActive,    c.Surface);
        stack.PushColor(ImGuiCol.Border,           c.Border);
        stack.PushColor(ImGuiCol.Header,           c.Surface);
        stack.PushColor(ImGuiCol.HeaderHovered,    c.SurfaceHover);
        stack.PushColor(ImGuiCol.HeaderActive,     c.Identity);
        stack.PushColor(ImGuiCol.CheckMark,        c.Primary);
        stack.PushColor(ImGuiCol.SliderGrab,       c.Primary);
        stack.PushColor(ImGuiCol.SliderGrabActive, c.PrimaryLight);
        return stack;
    }

    /// <summary>
    /// Global color and style-variable stack pushed once per frame in
    /// Plugin.Draw. Drives every Hellion-rendered window from the active
    /// theme's palette and layout values.
    /// </summary>
    /// <param name="theme">Active theme from ThemeRegistry.</param>
    /// <param name="windowOpacity">Window background alpha (0.5–1.0).</param>
    internal static IDisposable PushGlobal(Theme theme, float windowOpacity = 1.0f)
    {
        var c = theme.Colors;
        var l = theme.Layout;
        var stack = new StackHandle();

        var alphaByte = (uint)Math.Clamp((int)(windowOpacity * 255f), 0x55, 0xFF);
        var windowBgWithAlpha = (c.WindowBg & 0xFFFFFF00u) | alphaByte;
        var childBgWithAlpha  = (c.ChildBg  & 0xFFFFFF00u) | alphaByte;

        // Layout
        stack.PushStyleVar(ImGuiStyleVar.WindowRounding,    l.WindowRounding);
        stack.PushStyleVar(ImGuiStyleVar.ChildRounding,     l.ChildRounding);
        stack.PushStyleVar(ImGuiStyleVar.PopupRounding,     l.PopupRounding);
        stack.PushStyleVar(ImGuiStyleVar.FrameRounding,     l.FrameRounding);
        stack.PushStyleVar(ImGuiStyleVar.GrabRounding,      l.GrabRounding);
        stack.PushStyleVar(ImGuiStyleVar.TabRounding,       l.TabRounding);
        stack.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, l.ScrollbarRounding);
        stack.PushStyleVar(ImGuiStyleVar.WindowBorderSize,  l.WindowBorderSize);
        stack.PushStyleVar(ImGuiStyleVar.FrameBorderSize,   l.FrameBorderSize);

        // Surfaces
        stack.PushColor(ImGuiCol.WindowBg,    windowBgWithAlpha);
        stack.PushColor(ImGuiCol.ChildBg,     childBgWithAlpha);
        stack.PushColor(ImGuiCol.PopupBg,     c.ChildBg);
        stack.PushColor(ImGuiCol.Border,      c.Border);
        stack.PushColor(ImGuiCol.BorderShadow, 0u);

        // Frames
        stack.PushColor(ImGuiCol.FrameBg,        c.FrameBg);
        stack.PushColor(ImGuiCol.FrameBgHovered, c.SurfaceHover);
        stack.PushColor(ImGuiCol.FrameBgActive,  c.Surface);

        // Title bars
        stack.PushColor(ImGuiCol.TitleBg,          c.WindowBg);
        stack.PushColor(ImGuiCol.TitleBgActive,    c.Identity);
        stack.PushColor(ImGuiCol.TitleBgCollapsed, c.WindowBg);

        // Buttons
        stack.PushColor(ImGuiCol.Button,        c.Primary);
        stack.PushColor(ImGuiCol.ButtonHovered, c.PrimaryLight);
        stack.PushColor(ImGuiCol.ButtonActive,  c.PrimaryDark);

        // Headers / selectables
        stack.PushColor(ImGuiCol.Header,        c.Surface);
        stack.PushColor(ImGuiCol.HeaderHovered, c.SurfaceHover);
        stack.PushColor(ImGuiCol.HeaderActive,  c.Identity);

        // Tabs
        stack.PushColor(ImGuiCol.Tab,                c.FrameBg);
        stack.PushColor(ImGuiCol.TabHovered,         c.PrimaryLight);
        stack.PushColor(ImGuiCol.TabActive,          c.Identity);
        stack.PushColor(ImGuiCol.TabUnfocused,       c.ChildBg);
        stack.PushColor(ImGuiCol.TabUnfocusedActive, c.PrimaryDark);

        // Scrollbar
        stack.PushColor(ImGuiCol.ScrollbarBg,          c.WindowBg);
        stack.PushColor(ImGuiCol.ScrollbarGrab,        c.Surface);
        stack.PushColor(ImGuiCol.ScrollbarGrabHovered, c.AccentLight);
        stack.PushColor(ImGuiCol.ScrollbarGrabActive,  c.Accent);

        // Resize grip
        stack.PushColor(ImGuiCol.ResizeGrip,        c.FrameBg);
        stack.PushColor(ImGuiCol.ResizeGripHovered, c.AccentLight);
        stack.PushColor(ImGuiCol.ResizeGripActive,  c.Accent);

        // Check mark + slider grab
        stack.PushColor(ImGuiCol.CheckMark,         c.Primary);
        stack.PushColor(ImGuiCol.SliderGrab,        c.Primary);
        stack.PushColor(ImGuiCol.SliderGrabActive,  c.PrimaryLight);

        // Separator
        stack.PushColor(ImGuiCol.Separator,         c.Border);
        stack.PushColor(ImGuiCol.SeparatorHovered,  c.PrimaryLight);
        stack.PushColor(ImGuiCol.SeparatorActive,   c.Primary);

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
