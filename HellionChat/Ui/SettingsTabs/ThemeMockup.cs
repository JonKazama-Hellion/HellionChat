using System.Numerics;
using Dalamud.Bindings.ImGui;
using HellionChat.Themes;
using HellionChat.Util;

namespace HellionChat.Ui.SettingsTabs;

internal static class ThemeMockup
{
    // Zeichnet ein Mini-Chat-Window-Mockup mit den Theme-Werten direkt
    // ins WindowDrawList. Keine Texture, keine Allocation pro Frame —
    // alles via DrawList.AddRectFilled / AddText.
    public static void Draw(Vector2 origin, Vector2 size, Theme theme)
    {
        var draw = ImGui.GetWindowDrawList();
        var c = theme.Colors;

        // Window-Bg
        draw.AddRectFilled(origin, origin + size, ColourUtil.RgbaToAbgr(c.WindowBg | 0xFFu), theme.Layout.WindowRounding);

        // Title-Bar
        var titleHeight = 14f;
        draw.AddRectFilled(
            origin,
            new Vector2(origin.X + size.X, origin.Y + titleHeight),
            ColourUtil.RgbaToAbgr(c.Identity), theme.Layout.WindowRounding);

        // Tab-Bar — 3 Mini-Tabs
        var tabY = origin.Y + titleHeight + 4f;
        var tabHeight = 12f;
        for (var i = 0; i < 3; i++)
        {
            var tabX = origin.X + 6f + i * 28f;
            var color = i == 0 ? c.FrameBg : c.ChildBg;
            draw.AddRectFilled(
                new Vector2(tabX, tabY),
                new Vector2(tabX + 26f, tabY + tabHeight),
                ColourUtil.RgbaToAbgr(color), theme.Layout.TabRounding);

            if (i == 0)  // Active-Pill
            {
                draw.AddRectFilled(
                    new Vector2(tabX, tabY + tabHeight - 2f),
                    new Vector2(tabX + 26f, tabY + tabHeight),
                    ColourUtil.RgbaToAbgr(c.Primary));
            }
        }

        // Card-Row mit Mock-Sender + Text
        var rowY = tabY + tabHeight + 6f;
        var rowHeight = 18f;
        draw.AddRectFilled(
            new Vector2(origin.X + 6f, rowY),
            new Vector2(origin.X + size.X - 6f, rowY + rowHeight),
            ColourUtil.RgbaToAbgr(c.Surface), 2f);

        // Akzent-Button rechts unten
        var btnW = 28f;
        var btnH = 10f;
        var btnX = origin.X + size.X - btnW - 6f;
        var btnY = origin.Y + size.Y - btnH - 6f;
        draw.AddRectFilled(
            new Vector2(btnX, btnY),
            new Vector2(btnX + btnW, btnY + btnH),
            ColourUtil.RgbaToAbgr(c.Accent), theme.Layout.FrameRounding);

        // Border um das gesamte Mockup
        draw.AddRect(origin, origin + size, ColourUtil.RgbaToAbgr(c.Border), theme.Layout.WindowRounding);
    }
}
