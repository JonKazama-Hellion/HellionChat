using HellionChat.Code;
using HellionChat.Resources;
using HellionChat.Util;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

internal sealed class FontsAndColours : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Card_FontsAndColours_Title + "###tabs-fontsandcolours";

    internal FontsAndColours(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        DrawFontsSection();
        ImGui.Spacing();
        DrawColoursSection();
    }

    private void DrawFontsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_FontsAndColours_Fonts_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            if (ImGui.Checkbox(HellionStrings.Theme_UseHellionFont_Name, ref Mutable.UseHellionFont))
            {
                if (Mutable.UseHellionFont)
                    Mutable.FontsEnabled = false;
            }
            ImGuiUtil.HelpMarker(HellionStrings.Theme_UseHellionFont_Description);
            ImGui.Spacing();

            if (Mutable.UseHellionFont)
            {
                ImGuiUtil.FontSizeCombo(Language.Options_FontSize_Name, ref Mutable.FontSizeV2);
                ImGui.Spacing();

                ImGuiUtil.FontSizeCombo(Language.Options_SymbolsFontSize_Name, ref Mutable.SymbolsFontSizeV2);
                ImGuiUtil.HelpMarker(Language.Options_SymbolsFontSize_Description);

                ImGui.Spacing();
                return;
            }

            ImGui.Checkbox(Language.Options_FontsEnabled, ref Mutable.FontsEnabled);
            ImGui.Spacing();

            var unused = false;
            if (!Mutable.FontsEnabled)
            {
                ImGuiUtil.FontSizeCombo(Language.Options_FontSize_Name, ref Mutable.FontSizeV2);
            }
            else
            {
                var globalChooser = ImGuiUtil.FontChooser(Language.Options_Font_Name, Mutable.GlobalFontV2, false, ref unused);
                globalChooser?.ResultTask.ContinueWith(r =>
                {
                    if (r.IsCompletedSuccessfully)
                    {
                        Plugin.Framework.Run(() => Mutable.GlobalFontV2 = r.Result);
                    }
                });
                ImGui.SameLine();
                if (ImGui.Button("Reset##global"))
                {
                    Mutable.GlobalFontV2 = new SingleFontSpec { FontId = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkRegular), SizePt = 12.75f };
                }

                ImGuiUtil.HelpMarker(string.Format(Language.Options_Font_Description, Plugin.PluginName));
                ImGuiUtil.WarningText(Language.Options_Font_Warning);
                ImGui.Spacing();

                var japaneseChooser = ImGuiUtil.FontChooser(Language.Options_JapaneseFont_Name, Mutable.JapaneseFontV2, false, ref unused, id => !id.LocaleNames?.ContainsKey("ja-jp") ?? false, "いろはにほへと   ちりぬるを");
                japaneseChooser?.ResultTask.ContinueWith(r =>
                {
                    if (r.IsCompletedSuccessfully)
                    {
                        Plugin.Framework.Run(() => Mutable.JapaneseFontV2 = r.Result);
                    }
                });
                ImGui.SameLine();
                if (ImGui.Button("Reset##japanese"))
                {
                    Mutable.JapaneseFontV2 = new SingleFontSpec { FontId = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkMedium), SizePt = 12.75f };
                }

                ImGuiUtil.HelpMarker(string.Format(Language.Options_JapaneseFont_Description, Plugin.PluginName));
                ImGui.Spacing();

                var italicChooser = ImGuiUtil.FontChooser(Language.Options_ItalicFont_Name, Mutable.ItalicFontV2, true, ref Mutable.ItalicEnabled);
                italicChooser?.ResultTask.ContinueWith(r =>
                {
                    if (r.IsCompletedSuccessfully)
                    {
                        Plugin.Framework.Run(() => Mutable.ItalicFontV2 = r.Result);
                    }
                });
                ImGui.SameLine();
                if (ImGui.Button("Reset##italic"))
                {
                    Mutable.ItalicEnabled = false;
                    Mutable.ItalicFontV2 = new SingleFontSpec { FontId = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkRegular), SizePt = 12.75f };
                }

                ImGuiUtil.HelpMarker(string.Format(Language.Options_Italic_Description, Plugin.PluginName));
                ImGui.Spacing();

                if (ImGui.CollapsingHeader(Language.Options_ExtraGlyphs_Name))
                {
                    ImGuiUtil.HelpMarker(string.Format(Language.Options_ExtraGlyphs_Description, Plugin.PluginName));

                    var range = (int)Mutable.ExtraGlyphRanges;
                    foreach (var extra in Enum.GetValues<ExtraGlyphRanges>())
                    {
                        ImGui.CheckboxFlags(extra.Name(), ref range, (int)extra);
                    }

                    Mutable.ExtraGlyphRanges = (ExtraGlyphRanges)range;
                }

                ImGui.Spacing();
            }

            ImGuiUtil.FontSizeCombo(Language.Options_SymbolsFontSize_Name, ref Mutable.SymbolsFontSizeV2);
            ImGuiUtil.HelpMarker(Language.Options_SymbolsFontSize_Description);

            ImGui.Spacing();
        }
    }

    private void DrawColoursSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_FontsAndColours_Colours_Heading);
        if (!tree.Success)
            return;

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            DrawColourPresetButtons();
            ImGui.TextDisabled(HellionStrings.Settings_Appearance_Colours_PresetsHint);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Checkbox(Language.Options_ColorSelectedInputChannelButton_Name, ref Mutable.ColorSelectedInputChannelButton);
            ImGuiUtil.HelpMarker(Language.Options_ColorSelectedInputChannelButton_Description);
            ImGui.Spacing();

            foreach (var (_, types) in ChatTypeExt.SortOrder)
            {
                foreach (var type in types)
                {
                    if (ImGuiUtil.IconButton(FontAwesomeIcon.UndoAlt, $"{type}", Language.Options_ChatColours_Reset))
                    {
                        Mutable.ChatColours.Remove(type);
                    }

                    ImGui.SameLine();

                    if (ImGuiUtil.IconButton(FontAwesomeIcon.LongArrowAltDown, $"{type}", Language.Options_ChatColours_Import))
                    {
                        var gameColour = Plugin.Functions.Chat.GetChannelColor(type);
                        Mutable.ChatColours[type] = gameColour ?? type.DefaultColor() ?? 0;
                    }

                    ImGui.SameLine();

                    var vec = Mutable.ChatColours.TryGetValue(type, out var colour)
                        ? ColourUtil.RgbaToVector3(colour)
                        : ColourUtil.RgbaToVector3(type.DefaultColor() ?? 0);
                    if (ImGui.ColorEdit3(type.Name(), ref vec, ImGuiColorEditFlags.NoInputs))
                    {
                        Mutable.ChatColours[type] = ColourUtil.Vector3ToRgba(vec);
                    }
                }
            }

            ImGui.Spacing();
        }
    }

    private void DrawColourPresetButtons()
    {
        var first = true;
        foreach (var (_, preset) in ChatColourPresets.All)
        {
            if (!first)
            {
                ImGui.SameLine();
            }
            first = false;

            if (preset.IsBrandPreset)
            {
                var border = ColourUtil.RgbaToVector3(ColourUtil.ComponentsToRgba(255, 128, 200));
                var btn = ColourUtil.RgbaToVector3(ColourUtil.ComponentsToRgba(74, 42, 106));
                ImGui.PushStyleColor(ImGuiCol.Border, new System.Numerics.Vector4(border.X, border.Y, border.Z, 1f));
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(btn.X, btn.Y, btn.Z, 1f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.5f);
            }

            if (ImGui.Button(GetPresetLabel(preset)))
            {
                ApplyPreset(preset);
            }

            if (preset.IsBrandPreset)
            {
                ImGui.PopStyleVar();
                ImGui.PopStyleColor(2);
            }
        }
    }

    private static string GetPresetLabel(ChatColourPreset preset)
    {
        var localized = HellionStrings.ResourceManager.GetString(preset.LocalizationKey, HellionStrings.Culture);
        return string.IsNullOrEmpty(localized) ? preset.DisplayName : localized;
    }

    private void ApplyPreset(ChatColourPreset preset)
    {
        foreach (var (channel, colour) in preset.Colours)
        {
            Mutable.ChatColours[channel] = colour;
        }
        Plugin.SaveConfig();
        GlobalParametersCache.Refresh();
        Plugin.Log.Debug($"Applied chat colour preset: {preset.DisplayName}");
    }
}
