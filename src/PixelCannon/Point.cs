using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PixelCannon
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        public static int SizeInBytes => Marshal.SizeOf(typeof(Point));

        public static Point Zero => new Point();

        public int X;

        public int Y;

        public Point(int width, int height)
        {
            X = width;
            Y = height;
        }

        public override string ToString() => $"{{ {X}, {Y} }}";

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Point))
                return false;

            return Equals((Point)obj);
        }

        public bool Equals(Point other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public Vector2 ToVector2() => new Vector2(X, Y);

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !p1.Equals(p2);
        }
    }
}
