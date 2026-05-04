using HellionChat.Code;
using HellionChat.Resources;
using HellionChat.Util;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Ui.SettingsTabs;

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

            // The Bestand-Slider WindowAlpha targets the chat log window's
            // background only. The Hellion theme opacity above already covers
            // every plugin window globally, so the two sliders fight each
            // other when the theme is active. Disable the legacy slider in
            // that case to make Hellion theme the single source of truth.
            using (ImRaii.Disabled(Mutable.HellionThemeEnabled))
            {
                ImGuiUtil.DragFloatVertical(Language.Options_WindowOpacity_Name, ref Mutable.WindowAlpha, .25f, 0f, 100f, $"{Mutable.WindowAlpha:N2}%%", ImGuiSliderFlags.AlwaysClamp);
            }
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
            if (ImGui.Checkbox(HellionStrings.Theme_UseHellionFont_Name, ref Mutable.UseHellionFont))
            {
                // Mutex with the Bestand custom-font stack. Leaving FontsEnabled
                // checked alongside UseHellionFont made both checkboxes look
                // active even though the lower stack was greyed out, which
                // confused the user during the v0.5.0 walkthrough.
                if (Mutable.UseHellionFont)
                    Mutable.FontsEnabled = false;
            }
            ImGuiUtil.HelpMarker(HellionStrings.Theme_UseHellionFont_Description);
            ImGui.Spacing();

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

                // LocaleNames being null means it is likely a game font which all support JP symbols.
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
        using var tree = ImRaii.TreeNode(HellionStrings.Settings_Appearance_Colours_Heading);
        if (!tree.Success)
        {
            return;
        }

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

    // Hellion Chat — v0.6.0 preset-buttons row above the per-channel colour
    // editors. Apply is immediate and overwrites every channel covered by
    // the preset; channels not in the preset keep their current colour.
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
                // Hellion-Brand visuell hervorheben — blau-violetter Frame-Akzent
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

    // Localized label for a preset; falls back to DisplayName if the i18n
    // key is missing (defensive — the resource manager returns the key
    // string itself when a lookup fails).
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
