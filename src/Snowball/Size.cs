﻿using System.Runtime.InteropServices;

namespace Snowball
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        public static int SizeInBytes => Marshal.SizeOf(typeof(Size));

        public int Width;

        public int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
