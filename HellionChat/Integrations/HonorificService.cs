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

    // Honorific has been on major version 3 since the IPC was introduced.
    // We treat anything else as incompatible because a major bump from
    // upstream signals a breaking IPC contract change, and rendering a
    // title against the wrong shape is worse than rendering nothing.
    // If Honorific later ships a non-breaking 4.x major, we relax this
    // by extending the accepted-major set rather than removing the check.
    internal static bool IsApiVersionCompatible((uint Major, uint Minor) apiVersion)
    {
        return apiVersion.Major == 3;
    }

    // Single source of truth for whether the chat header should draw the
    // Honorific slot in the current frame. Returning a single bool keeps
    // the render call branch-free; all skip conditions are evaluated here.
    // The IsOriginal short-circuit means: when the user has Honorific
    // installed but is using the original FFXIV title, we render nothing —
    // matches the design decision in the spec ("Empty-State A: silent
    // auto-hide").
    internal static bool ShouldRenderSlot(
        bool toggleEnabled,
        bool isAvailable,
        HonorificTitleData? title)
    {
        if (!toggleEnabled) return false;
        if (!isAvailable) return false;
        if (title is null) return false;
        if (title.IsOriginal) return false;
        if (string.IsNullOrEmpty(title.Title)) return false;
        return true;
    }
}
