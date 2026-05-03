using System.Numerics;

namespace HellionChat.Util;

public static class MathUtil
{
    public record Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int SizeX;
        public int SizeY;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            SizeX = X + Width;
            SizeY = Y + Height;
        }

        public Rectangle(Vector2 pos, Vector2 size) : this((int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y) { }

        public override string ToString()
            => $"X: {X} Y: {Y} Width: {Width} Height: {Height}";
    }

    /// <summary>
    /// Checks if two rectangles overlap at any point.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>True if overlapping</returns>
    public static bool HasOverlap(this Rectangle a, Rectangle b)
    {
        // Standard AABB overlap test: two rectangles overlap iff they
        // overlap on both axes. The previous nested ValueInRange approach
        // used strict inequalities at both ends, which dropped identical
        // rectangles and shared-edge cases as false negatives.
        return a.X < b.X + b.Width && a.X + a.Width > b.X &&
               a.Y < b.Y + b.Height && a.Y + a.Height > b.Y;
    }
}