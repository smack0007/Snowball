using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Snowball
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle : IEquatable<Rectangle>
    {
        public static int SizeInBytes => Marshal.SizeOf(typeof(Rectangle));

        public static Rectangle Empty => new Rectangle();

        public int X;

        public int Y;

        public int Width;

        public int Height;

        /// <summary>
        /// The left side of the Rectangle.
        /// </summary>
        public int Left
        {
            get { return X; }
            set { X = value; }
        }

        /// <summary>
        /// The top side of the Rectangle.
        /// </summary>
        public int Top
        {
            get { return Y; }
            set { Y = value; }
        }

        /// <summary>
        /// The right side of the Rectangle.
        /// </summary>
        public int Right
        {
            get { return X + Width; }
            set { X = value - Width; }
        }

        /// <summary>
        /// The bottom side of the Rectangle.
        /// </summary>
        public int Bottom
        {
            get { return Y + Height; }
            set { Y = value - Height; }
        }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Rectangle))
                return false;

            return Equals((Rectangle)obj);
        }

        public bool Equals(Rectangle other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        /// <summary>
        /// Returns true if r1 instersects r2.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool Intersects(Rectangle r1, Rectangle r2)
        {
            return Intersects(ref r1, ref r2);
        }

        /// <summary>
        /// Returns true if r1 intersects r2.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool Intersects(ref Rectangle r1, ref Rectangle r2)
        {
            if (r2.Left > r1.Right || r2.Right < r1.Left ||
               r2.Top > r1.Bottom || r2.Bottom < r1.Top)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the Rectangle intersects with the other.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(Rectangle other)
        {
            return Intersects(ref this, ref other);
        }

        /// <summary>
        /// Returns true if the Rectangle intersects with the other.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(ref Rectangle other)
        {
            return Intersects(ref this, ref other);
        }

        /// <summary>
        /// Returns true if the Rectangle contains the given Point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Point point)
        {
            return point.X >= X &&
                   point.X <= X + Width &&
                   point.Y >= Y &&
                   point.Y <= Y + Height;
        }

        /// <summary>
        /// Returns true if the Rectangle contains the given Vector2.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public bool Contains(Vector2 vec)
        {
            return vec.X >= X &&
                   vec.X <= X + Width &&
                   vec.Y >= Y &&
                   vec.Y <= Y + Height;
        }

        /// <summary>
        /// Returns true if the Rectangle contains the given X and Y coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(int x, int y)
        {
            return x >= X &&
                   x <= X + Width &&
                   y >= Y &&
                   y <= Y + Height;
        }

        public static bool operator ==(Rectangle r1, Rectangle r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !r1.Equals(r2);
        }
    }
}
