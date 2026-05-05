using System.Text;

namespace HellionChat.Util;

public static class MemoryUtil
{
    // Diagnostic helper. Pointer dereferences here would crash on a null/garbage
    // address and a huge length would log megabytes of raw bytes; both are easy
    // to trigger from a debugger and pollute the log with potentially sensitive
    // game-state. Validate the inputs before reading.
    private const int MaxDumpLength = 4096;

    public static unsafe void PrintMemoryArea(nint address, int length)
    {
        if (address == nint.Zero)
            throw new ArgumentException("Memory address cannot be zero.", nameof(address));
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be positive.");
        if (length > MaxDumpLength)
            throw new ArgumentOutOfRangeException(nameof(length), length, $"Length exceeds the {MaxDumpLength}-byte safety cap.");

        var ptr = (byte*)address;
        var str = new StringBuilder("\n");
        for(var i = 0; i < length; i++)
        {
            str.Append($"{ptr![i]:X02}");

            if (i == 0)
                continue;

            if ((i+1) % 16 == 0)
                str.Append('\n');
            else if ((i+1) % 4 == 0)
                str.Append(' ');
        }

        Plugin.Log.Information(str.ToString());
    }
}