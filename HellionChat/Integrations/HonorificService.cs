using Newtonsoft.Json;

namespace HellionChat.Integrations;

internal sealed class HonorificService
{
    // We pull Newtonsoft.Json into this single file for IPC compatibility:
    // Honorific serialises with Newtonsoft (see Honorific-master/IpcProvider.cs:9
    // and CustomTitle.cs:12). Using the same library guarantees identical
    // handling of System.Numerics.Vector3? and the enum fields we ignore.
    // Newtonsoft is a transitive dependency via Dalamud, so no new NuGet
    // entry is needed. The rest of HellionChat keeps using System.Text.Json.

    // Returns null when the JSON is empty (Honorific signals "no custom title"
    // with string.Empty — see IpcProvider.cs:100), or when deserialisation
    // throws (defensive: a malformed payload shouldn't crash the chat header).
    internal static HonorificTitleData? ParseTitleJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<HonorificTitleData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
