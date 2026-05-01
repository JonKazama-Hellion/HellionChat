using System.Globalization;
using System.Text;
using ChatTwo.Code;

namespace ChatTwo.Export;

internal enum ExportFormat
{
    Markdown,
    Json,
    Csv,
}

internal static class ExportFormatExt
{
    internal static string Extension(this ExportFormat fmt) => fmt switch
    {
        ExportFormat.Markdown => "md",
        ExportFormat.Json => "json",
        ExportFormat.Csv => "csv",
        _ => "txt",
    };

    internal static string Filter(this ExportFormat fmt) => fmt switch
    {
        ExportFormat.Markdown => ".md",
        ExportFormat.Json => ".json",
        ExportFormat.Csv => ".csv",
        _ => ".txt",
    };
}

/// <summary>
/// Serializes message snapshots into Markdown, JSON, or CSV. The caller is
/// expected to filter the input enumerable; this class only handles
/// formatting and writes to the supplied path. Sender substring filtering
/// happens here because it requires deserialized SeString.TextValue.
/// </summary>
internal static class MessageExporter
{
    internal record FilterDescription(
        IReadOnlyCollection<int>? ChatTypes,
        DateTimeOffset? From,
        DateTimeOffset? To,
        string? SenderSubstring);

    internal static int ExportToFile(
        string path,
        ExportFormat format,
        IEnumerable<Message> messages,
        FilterDescription filter)
    {
        var matching = filter.SenderSubstring is { Length: > 0 } needle
            ? messages.Where(m => MatchesSender(m, needle))
            : messages;

        using var writer = new StreamWriter(path, append: false, encoding: Encoding.UTF8);
        return format switch
        {
            ExportFormat.Markdown => WriteMarkdown(writer, matching, filter),
            ExportFormat.Json => WriteJson(writer, matching, filter),
            ExportFormat.Csv => WriteCsv(writer, matching, filter),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    private static bool MatchesSender(Message m, string needle)
        => m.SenderSource.TextValue.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static int WriteMarkdown(StreamWriter w, IEnumerable<Message> messages, FilterDescription filter)
    {
        w.WriteLine("# Hellion Chat Export");
        w.WriteLine();
        w.WriteLine($"Generated: {DateTimeOffset.Now:yyyy-MM-dd HH:mm zzz}");
        WriteFilterSummaryMarkdown(w, filter);
        w.WriteLine();

        DateTimeOffset? lastDate = null;
        var count = 0;
        foreach (var m in messages)
        {
            count++;
            var localDate = m.Date.ToLocalTime();
            if (lastDate is null || localDate.Date != lastDate.Value.Date)
            {
                w.WriteLine();
                w.WriteLine($"## {localDate:yyyy-MM-dd}");
                w.WriteLine();
                lastDate = localDate;
            }

            var chatType = (ChatType)(ushort)m.Code.Type;
            var sender = m.SenderSource.TextValue.Trim().Trim('<', '>', '[', ']', ':').Trim();
            var content = m.ContentSource.TextValue;
            if (string.IsNullOrEmpty(sender))
                w.WriteLine($"**[{localDate:HH:mm}] {chatType}:** {content}");
            else
                w.WriteLine($"**[{localDate:HH:mm}] {chatType} {sender}:** {content}");
        }

        w.WriteLine();
        w.WriteLine($"---");
        w.WriteLine($"Total messages: {count}");
        return count;
    }

    private static void WriteFilterSummaryMarkdown(StreamWriter w, FilterDescription filter)
    {
        if (filter.ChatTypes is { Count: > 0 })
            w.WriteLine($"ChatTypes: {string.Join(", ", filter.ChatTypes.Select(t => $"{(ChatType)(ushort)t}({t})"))}");
        if (filter.From is not null)
            w.WriteLine($"From: {filter.From.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
        if (filter.To is not null)
            w.WriteLine($"To: {filter.To.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
        if (filter.SenderSubstring is { Length: > 0 })
            w.WriteLine($"Sender contains: \"{filter.SenderSubstring}\"");
    }

    private static int WriteJson(StreamWriter w, IEnumerable<Message> messages, FilterDescription filter)
    {
        // Manual JSON to avoid pulling in System.Text.Json policy choices.
        // Output is a single object with metadata and an array of messages.
        w.Write("{\n  \"exported_at\": \"");
        w.Write(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        w.Write("\",\n  \"plugin\": \"Hellion Chat\",\n");
        w.Write("  \"filter\": {\n");
        w.Write("    \"chat_types\": ");
        if (filter.ChatTypes is { Count: > 0 })
            w.Write("[" + string.Join(",", filter.ChatTypes) + "]");
        else
            w.Write("null");
        w.Write(",\n    \"from\": ");
        w.Write(filter.From is null ? "null" : "\"" + filter.From.Value.ToString("O", CultureInfo.InvariantCulture) + "\"");
        w.Write(",\n    \"to\": ");
        w.Write(filter.To is null ? "null" : "\"" + filter.To.Value.ToString("O", CultureInfo.InvariantCulture) + "\"");
        w.Write(",\n    \"sender_substring\": ");
        w.Write(filter.SenderSubstring is null ? "null" : JsonString(filter.SenderSubstring));
        w.Write("\n  },\n  \"messages\": [\n");

        var first = true;
        var count = 0;
        foreach (var m in messages)
        {
            if (!first)
                w.Write(",\n");
            first = false;
            count++;

            var chatType = (ChatType)(ushort)m.Code.Type;
            w.Write("    {");
            w.Write($"\"id\":\"{m.Id}\"");
            w.Write($",\"date\":\"{m.Date.ToString("O", CultureInfo.InvariantCulture)}\"");
            w.Write($",\"chat_type\":{(int)m.Code.Type}");
            w.Write($",\"chat_type_name\":\"{chatType}\"");
            w.Write($",\"source_kind\":{m.Code.Source}");
            w.Write($",\"target_kind\":{m.Code.Target}");
            w.Write($",\"receiver\":{m.Receiver}");
            w.Write($",\"content_id\":{m.ContentId}");
            w.Write($",\"sender\":{JsonString(m.SenderSource.TextValue)}");
            w.Write($",\"content\":{JsonString(m.ContentSource.TextValue)}");
            w.Write("}");
        }

        w.Write("\n  ],\n");
        w.Write($"  \"total\": {count}\n}}\n");
        return count;
    }

    private static int WriteCsv(StreamWriter w, IEnumerable<Message> messages, FilterDescription filter)
    {
        // Header line always written so empty exports are still importable.
        w.WriteLine("Date,ChatType,ChatTypeName,Sender,Content,Receiver,ContentId");
        var count = 0;
        foreach (var m in messages)
        {
            count++;
            var chatType = (ChatType)(ushort)m.Code.Type;
            w.Write(m.Date.ToString("O", CultureInfo.InvariantCulture));
            w.Write(',');
            w.Write((int)m.Code.Type);
            w.Write(',');
            w.Write(CsvString(chatType.ToString()));
            w.Write(',');
            w.Write(CsvString(m.SenderSource.TextValue));
            w.Write(',');
            w.Write(CsvString(m.ContentSource.TextValue));
            w.Write(',');
            w.Write(m.Receiver);
            w.Write(',');
            w.Write(m.ContentId);
            w.WriteLine();
        }
        return count;
    }

    private static string JsonString(string s)
    {
        var sb = new StringBuilder(s.Length + 2);
        sb.Append('"');
        foreach (var c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                        sb.Append($"\\u{(int)c:x4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static string CsvString(string s)
    {
        if (s.IndexOfAny(['"', ',', '\n', '\r']) < 0)
            return s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
