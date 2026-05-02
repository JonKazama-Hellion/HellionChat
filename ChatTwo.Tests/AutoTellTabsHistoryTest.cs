using System;
using System.IO;
using System.Linq;
using ChatTwo.Code;
using ChatTwo.Util;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatTwo.Tests;

// Hellion Chat — Auto-Tell-Tabs history-preload coverage.
//
// These tests exercise MessageStore.GetTellHistoryWithSender, the query the
// AutoTellTabsService uses to populate a freshly spawned temp tab with the
// last conversations with that player.
//
// NOTE: like the rest of ChatTwo.Tests today, these will fail at runtime
// until the project's Dalamud.dll runtime dependency is sorted out (see
// Phase-2 backlog item "Test-Projekt fixen"). Compile-time the suite builds
// fine via DALAMUD_HOME, so the tests guard against API drift even before
// they can be executed locally.
[TestClass]
[TestSubject(typeof(MessageStore))]
public class AutoTellTabsHistoryTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [Timeout(5000)]
    public void GetTellHistoryWithSender_FiltersByNameAndWorld()
    {
        var tempDir = Directory.CreateTempSubdirectory("ChatTwo_test_");
        var dbPath = Path.Join(tempDir.FullName, "test.db");
        TestContext.WriteLine("Using database path: " + dbPath);
        using var store = new MessageStore(dbPath);

        const ulong receiver = 99001;
        var now = DateTimeOffset.UtcNow;

        // Two tells with the target sender, one with a different sender on
        // the same world, one with the same name on a different world. Only
        // the first two should make it into the result.
        var asukaLichIn = TellMessage("Asuka", 76, receiver, now.AddMinutes(-30), ChatType.TellIncoming);
        var asukaLichOut = TellMessage("Asuka", 76, receiver, now.AddMinutes(-20), ChatType.TellOutgoing);
        var broboLich = TellMessage("Brobo", 76, receiver, now.AddMinutes(-10), ChatType.TellIncoming);
        var asukaOmega = TellMessage("Asuka", 90, receiver, now.AddMinutes(-5), ChatType.TellIncoming);

        store.UpsertMessage(asukaLichIn);
        store.UpsertMessage(asukaLichOut);
        store.UpsertMessage(broboLich);
        store.UpsertMessage(asukaOmega);

        var result = store.GetTellHistoryWithSender(receiver, "Asuka", 76, limit: 50);

        Assert.AreEqual(2, result.Count);
        // Result is oldest-first so a tab can append messages chronologically.
        Assert.AreEqual(asukaLichIn.Id, result[0].Id);
        Assert.AreEqual(asukaLichOut.Id, result[1].Id);
    }

    [TestMethod]
    [Timeout(5000)]
    public void GetTellHistoryWithSender_RespectsLimit()
    {
        var tempDir = Directory.CreateTempSubdirectory("ChatTwo_test_");
        var dbPath = Path.Join(tempDir.FullName, "test.db");
        TestContext.WriteLine("Using database path: " + dbPath);
        using var store = new MessageStore(dbPath);

        const ulong receiver = 99002;
        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < 30; i++)
        {
            var msg = TellMessage("Asuka", 76, receiver, now.AddMinutes(-i - 1), ChatType.TellIncoming);
            store.UpsertMessage(msg);
        }

        var result = store.GetTellHistoryWithSender(receiver, "Asuka", 76, limit: 5);

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    [Timeout(5000)]
    public void GetTellHistoryWithSender_ZeroLimitReturnsEmpty()
    {
        var tempDir = Directory.CreateTempSubdirectory("ChatTwo_test_");
        var dbPath = Path.Join(tempDir.FullName, "test.db");
        TestContext.WriteLine("Using database path: " + dbPath);
        using var store = new MessageStore(dbPath);

        const ulong receiver = 99003;

        var msg = TellMessage("Asuka", 76, receiver, DateTimeOffset.UtcNow, ChatType.TellIncoming);
        store.UpsertMessage(msg);

        var result = store.GetTellHistoryWithSender(receiver, "Asuka", 76, limit: 0);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [Timeout(5000)]
    public void GetTellHistoryWithSender_IgnoresOtherReceivers()
    {
        var tempDir = Directory.CreateTempSubdirectory("ChatTwo_test_");
        var dbPath = Path.Join(tempDir.FullName, "test.db");
        TestContext.WriteLine("Using database path: " + dbPath);
        using var store = new MessageStore(dbPath);

        const ulong ourReceiver = 99004;
        const ulong otherReceiver = 99005;
        var now = DateTimeOffset.UtcNow;

        // Tell on the local player's account.
        var ours = TellMessage("Asuka", 76, ourReceiver, now.AddMinutes(-1), ChatType.TellIncoming);
        // Same sender, but logged against a different local character —
        // common when the user has alts. Must not surface.
        var foreign = TellMessage("Asuka", 76, otherReceiver, now, ChatType.TellIncoming);

        store.UpsertMessage(ours);
        store.UpsertMessage(foreign);

        var result = store.GetTellHistoryWithSender(ourReceiver, "Asuka", 76, limit: 50);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(ours.Id, result[0].Id);
    }

    private static Message TellMessage(
        string senderName,
        uint senderWorld,
        ulong receiver,
        DateTimeOffset dateTime,
        ChatType chatType)
    {
        var senderSeString = new SeStringBuilder()
            .Add(new PlayerPayload(senderName, senderWorld))
            .AddText(senderName)
            .Add(RawPayload.LinkTerminator)
            .Build();

        var contentSeString = new SeStringBuilder()
            .AddText("test message")
            .Build();

        var senderChunks = ChunkUtil.ToChunks(senderSeString, ChunkSource.Sender, chatType).ToList();
        var contentChunks = ChunkUtil.ToChunks(contentSeString, ChunkSource.Content, chatType).ToList();

        var chatCode = new ChatCode((XivChatType)chatType, XivChatRelationKind.LocalPlayer, XivChatRelationKind.LocalPlayer);
        return new Message(
            Guid.NewGuid(),
            receiver,
            0,
            dateTime,
            chatCode,
            senderChunks,
            contentChunks,
            senderSeString,
            contentSeString,
            Guid.Empty);
    }
}
