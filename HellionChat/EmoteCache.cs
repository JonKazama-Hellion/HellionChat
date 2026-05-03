using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HellionChat;

public static class EmoteCache
{
    private static readonly string[] NotWorking =
    [
        ":tf:", "(ditto)", "c!", "h!", "l!", "M&Mjc", "LUL3D", "p!",
        "POLICE2", "r!", "Pussy", "s!", "v!", "w!", "x0r6ztGiggle",
        "z!", "xar2EDM", "iron95Pls", "Clap2", "AlienPls3", "Life",
        "peepoPogClimbingTreeHard4House", "monkaGIGAftRobertDowneyJr",
        "DogLookingSussyAndCold", "DICKS"
    ];

    private static readonly HttpClient Client = new();

    private const string BetterTTV = "https://api.betterttv.net/3";
    private const string GlobalEmotes = $"{BetterTTV}/cached/emotes/global";
    private const string Top100Emotes = "{0}/emotes/shared/top?before={1}&limit=100";
    private const string EmotePath = "https://cdn.betterttv.net/emote/{0}/3x";

    [Serializable]
    private struct Top100()
    {
        [JsonPropertyName("emote")]
        public Emote Emote { get; set; }

        [JsonPropertyName("id")]
        public required string Id { get; set; }
    }

