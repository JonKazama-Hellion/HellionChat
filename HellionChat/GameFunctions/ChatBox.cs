using System.Text;
using HellionChat.Resources;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace HellionChat.GameFunctions;

public unsafe class ChatBox
{
    public static void SendMessageUnsafe(byte[] message)
    {
        var mes = Utf8String.FromSequence(message.NullTerminate());
        UIModule.Instance()->ProcessChatBoxEntry(mes);
        mes->Dtor(true);
    }

    public static void SendMessage(string message)
    {
        var bytes = ValidateMessage(message);
        SendMessageUnsafe(bytes);
    }

    // Validation split out so the deterministic checks (UTF-8 length, sanitise
    // round-trip) can run in xUnit without ClientStructs game memory. The
    // sanitiser is injectable so tests can pin throw behaviour without invoking
    // Utf8String->SanitizeString, which only resolves in-process. Returns the
    // already-encoded bytes so SendMessage doesn't pay GetBytes twice.
    // TEST-MIRROR: ../../../Hellion Build test/GameFunctions/ChatBoxTests.cs
    internal static byte[] ValidateMessage(string message, Func<string, string>? sanitiserOverride = null)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
            throw new ArgumentException(Language.ChatBox_Error_Empty, nameof(message));

        if (bytes.Length > 500)
            throw new ArgumentException(Language.ChatBox_Error_Too_Long, nameof(message));

        var sanitiser = sanitiserOverride ?? SanitiseText;
        if (message.Length != sanitiser(message).Length)
            throw new ArgumentException(Language.ChatBox_Error_Invalid, nameof(message));

        return bytes;
    }

    private static string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);

        uText->SanitizeString((AllowedEntities) 0x27F);
        var sanitised = uText->ToString();
        uText->Dtor(true);

        return sanitised;
    }
}