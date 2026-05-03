using System.Linq;
using ChatTwo.Resources;
using Dalamud.Plugin;

namespace ChatTwo;

internal static class ChatTwoConflictDetector
{
    private const string UpstreamInternalName = "ChatTwo";

    public static void ThrowIfChatTwoIsLoaded(IDalamudPluginInterface pluginInterface)
    {
        var conflict = pluginInterface.InstalledPlugins
            .FirstOrDefault(p =>
                p.InternalName == UpstreamInternalName &&
                p.IsLoaded);

        if (conflict is null)
            return;

        var message = HellionStrings.ChatTwoConflictTitle + "\n\n" +
                      HellionStrings.ChatTwoConflictBody + "\n\n" +
                      HellionStrings.ChatTwoConflictAction;

        throw new System.InvalidOperationException(message);
    }
}
