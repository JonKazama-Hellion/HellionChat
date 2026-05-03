using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace HellionChat;

public class FontManager
{
    internal IFontHandle Axis = null!;
    internal IFontHandle AxisItalic = null!;

    internal IFontHandle RegularFont = null!;
    internal IFontHandle? ItalicFont;

    internal IFontHandle FontAwesome = null!;

    internal readonly byte[] GameSymFont;

    private ushort[] Ranges = [];
    private ushort[] JpRange = [];

    public static readonly HashSet<float> AxisFontSizeList =
    [
        9.6f, 10f, 12f, 14f, 16f,
        18f, 18.4f, 20f, 23f, 34f,
        36f, 40f, 45f, 46f, 68f, 90f,
    ];

    public FontManager()
    {
        var filePath = Path.Combine(Plugin.Interface.ConfigDirectory.FullName, "FFXIV_Lodestone_SSF.ttf");
        if (File.Exists(filePath))
        {
            GameSymFont = File.ReadAllBytes(filePath);
        }
        else
        {
            // Dispose HttpClient and HttpResponseMessage to avoid socket
            // exhaustion on repeated cold-start downloads. GetAwaiter().GetResult()
            // unwraps AggregateException so failures surface cleanly. A full
            // async refactor of the constructor would be cleaner but is out of
            // scope for v1.0.0 — tracked in the backlog.
            using var client = new HttpClient();
            using var response = client
                .GetAsync("https://img.finalfantasyxiv.com/lds/pc/global/fonts/FFXIV_Lodestone_SSF.ttf")
                .GetAwaiter()
                .GetResult();
            response.EnsureSuccessStatusCode();
            GameSymFont = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

            Dalamud.Utility.FilesystemUtil.WriteAllBytesSafe(filePath, GameSymFont);
        }
    }

    /// <summary>
    /// Backing bytes for the bundled Hellion font (Exo 2, OFL-1.1). Lazily
    /// extracted from the assembly's manifest resources on first use; the
    /// load happens inside the font atlas build callback so we keep the
    /// allocation off the plugin constructor's hot path.
    /// </summary>
    private static byte[]? HellionFontBytes;

    private static byte[] GetHellionFontBytes()
    {
        if (HellionFontBytes is not null)
            return HellionFontBytes;

        using var stream = typeof(FontManager).Assembly.GetManifestResourceStream("HellionFont.ttf")
            ?? throw new FileNotFoundException("Hellion font resource not embedded in the assembly");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        HellionFontBytes = ms.ToArray();
        return HellionFontBytes;
    }

    private unsafe void SetUpRanges()
    {
        ushort[] BuildRange(IReadOnlyList<ushort>? chars, params nint[] ranges)
        {
            var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder());
            // text
            foreach (var range in ranges)
                builder.AddRanges((ushort*)range);

            // chars
            if (chars != null)
            {
                for (var i = 0; i < chars.Count; i += 2)
                {
                    if (chars[i] == 0)
                        break;

                    for (var j = (uint) chars[i]; j <= chars[i + 1]; j++)
                        builder.AddChar((ushort) j);
                }
            }

            // Ingame supported ranges
            var reader = new FdtReader(Plugin.DataManager.GetFile("common/font/axis_12.fdt")!.Data);
            foreach (var c in reader.Glyphs)
                builder.AddChar(c.Char);

            // various symbols
            // French
            // Romanian
            // builder.AddText("←→↑↓《》■※☀★★☆♥♡ヅツッシ☀☁☂℃℉°♀♂♠♣♦♣♧®©™€$£♯♭♪✓√◎◆◇♦■□〇●△▽▼▲‹›≤≥<«“”─＼～");
            builder.AddText("Œœ");
            builder.AddText("ĂăÂâÎîȘșȚț");

            // "Enclosed Alphanumerics" (partial) https://www.compart.com/en/unicode/block/U+2460
            for (var i = 0x2460; i <= 0x24B5; i++)
                builder.AddChar((char) i);

            builder.AddChar('⓪');
            return builder.BuildRangesToArray();
        }

        var ranges = new List<nint> { (nint)ImGui.GetIO().Fonts.GetGlyphRangesDefault() };
        foreach (var extraRange in Enum.GetValues<ExtraGlyphRanges>())
            if (Plugin.Config.ExtraGlyphRanges.HasFlag(extraRange))
                ranges.Add(extraRange.Range());

