using System.Runtime.InteropServices;

namespace Snowball
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel
    {
        public static int SizeInBytes => Marshal.SizeOf(typeof(Pixel));

        public byte R;

        public byte G;

        public byte B;

        public byte A;

        public Pixel(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
