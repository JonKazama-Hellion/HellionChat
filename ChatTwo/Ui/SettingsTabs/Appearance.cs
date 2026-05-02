using ChatTwo.Code;
using ChatTwo.Resources;
using ChatTwo.Util;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace ChatTwo.Ui.SettingsTabs;

internal sealed class Appearance : ISettingsTab
{
    private Plugin Plugin { get; }
    private Configuration Mutable { get; }

    public string Name => HellionStrings.Settings_Tab_Appearance + "###tabs-appearance";

    internal Appearance(Plugin plugin, Configuration mutable)
    {
        Plugin = plugin;
        Mutable = mutable;
    }

    public void Draw(bool changed)
    {
        DrawThemeSection();
        ImGui.Spacing();
        DrawFontsSection();
        ImGui.Spacing();
        DrawColoursSection();
        ImGui.Spacing();
        DrawTimestampsSection();
    }

    private void DrawThemeSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Appearance_Theme_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(HellionStrings.Theme_Enabled_Name, ref Mutable.HellionThemeEnabled);
            ImGuiUtil.HelpMarker(HellionStrings.Theme_Enabled_Description);

            // Clamp 0.5–1.0 stays consistent with Privacy.cs which already
            // shipped this slider; lower values would let chat windows
            // disappear behind game UI.
            using (ImRaii.Disabled(!Mutable.HellionThemeEnabled))
            {
                ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
                var opacity = Mutable.HellionThemeWindowOpacity;
                if (ImGui.SliderFloat($"{HellionStrings.Theme_WindowOpacity_Label}##theme-opacity", ref opacity, 0.5f, 1.0f, "%.2f"))
                {
                    Mutable.HellionThemeWindowOpacity = Math.Clamp(opacity, 0.5f, 1.0f);
                }
                ImGuiUtil.HelpMarker(HellionStrings.Theme_WindowOpacity_Help);
            }

            ImGui.Spacing();

            ImGui.Checkbox(Language.Options_OverrideStyle_Name, ref Mutable.OverrideStyle);
            ImGuiUtil.HelpMarker(Language.Options_OverrideStyle_Name_Desc);

            if (Mutable.OverrideStyle)
            {
                DrawStyleCombo();
            }

            ImGuiUtil.DragFloatVertical(Language.Options_WindowOpacity_Name, ref Mutable.WindowAlpha, .25f, 0f, 100f, $"{Mutable.WindowAlpha:N2}%%", ImGuiSliderFlags.AlwaysClamp);
        }
    }

    private void DrawStyleCombo()
    {
        var styles = StyleModel.GetConfiguredStyles();
        if (styles == null)
        {
            ImGui.TextUnformatted(Language.Options_OverrideStyle_NotAvailable);
            return;
        }

        var currentStyle = Mutable.ChosenStyle ?? Language.Options_OverrideStyle_NotSelected;
        using var combo = ImRaii.Combo(Language.Options_OverrideStyleDropdown_Name, currentStyle);
        if (!combo)
        {
            return;
        }

        foreach (var style in styles)
        {
            if (ImGui.Selectable(style.Name, Mutable.ChosenStyle == style.Name))
            {
                Mutable.ChosenStyle = style.Name;
            }
        }
    }

    private void DrawFontsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Appearance_Fonts_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(HellionStrings.Theme_UseHellionFont_Name, ref Mutable.UseHellionFont);
            ImGuiUtil.HelpMarker(HellionStrings.Theme_UseHellionFont_Description);
            ImGui.Spacing();

            // Hellion-Font und der Custom-Font-Stack schließen sich aus:
            // wenn Exo 2 erzwungen wird, sind die ChatTwo-Font-Picker
            // ohne Wirkung, also UI-seitig ausgrauen statt versteckt
            // weiterzuwerken.
            using var fontDisabled = ImRaii.Disabled(Mutable.UseHellionFont);

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
                        Mutable.GlobalFontV2 = r.Result;
                });
                ImGui.SameLine();
                if (ImGui.Button("Reset##global"))
                {
                    Mutable.GlobalFontV2 = new SingleFontSpec { FontId = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkRegular), SizePt = 12.75f };
                }

                ImGuiUtil.HelpMarker(string.Format(Language.Options_Font_Description, Plugin.PluginName));
                ImGuiUtil.WarningText(Language.Options_Font_Warning);
                ImGui.Spacing();

                // LocaleNames being null means it is likely a game font which all support JP symbols.
                var japaneseChooser = ImGuiUtil.FontChooser(Language.Options_JapaneseFont_Name, Mutable.JapaneseFontV2, false, ref unused, id => !id.LocaleNames?.ContainsKey("ja-jp") ?? false, "いろはにほへと   ちりぬるを");
                japaneseChooser?.ResultTask.ContinueWith(r =>
                {
                    if (r.IsCompletedSuccessfully)
                        Mutable.JapaneseFontV2 = r.Result;
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
                        Mutable.ItalicFontV2 = r.Result;
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
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Appearance_Colours_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
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

    private void DrawTimestampsSection()
    {
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Appearance_Timestamps_Heading);
        if (!tree.Success)
        {
            return;
        }

        using (ImRaii.PushIndent(ImGui.GetStyle().IndentSpacing, false))
        {
            ImGui.Checkbox(Language.Options_PrettierTimestamps_Name, ref Mutable.PrettierTimestamps);
            ImGuiUtil.HelpMarker(Language.Options_PrettierTimestamps_Description);

            if (Mutable.PrettierTimestamps)
            {
                ImGui.Checkbox(Language.Options_MoreCompactPretty_Name, ref Mutable.MoreCompactPretty);
                ImGuiUtil.HelpMarker(Language.Options_MoreCompactPretty_Description);

                ImGui.Checkbox(Language.Options_HideSameTimestamps_Name, ref Mutable.HideSameTimestamps);
                ImGuiUtil.HelpMarker(Language.Options_HideSameTimestamps_Description);
            }

            ImGui.Checkbox(Language.Options_Use24HourClock_Name, ref Mutable.Use24HourClock);
            ImGuiUtil.HelpMarker(Language.Options_Use24HourClock_Description);
        }
    }
}