        Ranges = BuildRange(null, ranges.ToArray());
        JpRange = BuildRange(GlyphRangesJapanese.GlyphRanges);
    }

    public void BuildFonts()
    {
        SetUpRanges();

        Axis = Plugin.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.Axis, SizeInPx(Plugin.Config.FontSizeV2)));
        AxisItalic = Plugin.Interface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.Axis, SizeInPx(Plugin.Config.FontSizeV2))
        {
            SkewStrength = SizeInPx(Plugin.Config.FontSizeV2) / 6
        });

        FontAwesome = Plugin.Interface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
        {
            e.OnPreBuild(tk => tk.AddFontAwesomeIconFont(new SafeFontConfig { SizePx = GetFontSize() }));
            e.OnPostBuild(tk => tk.FitRatio(tk.Font));
        });

        RegularFont = Plugin.Interface.UiBuilder.FontAtlas.NewDelegateFontHandle(
            e => e.OnPreBuild(
                tk =>
                {
                    var config = new SafeFontConfig {SizePt = Plugin.Config.GlobalFontV2.SizePt, GlyphRanges = Ranges};
                    config.MergeFont = Plugin.Config.UseHellionFont
                        ? tk.AddFontFromMemory(GetHellionFontBytes(), config, "Hellion-Exo2")
                        : AddFontWithFallback(tk, Plugin.Config.GlobalFontV2.FontId, config, "global");

                    config.SizePt = Plugin.Config.JapaneseFontV2.SizePt;
                    config.GlyphRanges = JpRange;
                    AddFontWithFallback(tk, Plugin.Config.JapaneseFontV2.FontId, config, "japanese");

                    config.SizePt = Plugin.Config.SymbolsFontSizeV2;
                    tk.AddGameSymbol(config);

                    tk.Font = config.MergeFont;
                }
            ));

        if (Plugin.Config.ItalicEnabled)
        {
            ItalicFont = Plugin.Interface.UiBuilder.FontAtlas.NewDelegateFontHandle(
                e => e.OnPreBuild(
                    tk =>
                    {
                        var config = new SafeFontConfig {SizePt = Plugin.Config.ItalicFontV2.SizePt, GlyphRanges = Ranges};
                        config.MergeFont = AddFontWithFallback(tk, Plugin.Config.ItalicFontV2.FontId, config, "italic");

                        config.SizePt = Plugin.Config.JapaneseFontV2.SizePt;
                        config.GlyphRanges = JpRange;
                        AddFontWithFallback(tk, Plugin.Config.JapaneseFontV2.FontId, config, "japanese");

                        config.SizePt = Plugin.Config.SymbolsFontSizeV2;
                        tk.AddGameSymbol(config);

                        tk.Font = config.MergeFont;
                    }
                ));
        }
        else
        {
            ItalicFont = null;
        }
    }

    /// <summary>
    /// Try to add a user-configured font to the build toolkit, falling back to
    /// the bundled NotoSansCjkRegular asset if the configured font isn't
    /// available on the system. Without this guard a stale SystemFontId
    /// pointing at a font the user uninstalled or that never existed on
    /// Linux (e.g. "Crimson Text") tears down the entire font atlas build.
    /// </summary>
    private static ImFontPtr AddFontWithFallback(IFontAtlasBuildToolkitPreBuild tk, IFontId fontId, SafeFontConfig config, string slot)
    {
        try
        {
            return fontId.AddToBuildToolkit(tk, config);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException or IOException)
        {
            Plugin.Log.Warning(e, $"Configured {slot} font unavailable, falling back to NotoSansCjkRegular");
            var fallback = new DalamudAssetFontAndFamilyId(DalamudAsset.NotoSansCjkRegular);
            return fallback.AddToBuildToolkit(tk, config);
        }
    }

    public static float SizeInPt(float px) => (float) (px * 3.0 / 4.0);
    public static float SizeInPx(float pt) => (float) (pt * 4.0 / 3.0);
    public static float GetFontSize() => Plugin.Config.FontsEnabled ? Plugin.Config.GlobalFontV2.SizePx : SizeInPx(Plugin.Config.FontSizeV2);
}
