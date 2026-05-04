using System.Buffers.Binary;
using System.Numerics;

namespace HellionChat.Util;

internal static class ColourUtil {
    private static (byte r, byte g, byte b) RgbaToRgbComponents(uint rgba)
    {
        var r = (byte) ((rgba & 0xFF000000) >> 24);
        var g = (byte) ((rgba & 0xFF0000) >> 16);
        var b = (byte) ((rgba & 0xFF00) >> 8);
        return (r, g, b);
    }

    internal static uint RgbaToAbgr(uint rgba) => BinaryPrimitives.ReverseEndianness(rgba);

    internal static Vector3 RgbaToVector3(uint rgba)
    {
        var (r, g, b) = RgbaToRgbComponents(rgba);
        return new Vector3((float) r / 255, (float) g / 255, (float) b / 255);
    }

    internal static uint Vector3ToRgba(Vector3 col)
    {
        return ComponentsToRgba(
            (byte) Math.Round(col.X * 255),
            (byte) Math.Round(col.Y * 255),
            (byte) Math.Round(col.Z * 255)
        );
    }

    internal static uint Vector4ToAbgr(Vector4 col)
    {
        return RgbaToAbgr(ComponentsToRgba(
            (byte) Math.Round(col.X * 255),
            (byte) Math.Round(col.Y * 255),
            (byte) Math.Round(col.Z * 255),
            (byte) Math.Round(col.W * 255)
        ));
    }

    public static unsafe uint ArgbToRgba(uint x)
    {
        var buf = (byte*)&x;
        (buf[1], buf[2], buf[3], buf[0]) = (buf[0], buf[1], buf[2], buf[3]);
        return x;
    }

    internal static uint ComponentsToRgba(byte red, byte green, byte blue, byte alpha = 0xFF)
        => alpha | (uint) (red << 24) | (uint) (green << 16) | (uint) (blue << 8);

    internal static uint AdjustBrightness(uint abgr, float factor)
    {
        var a = (byte) ((abgr & 0xFF000000) >> 24);
        var b = (byte) ((abgr & 0x00FF0000) >> 16);
        var g = (byte) ((abgr & 0x0000FF00) >> 8);
        var r = (byte)  (abgr & 0x000000FF);

        var nr = (byte) Math.Clamp(r * factor, 0f, 255f);
        var ng = (byte) Math.Clamp(g * factor, 0f, 255f);
        var nb = (byte) Math.Clamp(b * factor, 0f, 255f);

        return ((uint) a << 24) | ((uint) nb << 16) | ((uint) ng << 8) | nr;
    }
}
