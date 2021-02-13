using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Snowball.Platforms.OpenGL;

namespace Snowball
{
    internal static class NativeLibraryLoader
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, ResolveDllImport);
        }

        private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == GL.LibraryName)
            {
                return NativeLibrary.Load("opengl32");
            }

            return IntPtr.Zero;
        }
    }
}
