using System;
using System.Runtime.InteropServices;

#nullable disable

namespace Snowball.Platforms.OpenGL
{
    public static unsafe class GL
    {
        public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_QUADS = 0x0007;
        public const uint GL_RGBA = 0x1908;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
        public const uint GL_UNSIGNED_BYTE = 0x1401;

        public delegate void _glBegin(uint mode);
        public static _glBegin glBegin;

        public delegate void _glBindTexture(uint target, uint texture);
        public static _glBindTexture glBindTexture;

        public delegate void _glClear(uint mask);
        public static _glClear glClear;

        public delegate void _glClearColor(float red, float green, float blue, float alpha);
        public static _glClearColor glClearColor;

        public delegate void _glEnable(uint cap);
        public static _glEnable glEnable;

        public delegate void _glEnd();
        public static _glEnd glEnd;

        public delegate void _glGenTextures(int n, uint* textures);
        public static _glGenTextures glGenTextures;

        public delegate void _glTexCoord2f(float s, float t);
        public static _glTexCoord2f glTexCoord2f;

        public delegate void _glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, void* pixels);
        public static _glTexImage2D glTexImage2D;

        public delegate void _glTexParameteri(uint target, uint pname, int param);
        public static _glTexParameteri glTexParameteri;

        public delegate void _glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, void* pixels);
        public static _glTexSubImage2D glTexSubImage2D;

        public delegate void _glVertex3f(float x, float y, float z);
        public static _glVertex3f glVertex3f;

        public delegate void _glViewport(int x, int y, int width, int height);
        public static _glViewport glViewport;

        public static void glInit(Func<string, IntPtr> getProcAddress)
        {
            glBegin = Marshal.GetDelegateForFunctionPointer<_glBegin>(getProcAddress(nameof(glBegin)));
            glBindTexture = Marshal.GetDelegateForFunctionPointer<_glBindTexture>(getProcAddress(nameof(glBindTexture)));
            glClear = Marshal.GetDelegateForFunctionPointer<_glClear>(getProcAddress(nameof(glClear)));
            glClearColor = Marshal.GetDelegateForFunctionPointer<_glClearColor>(getProcAddress(nameof(glClearColor)));
            glEnable = Marshal.GetDelegateForFunctionPointer<_glEnable>(getProcAddress(nameof(glEnable)));
            glEnd = Marshal.GetDelegateForFunctionPointer<_glEnd>(getProcAddress(nameof(glEnd)));
            glGenTextures = Marshal.GetDelegateForFunctionPointer<_glGenTextures>(getProcAddress(nameof(glGenTextures)));
            glTexCoord2f = Marshal.GetDelegateForFunctionPointer<_glTexCoord2f>(getProcAddress(nameof(glTexCoord2f)));
            glTexImage2D = Marshal.GetDelegateForFunctionPointer<_glTexImage2D>(getProcAddress(nameof(glTexImage2D)));
            glTexParameteri = Marshal.GetDelegateForFunctionPointer<_glTexParameteri>(getProcAddress(nameof(glTexParameteri)));
            glTexSubImage2D = Marshal.GetDelegateForFunctionPointer<_glTexSubImage2D>(getProcAddress(nameof(glTexSubImage2D)));
            glVertex3f = Marshal.GetDelegateForFunctionPointer<_glVertex3f>(getProcAddress(nameof(glVertex3f)));
            glViewport = Marshal.GetDelegateForFunctionPointer<_glViewport>(getProcAddress(nameof(glClearColor)));
        }
    }
}
