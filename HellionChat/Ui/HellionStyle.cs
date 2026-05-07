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
        var a = theme.AbgrCache;
        var stack = new StackHandle();
        stack.PushColorAbgr(ImGuiCol.Button,           a.Primary);
        stack.PushColorAbgr(ImGuiCol.ButtonHovered,    a.PrimaryLight);
        stack.PushColorAbgr(ImGuiCol.ButtonActive,     a.PrimaryDark);
        stack.PushColorAbgr(ImGuiCol.FrameBg,          a.FrameBg);
        stack.PushColorAbgr(ImGuiCol.FrameBgHovered,   a.SurfaceHover);
        stack.PushColorAbgr(ImGuiCol.FrameBgActive,    a.Surface);
        stack.PushColorAbgr(ImGuiCol.Border,           a.Border);
        stack.PushColorAbgr(ImGuiCol.Header,           a.Surface);
        stack.PushColorAbgr(ImGuiCol.HeaderHovered,    a.SurfaceHover);
        stack.PushColorAbgr(ImGuiCol.HeaderActive,     a.Identity);
        stack.PushColorAbgr(ImGuiCol.CheckMark,        a.Primary);
        stack.PushColorAbgr(ImGuiCol.SliderGrab,       a.Primary);
        stack.PushColorAbgr(ImGuiCol.SliderGrabActive, a.PrimaryLight);
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
        var a = theme.AbgrCache;
        var stack = new StackHandle();

        var alphaByte = (uint)Math.Clamp((int)(windowOpacity * 255f), 0x55, 0xFF);
        var windowBgWithAlpha = (c.WindowBg & 0xFFFFFF00u) | alphaByte;

        // ChildBg-Alpha: Sub-Bereiche (Tab-Sidebar, Message-Area, Input-Bar)
        // werden im ChatLog-Window als BeginChild gezeichnet. Würde der ChildBg
        // mit dem gleichen Alpha wie WindowBg gerendert, multiplizieren sich
        // die Layer (1 - (1-α)² Deckung), und 50 % WindowOpacity kommt mit
        // 75 % Deckung im Child-Bereich an — das Fenster wirkt solider als der
        // Slider verspricht. Bei voller Opacity bleibt der Theme-Akzent
        // erhalten (Theme-eigene Alpha-Komponente, i.d.R. FF); sobald der User
        // Transparenz zieht, wird ChildBg vollständig durchsichtig damit nur
        // der WindowBg-Layer die finale Deckung bestimmt.
        var childBgAlpha = windowOpacity >= 0.999f ? (c.ChildBg & 0xFFu) : 0u;
        var childBgWithAlpha = (c.ChildBg & 0xFFFFFF00u) | childBgAlpha;

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

        // Surfaces — WindowBg/ChildBg use the per-push opacity-modulated value,
        // so they go through the RGBA path; everything else reads from cache.
        stack.PushColor(ImGuiCol.WindowBg,         windowBgWithAlpha);
        stack.PushColor(ImGuiCol.ChildBg,          childBgWithAlpha);
        stack.PushColorAbgr(ImGuiCol.PopupBg,      a.ChildBg);
        stack.PushColorAbgr(ImGuiCol.Border,       a.Border);
        stack.PushColorAbgr(ImGuiCol.BorderShadow, 0u);

        // Frames
        stack.PushColorAbgr(ImGuiCol.FrameBg,        a.FrameBg);
        stack.PushColorAbgr(ImGuiCol.FrameBgHovered, a.SurfaceHover);
        stack.PushColorAbgr(ImGuiCol.FrameBgActive,  a.Surface);

        // Title bars
        stack.PushColorAbgr(ImGuiCol.TitleBg,          a.WindowBg);
        stack.PushColorAbgr(ImGuiCol.TitleBgActive,    a.Identity);
        stack.PushColorAbgr(ImGuiCol.TitleBgCollapsed, a.WindowBg);

        // Buttons
        stack.PushColorAbgr(ImGuiCol.Button,        a.Primary);
        stack.PushColorAbgr(ImGuiCol.ButtonHovered, a.PrimaryLight);
        stack.PushColorAbgr(ImGuiCol.ButtonActive,  a.PrimaryDark);

        // Headers / selectables
        stack.PushColorAbgr(ImGuiCol.Header,        a.Surface);
        stack.PushColorAbgr(ImGuiCol.HeaderHovered, a.SurfaceHover);
        stack.PushColorAbgr(ImGuiCol.HeaderActive,  a.Identity);

        // Tabs
        stack.PushColorAbgr(ImGuiCol.Tab,                a.FrameBg);
        stack.PushColorAbgr(ImGuiCol.TabHovered,         a.PrimaryLight);
        stack.PushColorAbgr(ImGuiCol.TabActive,          a.Identity);
        stack.PushColorAbgr(ImGuiCol.TabUnfocused,       a.ChildBg);
        stack.PushColorAbgr(ImGuiCol.TabUnfocusedActive, a.PrimaryDark);

        // Scrollbar
        stack.PushColorAbgr(ImGuiCol.ScrollbarBg,          a.WindowBg);
        stack.PushColorAbgr(ImGuiCol.ScrollbarGrab,        a.Surface);
        stack.PushColorAbgr(ImGuiCol.ScrollbarGrabHovered, a.AccentLight);
        stack.PushColorAbgr(ImGuiCol.ScrollbarGrabActive,  a.Accent);

        // Resize grip
        stack.PushColorAbgr(ImGuiCol.ResizeGrip,        a.FrameBg);
        stack.PushColorAbgr(ImGuiCol.ResizeGripHovered, a.AccentLight);
        stack.PushColorAbgr(ImGuiCol.ResizeGripActive,  a.Accent);

        // Check mark + slider grab
        stack.PushColorAbgr(ImGuiCol.CheckMark,         a.Primary);
        stack.PushColorAbgr(ImGuiCol.SliderGrab,        a.Primary);
        stack.PushColorAbgr(ImGuiCol.SliderGrabActive,  a.PrimaryLight);

        // Separator
        stack.PushColorAbgr(ImGuiCol.Separator,         a.Border);
        stack.PushColorAbgr(ImGuiCol.SeparatorHovered,  a.PrimaryLight);
        stack.PushColorAbgr(ImGuiCol.SeparatorActive,   a.Primary);

        return stack;
    }

    private sealed class StackHandle : IDisposable
    {
        private readonly List<IDisposable> _items = new(64);

        internal void PushColor(ImGuiCol slot, uint rgba)
            => _items.Add(ImRaii.PushColor(slot, ColourUtil.RgbaToAbgr(rgba)));

        internal void PushColorAbgr(ImGuiCol slot, uint abgr)
            => _items.Add(ImRaii.PushColor(slot, abgr));

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
