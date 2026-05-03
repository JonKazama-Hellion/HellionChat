using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

namespace ChatTwo;

internal sealed class IpcManager : IDisposable
{
    private ICallGateProvider<string> RegisterGate { get; }
    private ICallGateProvider<string, object?> UnregisterGate { get; }
    private ICallGateProvider<object?> AvailableGate { get; }
    private ICallGateProvider<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?> InvokeGate { get; }

    internal List<string> Registered { get; } = [];

    public IpcManager()
    {
        RegisterGate = Plugin.Interface.GetIpcProvider<string>("HellionChat.Register");
        RegisterGate.RegisterFunc(Register);

        AvailableGate = Plugin.Interface.GetIpcProvider<object?>("HellionChat.Available");

        UnregisterGate = Plugin.Interface.GetIpcProvider<string, object?>("HellionChat.Unregister");
        UnregisterGate.RegisterAction(Unregister);

        InvokeGate = Plugin.Interface.GetIpcProvider<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?>("HellionChat.Invoke");

        AvailableGate.SendMessage();
    }

    internal void Invoke(string id, PlayerPayload? sender, ulong contentId, Payload? payload, SeString? senderString, SeString? content)
    {
        InvokeGate.SendMessage(id, sender, contentId, payload, senderString, content);
    }

    private string Register()
    {
        var id = Guid.NewGuid().ToString();
        Registered.Add(id);
        return id;
    }

    private void Unregister(string id)
    {
        Registered.Remove(id);
    }

    public void Dispose()
    {
        UnregisterGate.UnregisterFunc();
        RegisterGate.UnregisterFunc();
        Registered.Clear();
    }
}