    [Serializable]
    public struct Emote()
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("code")]
        public required string Code { get; set; }

        [JsonPropertyName("imageType")]
        public required string ImageType { get; set; }
    }

    public enum LoadingState
    {
        Unloaded,
        Loading,
        Done
    }

    // All of this data is uninitalized while State is not `LoadingState.Done`
    public static LoadingState State = LoadingState.Unloaded;

    private static readonly Dictionary<string, Emote> Cache = new();
    private static readonly Dictionary<string, EmoteBase> EmoteImages = new();

    public static string[] SortedCodeArray = [];

    public static async Task LoadData()
    {
        if (State is not LoadingState.Unloaded)
            return;

        State = LoadingState.Loading;
        try
        {
            var global = await Client.GetAsync(GlobalEmotes);
            var globalList = await global.Content.ReadAsStringAsync();

            foreach (var emote in JsonSerializer.Deserialize<Emote[]>(globalList)!)
                if (!string.IsNullOrEmpty(emote.Code) && !NotWorking.Contains(emote.Code))
                    Cache.TryAdd(emote.Code, emote);

            var lastId = string.Empty;
            for (var i = 0; i < 15; i++)
            {
                var top = await Client.GetAsync(Top100Emotes.Format(BetterTTV, lastId));
                var topList = await top.Content.ReadAsStringAsync();

                var jsonList = JsonSerializer.Deserialize<List<Top100>>(topList)!;
                // BetterTTV occasionally returns entries with a null Code; the
                // upstream code passed those straight into Dictionary.TryAdd
                // and tripped ArgumentNullException, killing the whole emote
                // load. Skip them defensively so a single bad row no longer
                // breaks the cache for everyone else.
                foreach (var emote in jsonList)
                    if (!string.IsNullOrEmpty(emote.Emote.Code) && !NotWorking.Contains(emote.Emote.Code))
                        Cache.TryAdd(emote.Emote.Code, emote.Emote);

                lastId = jsonList.Last().Id;
            }

            SortedCodeArray = Cache.Keys.Order().ToArray();
            State = LoadingState.Done;
        }
        catch (Exception ex)
        {
            // Reset to Unloaded so a later trigger (e.g. the user reopening
            // the Emotes tab after the network recovers) can retry. Without
            // this the State stays on Loading and the early-out at the top
            // of LoadData blocks every further attempt until plugin reload.
            State = LoadingState.Unloaded;
            Plugin.Log.Error(ex, "BetterTTV cache wasn't initialized");
        }
    }

    public static void Dispose()
    {
        foreach (var emote in EmoteImages.Values)
            emote.InnerDispose();
    }

    internal static bool Exists(string code)
    {
        return State is LoadingState.Done && SortedCodeArray.Contains(code);
    }

    internal static EmoteBase? GetEmote(string code)
    {
        if (State is not LoadingState.Done)
            return null;

        if (!Cache.TryGetValue(code, out var emoteDetail))
            return null;

        if (EmoteImages.TryGetValue(emoteDetail.Id, out var emote))
            return emote;

        try
        {
            if (emoteDetail.ImageType == "gif")
            {
                var animatedEmote = new ImGuiGif().Prepare(emoteDetail);
                EmoteImages.Add(emoteDetail.Id, animatedEmote);
                return animatedEmote;
            }

            var staticEmote = new ImGuiEmote().Prepare(emoteDetail);
            EmoteImages.Add(emoteDetail.Id, staticEmote);

            return staticEmote;
        }
        catch
        {
            Plugin.Log.Error("Failed to convert");
            return null;
        }
    }

    public abstract class EmoteBase
    {
        public bool Failed;
        public bool IsLoaded;

        public byte[] RawData = [];

        protected IDalamudTextureWrap? Texture;

        public virtual void Draw(Vector2 size)
        {
            ImGui.Image(Texture!.Handle, size);
        }

        internal async Task<byte[]> LoadAsync(Emote emote)
        {
            // BetterTTV-supplied Id and ImageType are interpolated straight
            // into the filename. HTTPS protects the wire, but a compromised
            // upstream could still hand us "../foo" and write into the
            // pluginConfigs root (or worse). Resolve the candidate path and
            // refuse anything that escapes the cache directory.
            var dir = Path.GetFullPath(Path.Join(Plugin.Interface.ConfigDirectory.FullName, "EmoteCacheV1"));
            Directory.CreateDirectory(dir);

            var dirPrefix = dir.EndsWith(Path.DirectorySeparatorChar) ? dir : dir + Path.DirectorySeparatorChar;
            var filePath = Path.GetFullPath(Path.Join(dir, $"{emote.Id}.{emote.ImageType}"));
            if (!filePath.StartsWith(dirPrefix, StringComparison.Ordinal))
                throw new InvalidOperationException($"Emote path escapes cache directory: id={emote.Id}, type={emote.ImageType}");

            if (File.Exists(filePath))
            {
                RawData = await File.ReadAllBytesAsync(filePath);
            }
            else
            {
                var content = await new HttpClient().GetAsync(EmotePath.Format(emote.Id));
                RawData = await content.Content.ReadAsByteArrayAsync();

                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.Write(RawData, 0, RawData.Length);
            }

            return RawData;
        }

        public abstract void InnerDispose();
    }

    public sealed class ImGuiEmote : EmoteBase
    {
        public ImGuiEmote Prepare(Emote emote)
        {
            Task.Run(() => Load(emote));
            return this;
        }

        private async void Load(Emote emote)
        {
            try
            {
                var image = await LoadAsync(emote);
                if (image.Length <= 0)
                    return;

                Texture = await Plugin.TextureProvider.CreateFromImageAsync(image);
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Failed = true;
                Plugin.Log.Error(ex, $"Unable to load {emote.Code} with id {emote.Id}");
            }
        }

        public override void InnerDispose()
        {
            Texture?.Dispose();
        }
    }

    public sealed class ImGuiGif : EmoteBase
    {
        private List<(IDalamudTextureWrap Texture, float Delay)> Frames = [];
        private float FrameTimer;
        private int CurrentFrame;
        private ulong GlobalFrameCount;

        public override void Draw(Vector2 size)
        {
            if (Frames.Count == 0)
                return;

            if (CurrentFrame >= Frames.Count)
            {
                CurrentFrame = 0;
                FrameTimer = -1f;
            }

            var frame = Frames[CurrentFrame];
            if (FrameTimer <= 0.0f)
                FrameTimer = frame.Delay;

            ImGui.Image(frame.Texture.Handle, size);

            if (GlobalFrameCount == Plugin.Interface.UiBuilder.FrameCount)
                return;

            GlobalFrameCount = Plugin.Interface.UiBuilder.FrameCount;

            FrameTimer -= ImGui.GetIO().DeltaTime;
            if (FrameTimer <= 0f)
                CurrentFrame++;
        }

        public override void InnerDispose()
        {
            Frames.ForEach(f => f.Texture.Dispose());
            Frames.Clear();
        }

        public ImGuiGif Prepare(Emote emote)
        {
            Task.Run(() => Load(emote));
            return this;
        }

        private async void Load(Emote emote)
        {
            try
            {
                var image = await LoadAsync(emote);
                if (image.Length <= 0)
                    return;

                using var ms = new MemoryStream(image);
                using var img = Image.Load<Rgba32>(ms);
                if (img.Frames.Count == 0)
                    return;

                var frames = new List<(IDalamudTextureWrap Tex, float Delay)>();
                foreach (var frame in img.Frames)
                {
                    var delay = frame.Metadata.GetGifMetadata().FrameDelay / 100f;

                    // Follows the same pattern as browsers, anything under 0.02s delay will be rounded up to 0.1s
                    if (delay < 0.02f)
                        delay = 0.1f;

                    var buffer = new byte[4 * frame.Width * frame.Height];
                    frame.CopyPixelDataTo(buffer);
                    var tex = await Plugin.TextureProvider.CreateFromRawAsync(RawImageSpecification.Rgba32(frame.Width, frame.Height), buffer);
                    frames.Add((tex, delay));
                }

                Frames = frames;
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Failed = true;
                Plugin.Log.Error(ex, $"Unable to load {emote.Code} with id {emote.Id}");
            }
        }
    }
}