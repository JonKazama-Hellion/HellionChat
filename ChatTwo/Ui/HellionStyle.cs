using ChatTwo.Util;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ChatTwo.Ui;

/// <summary>
/// Local ImGui style override applied inside Hellion-owned settings surfaces
/// (Privacy tab, first-run wizard). Industrial HUD palette: deep slate
/// background with cyan-teal accents and sharper geometric framing. Scoped
/// via using-blocks so upstream Chat 2 tabs render in their original style.
/// </summary>
internal static class HellionStyle
{
    // Palette — kept central so a future theme switch only edits one file.
    // Encoded as 0xRRGGBBAA, matching the convention ChatTwo uses elsewhere
    // (see Settings.cs Ko-fi buttons). RgbaToAbgr handles the byte swap.
    private const uint AccentRgba = 0x00B8D4FF;       // cyan-teal accent
    private const uint AccentHoverRgba = 0x26C6DAFF;
    private const uint AccentActiveRgba = 0x00838FFF;
    private const uint FrameBgRgba = 0x102027FF;      // deep slate
    private const uint FrameBgHoverRgba = 0x1B2C36FF;
    private const uint FrameBgActiveRgba = 0x263A45FF;
    private const uint BorderRgba = 0x37474FFF;       // steel border
    private const uint HeaderRgba = 0x1B2C36FF;
    private const uint HeaderHoverRgba = 0x263A45FF;
    private const uint HeaderActiveRgba = 0x324A57FF;
    private const uint TitleBgActiveRgba = 0x00838FFF;
    private const uint CheckMarkRgba = 0x00B8D4FF;
    private const uint SliderGrabRgba = 0x00B8D4FF;
    private const uint SliderGrabActiveRgba = 0x26C6DAFF;

    /// <summary>
    /// Push the Hellion color stack. Returns a disposable bundle that pops
    /// every color in reverse order on Dispose. Use inside a `using` block.
    /// </summary>
    internal static IDisposable Push()
    {
        var stack = new StackHandle();
        // Order matters less than count: each PushColor needs a matching pop.
        stack.Add(ImRaii.PushColor(ImGuiCol.Button, ColourUtil.RgbaToAbgr(AccentRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.ButtonHovered, ColourUtil.RgbaToAbgr(AccentHoverRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.ButtonActive, ColourUtil.RgbaToAbgr(AccentActiveRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.FrameBg, ColourUtil.RgbaToAbgr(FrameBgRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.FrameBgHovered, ColourUtil.RgbaToAbgr(FrameBgHoverRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.FrameBgActive, ColourUtil.RgbaToAbgr(FrameBgActiveRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.Border, ColourUtil.RgbaToAbgr(BorderRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.Header, ColourUtil.RgbaToAbgr(HeaderRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.HeaderHovered, ColourUtil.RgbaToAbgr(HeaderHoverRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.HeaderActive, ColourUtil.RgbaToAbgr(HeaderActiveRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.TitleBgActive, ColourUtil.RgbaToAbgr(TitleBgActiveRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.CheckMark, ColourUtil.RgbaToAbgr(CheckMarkRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.SliderGrab, ColourUtil.RgbaToAbgr(SliderGrabRgba)));
        stack.Add(ImRaii.PushColor(ImGuiCol.SliderGrabActive, ColourUtil.RgbaToAbgr(SliderGrabActiveRgba)));
        return stack;
    }

    private sealed class StackHandle : IDisposable
    {
        private readonly List<IDisposable> _items = new(16);

        internal void Add(IDisposable d) => _items.Add(d);

        public void Dispose()
        {
            // Pop in reverse order so the ImGui stack unwinds cleanly.
            for (var i = _items.Count - 1; i >= 0; i--)
                _items[i].Dispose();
            _items.Clear();
        }
    }
}
