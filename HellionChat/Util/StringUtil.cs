using System.Globalization;
using System.Text;
using Dalamud.Bindings.ImGui;

namespace HellionChat.Util;

internal static class StringUtil
{
    internal static byte[] ToTerminatedBytes(this string s)
    {
        var utf8 = Encoding.UTF8;
        var bytes = new byte[utf8.GetByteCount(s) + 1];
        utf8.GetBytes(s, 0, s.Length, bytes, 0);
        bytes[^1] = 0;
        return bytes;
    }

    // Taken from https://stackoverflow.com/a/4975942
    internal static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"]; // Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);
        // "0.#" keeps the rounded fractional digit (1.5 GB stays "1.5GB"); "N0"
        // would truncate it back to integer. InvariantCulture pins the decimal
        // separator to '.' so a German locale doesn't render "1,5GB".
        return (Math.Sign(byteCount) * num).ToString("0.#", CultureInfo.InvariantCulture) + suf[place];
    }

    // Returns the text unchanged when it already fits the width budget,
    // otherwise the longest prefix plus a horizontal-ellipsis character that
    // still fits. Used by the chat header Honorific title slot and reused by
    // the chat-line truncation path in later cycles.
    public static string TruncateToFitWidth(string text, float maxWidth)
    {
        if (ImGui.CalcTextSize(text).X <= maxWidth)
        {
            return text;
        }

        // Binary-search the longest prefix that fits with an ellipsis.
        const string ellipsis = "…";
        var lo = 0;
        var hi = text.Length;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            var candidate = text[..mid] + ellipsis;
            if (ImGui.CalcTextSize(candidate).X <= maxWidth)
            {
                lo = mid;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return lo == 0 ? ellipsis : text[..lo] + ellipsis;
    }
}
