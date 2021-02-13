using System.Runtime.InteropServices;

#nullable disable

namespace Snowball.Platforms.OpenGL
{
    public static unsafe class GL
    {
        public const string LibraryName = "opengl";

        public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_QUADS = 0x0007;
        public const uint GL_RGBA = 0x1908;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
        public const uint GL_UNSIGNED_BYTE = 0x1401;

        [DllImport(LibraryName)]
        public static extern void glBegin(uint mode);

        [DllImport(LibraryName)]
        public static extern void glBindTexture(uint target, uint texture);

        [DllImport(LibraryName)]
        public static extern void glClear(uint mask);

        [DllImport(LibraryName)]
        public static extern void glClearColor(float red, float green, float blue, float alpha);

        [DllImport(LibraryName)]
        public static extern void glEnable(uint cap);

        [DllImport(LibraryName)]
        public static extern void glEnd();

        [DllImport(LibraryName)]
        public static extern void glGenTextures(int n, uint* textures);

        [DllImport(LibraryName)]
        public static extern void glTexCoord2f(float s, float t);

        [DllImport(LibraryName)]
        public static extern void glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, void* pixels);

        [DllImport(LibraryName)]
        public static extern void glTexParameteri(uint target, uint pname, int param);

        [DllImport(LibraryName)]
        public static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, void* pixels);

        [DllImport(LibraryName)]
        public static extern void glVertex3f(float x, float y, float z);

        [DllImport(LibraryName)]
        public static extern void glViewport(int x, int y, int width, int height);
    }
}
