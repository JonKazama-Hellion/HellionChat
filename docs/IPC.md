# Hellion Chat IPC Integration Guide

This document describes the inter-plugin-communication (IPC) channels that
Hellion Chat exposes to other Dalamud plugins. Two integration surfaces are
covered: the **Context Menu IPC** for adding custom items to Hellion Chat's
right-click menus, and the **Typing State IPC** for reacting to the user's
input-box activity.

---

## Compatibility with Chat 2

Hellion Chat is a standalone fork of [Chat 2](https://github.com/Infiziert90/ChatTwo)
(EUPL-1.2). The IPC surface is one of the parts the fork inherits directly:
the same call shapes, the same tuple payloads, the same call semantics, the
same lifecycle. We did not redesign the API, we re-published it under our own
plugin name.

Concretely, this means:

- **Tuple shapes are identical.** A subscriber that worked against Chat 2's
  `ChatTwo.Invoke` works against Hellion Chat's `HellionChat.Invoke` without
  any code change beyond the channel string.
- **Lifecycle is identical.** The `Available` ping fires when the plugin
  becomes ready, your subscriber re-registers, and the registration ID is
  returned by the same `Register` call as before.
- **Channel-name prefix changed in v1.0.0.** Every `ChatTwo.*` channel name
  is now `HellionChat.*`. Existing third-party integrations need a one-line
  rename per channel string and nothing else.

If your plugin already supports Chat 2 and you want to add Hellion Chat
support, the cleanest path is to bind both prefixes and treat whichever one
becomes available first as the active host.

---

## Channel Reference

| Surface       | Channel                              | Direction       | Payload                                                                                  |
| ------------- | ------------------------------------ | --------------- | ---------------------------------------------------------------------------------------- |
| Context Menu  | `HellionChat.Available`              | plugin → caller | event, no payload                                                                        |
| Context Menu  | `HellionChat.Register`               | caller → plugin | returns registration `string` ID                                                         |
| Context Menu  | `HellionChat.Unregister`             | caller → plugin | takes registration ID `string`                                                           |
| Context Menu  | `HellionChat.Invoke`                 | plugin → caller | event, see Context Menu section                                                          |
| Typing State  | `HellionChat.GetChatInputState`      | caller → plugin | returns the typing-state tuple                                                           |
| Typing State  | `HellionChat.ChatInputStateChanged`  | plugin → caller | event, fires once on subscribe and on every state change, payload is the same tuple      |

---

## Context Menu IPC

Use this surface to draw your own selectables inside Hellion Chat's
right-click context menus. All registrations are called inside an ImGui
`BeginMenu`, so anything you draw appears as a regular menu entry.

### Lifecycle

1. Subscribe to `HellionChat.Available`. The host fires this once when it
   loads or reloads, so your plugin can re-register without polling.
2. Call `HellionChat.Register` to obtain a registration ID. Save it. You
   need it to filter `Invoke` callbacks that target your registration and
   to call `Unregister` later.
3. Subscribe to `HellionChat.Invoke` and draw your menu items inside the
   handler when the `id` matches your saved registration ID.
4. On plugin disable or unload, call `HellionChat.Unregister` with your
   saved ID and unsubscribe from `Invoke`.

### Example

```cs
public class HellionChatContextMenu {
    // Used to register your plugin with the IPC; returns an ID you must save.
    private ICallGateSubscriber<string> Register { get; }
    // Used to unregister your plugin from the IPC; call this on unload.
    private ICallGateSubscriber<string, object?> Unregister { get; }
    // Subscribe to receive a notification when Hellion Chat becomes ready
    // or reloads, so you can re-register.
    private ICallGateSubscriber<object?> Available { get; }
    // Subscribe to draw your custom context-menu items.
    private ICallGateSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?> Invoke { get; }

    private string? _id;

    public HellionChatContextMenu(DalamudPluginInterface @interface) {
        this.Register   = @interface.GetIpcSubscriber<string>("HellionChat.Register");
        this.Unregister = @interface.GetIpcSubscriber<string, object?>("HellionChat.Unregister");
        this.Invoke     = @interface.GetIpcSubscriber<string, PlayerPayload?, ulong, Payload?, SeString?, SeString?, object?>("HellionChat.Invoke");
        this.Available  = @interface.GetIpcSubscriber<object?>("HellionChat.Available");
    }

    public void Enable() {
        // Re-register automatically when Hellion Chat becomes ready or reloads.
        this.Available.Subscribe(() => this.DoRegister());
        // Register if Hellion Chat is already loaded.
        this.DoRegister();

        // Listen for context-menu events.
        this.Invoke.Subscribe(this.OnInvoke);
    }

    private void DoRegister() {
        this._id = this.Register.InvokeFunc();
    }

    public void Disable() {
        if (this._id != null) {
            this.Unregister.InvokeAction(this._id);
            this._id = null;
        }
        this.Invoke.Unsubscribe(this.OnInvoke);
    }

    private void OnInvoke(string id, PlayerPayload? sender, ulong contentId, Payload? payload, SeString? senderString, SeString? content) {
        // Filter: only react to invocations that target our registration.
        if (id != this._id) {
            return;
        }

        // Draw your custom menu items here. Available context:
        //   sender        — first PlayerPayload in the sender SeString, or null
        //   contentId     — content ID of the message sender, or 0 when not known
        //   payload       — the payload that was right-clicked, or null when text
        //   senderString  — the full sender SeString
        //   content       — the full message content SeString
        if (ImGui.Selectable("Test plugin")) {
            PluginLog.Log($"hi!\nsender: {sender}\ncontent id: {contentId:X}\npayload: {payload}\nsender string: {senderString}\ncontent string: {content}");
        }
    }
}
```

### Migration from Chat 2

If your plugin already integrates with `ChatTwo.*`, the rename is the only
required change:

```diff
-this.Register   = @interface.GetIpcSubscriber<string>("ChatTwo.Register");
-this.Unregister = @interface.GetIpcSubscriber<string, object?>("ChatTwo.Unregister");
-this.Invoke     = @interface.GetIpcSubscriber<...>("ChatTwo.Invoke");
-this.Available  = @interface.GetIpcSubscriber<object?>("ChatTwo.Available");
+this.Register   = @interface.GetIpcSubscriber<string>("HellionChat.Register");
+this.Unregister = @interface.GetIpcSubscriber<string, object?>("HellionChat.Unregister");
+this.Invoke     = @interface.GetIpcSubscriber<...>("HellionChat.Invoke");
+this.Available  = @interface.GetIpcSubscriber<object?>("HellionChat.Available");
```

---

## Typing State IPC

Use this surface when you need to know whether the player is currently
interacting with Hellion Chat's input box. Useful for typing indicators,
keyboard-shortcut suppression, or HUD elements that hide while the user is
typing.

### Tuple Payload

Both `HellionChat.GetChatInputState` (poll) and
`HellionChat.ChatInputStateChanged` (event) return the same tuple:

```cs
(bool InputVisible, bool InputFocused, bool HasText, bool IsTyping, int TextLength, ChatType ChannelType)
```

| Field          | Type       | Meaning                                                                          |
| -------------- | ---------- | -------------------------------------------------------------------------------- |
| `InputVisible` | `bool`     | True when Hellion Chat is not hidden by user, cutscene or battle settings.       |
| `InputFocused` | `bool`     | True while the input box currently has keyboard focus.                           |
| `HasText`      | `bool`     | True when the input buffer contains more than whitespace.                        |
| `IsTyping`     | `bool`     | Convenience flag, equivalent to `InputFocused && HasText`.                       |
| `TextLength`   | `int`      | Length of the raw input buffer in characters.                                    |
| `ChannelType`  | `ChatType` | The channel/mode that will be used if the buffer is submitted right now.         |

### Where `ChannelType` comes from

`ChannelType` is the `HellionChat.Code.ChatType` enum value representing the
target channel for the current submission. It is sourced from the active
tab's `UsedChannel` (`HellionChat/Configuration.cs`), which the plugin keeps
in sync by hooking the in-game shell (`HellionChat/GameFunctions/Chat.cs`)
and by resolving temporary overrides inside the chat UI
(`HellionChat/Ui/ChatLogWindow.cs:597`). `InputChannel` values are converted
into the exported `ChatType` via
`HellionChat/Code/InputChannelExt.ToChatType`.

### Behavior

- `ChatInputStateChanged` fires once immediately after subscribe so you do
  not need a separate `GetChatInputState` poll for the initial snapshot.
- After that it fires only when one or more fields actually change, so it
  is safe to subscribe without rate-limiting.
- `GetChatInputState` is available for one-shot polls, e.g. on plugin
  enable.

### Example

```cs
public sealed class HellionChatTypingIntegration {
    private ICallGateSubscriber<(bool InputVisible, bool InputFocused, bool HasText, bool IsTyping, int TextLength, ChatType ChannelType)> GetChatInputState { get; }
    private ICallGateSubscriber<(bool InputVisible, bool InputFocused, bool HasText, bool IsTyping, int TextLength, ChatType ChannelType)> ChatInputStateChanged { get; }

    public HellionChatTypingIntegration(DalamudPluginInterface @interface) {
        this.GetChatInputState     = @interface.GetIpcSubscriber<(bool, bool, bool, bool, int, ChatType)>("HellionChat.GetChatInputState");
        this.ChatInputStateChanged = @interface.GetIpcSubscriber<(bool, bool, bool, bool, int, ChatType)>("HellionChat.ChatInputStateChanged");
    }

    public void Enable() {
        this.ChatInputStateChanged.Subscribe(OnChatInputStateChanged);

        // Optional: poll once for an initial snapshot. The Subscribe call
        // above already fires once, so this is only useful when your code
        // path needs the value before the framework hands you the event.
        var state = this.GetChatInputState.InvokeFunc();
        PluginLog.Information($"Initial typing state: {state}");
    }

    public void Disable() {
        this.ChatInputStateChanged.Unsubscribe(OnChatInputStateChanged);
    }

    private void OnChatInputStateChanged((bool InputVisible, bool InputFocused, bool HasText, bool IsTyping, int TextLength, ChatType ChannelType) state) {
        if (state.IsTyping) {
            // Show typing indicator.
        } else {
            // Hide typing indicator.
        }
    }
}
```

### Migration from Chat 2

Same shape as the Context Menu surface — only the channel-name prefix needs
the rename:

```diff
-this.GetChatInputState     = @interface.GetIpcSubscriber<...>("ChatTwo.GetChatInputState");
-this.ChatInputStateChanged = @interface.GetIpcSubscriber<...>("ChatTwo.ChatInputStateChanged");
+this.GetChatInputState     = @interface.GetIpcSubscriber<...>("HellionChat.GetChatInputState");
+this.ChatInputStateChanged = @interface.GetIpcSubscriber<...>("HellionChat.ChatInputStateChanged");
```

---

## License & Attribution

This guide and the IPC surface it documents derive directly from the Chat 2
codebase. Hellion Chat is licensed under [EUPL-1.2](LICENSE), and credit for
the original IPC design and implementation goes to **Infiziert90 (Infi)**
and **Anna Clemens** — see [`NOTICE.md`](NOTICE.md) for full attribution.
