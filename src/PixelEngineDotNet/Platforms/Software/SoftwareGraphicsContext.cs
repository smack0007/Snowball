using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PixelEngineDotNet.Graphics;
using static PixelEngineDotNet.Platforms.OpenGL.GL;
using static PixelEngineDotNet.Platforms.Win32.WGL;

namespace PixelEngineDotNet.Platforms.Software
{
    public class SoftwareGraphicsContext : GraphicsContext
    {
        struct SurfaceData
        {
            public Pixel[] Pixels;
            public GCHandle GCHandle;
        }

        private uint _backBufferTexture;

        private Dictionary<uint, SurfaceData> _surfaces = new Dictionary<uint, SurfaceData>();

        public SoftwareGraphicsContext(GameWindow window, Size backBufferSize)
            : base(window, backBufferSize)
        {
        }

        protected override PlatformInitializeResult PlatformInitialize(Size backBufferSize)
        {
            wglInit(Window.Handle, 2, 1);
            glInit(wglGetProcAddress);

            glClearColor(0, 0, 0, 1.0f);
            glEnable(GL_TEXTURE_2D);

            unsafe
            {
                fixed (uint* backBufferTexturePtr = &_backBufferTexture)
                {
                    glGenTextures(1, backBufferTexturePtr);
                }
            }

            glBindTexture(GL_TEXTURE_2D, _backBufferTexture);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST);

            var backBuffer = new Surface(this, backBufferSize.Width, backBufferSize.Height);

            unsafe
            {
                fixed (void* surfacePtr = _surfaces[backBuffer.Handle].Pixels)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, backBufferSize.Width, backBufferSize.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, surfacePtr);
                }
            }

            return new PlatformInitializeResult()
            {
                BackBuffer = backBuffer
            };
        }

        protected override void PlatformDispose()
        {
            // TODO: glDeleteTextures(1, &_backBufferTexture);
        }

        protected override void PlatformPresent()
        {
            glClear(GL_COLOR_BUFFER_BIT);

            glViewport(0, 0, Window.Width, Window.Height);

            unsafe
            {
                fixed (void* surfacePtr = _surfaces[BackBuffer.Handle].Pixels)
                {
                    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, BackBuffer.Width, BackBuffer.Height, GL_RGBA, GL_UNSIGNED_BYTE, surfacePtr);
                }
            }

            glBegin(GL_QUADS);
            glTexCoord2f(0.0f, 1.0f);
            glVertex3f(-1.0f, -1.0f, 0.0f);
            glTexCoord2f(0.0f, 0.0f);
            glVertex3f(-1.0f, 1.0f, 0.0f);
            glTexCoord2f(1.0f, 0.0f);
            glVertex3f(1.0f, 1.0f, 0.0f);
            glTexCoord2f(1.0f, 1.0f);
            glVertex3f(1.0f, -1.0f, 0.0f);
            glEnd();

            wglSwapBuffers();
        }

        protected override uint PlatformAllocateSurface(int width, int height, Pixel[] pixels)
        {
            var handle = (uint)_surfaces.Count;
            
            var data = new SurfaceData()
            {
                Pixels = pixels,
            };

            data.GCHandle = GCHandle.Alloc(data.Pixels, GCHandleType.Pinned);

            _surfaces[handle] = data;

            return handle;
        }

        protected override Pixel[] PlatformGetSurfacePixels(Surface surface)
        {
            return _surfaces[surface.Handle].Pixels;
        }

        protected override void PlatformClear(Surface surface, Pixel pixel)
        {
            var pixels = _surfaces[surface.Handle].Pixels;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = pixel;
            }
        }

        protected override void PlatformBlit(Surface destinationSurface, Surface sourceSurface, in Point destination, Rectangle source)
        {
            var destinationPixelsHandle = _surfaces[destinationSurface.Handle].GCHandle;
            var sourcePixelsHandle = _surfaces[sourceSurface.Handle].GCHandle;

            unsafe
            {
                Pixel* destinationPixelsPtr = (Pixel*)destinationPixelsHandle.AddrOfPinnedObject();
                Pixel* sourcePixelsPtr = (Pixel*)sourcePixelsHandle.AddrOfPinnedObject();

                destinationPixelsPtr += (destination.Y * destinationSurface.Width) + destination.X;

                int count = sourceSurface.Width;

                if (destination.X + count > destinationSurface.Width)
                    count = destinationSurface.Width - destination.X;

                int countInBytes = count * Pixel.SizeInBytes;

                for (int y = 0; y < source.Height; y++)
                {
                    int destY = destination.Y + y;

                    if (destY < 0)
                        continue;

                    if (destY >= destinationSurface.Height)
                        break;

                    Buffer.MemoryCopy(sourcePixelsPtr, destinationPixelsPtr, countInBytes, countInBytes);

                    destinationPixelsPtr += destinationSurface.Width;
                    sourcePixelsPtr += sourceSurface.Width;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawPixel(ref Pixel destination, ref Pixel source, PixelMode pixelMode)
        {
            switch (pixelMode)
            {
                case PixelMode.Overwrite:
                    destination.R = source.R;
                    destination.G = source.G;
                    destination.B = source.B;
                    destination.A = source.A;
                    break;

                case PixelMode.AlphaBlend:
                    float blendFactor = 1.0f;
                    float a = (source.A / 255.0f) * blendFactor;
                    float c = 1.0f - a;
                    float r = a * source.R + c * destination.R;
                    float g = a * source.G + c * destination.G;
                    float b = a * source.B + c * destination.B;

                    destination.R = (byte)r;
                    destination.G = (byte)g;
                    destination.B = (byte)b;
                    break;
            }
        }

        protected override void PlatformDrawFilledRectangle(Pixel pixel, Rectangle rectangle, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[DrawTarget.Handle].Pixels;

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    DrawPixel(
                        ref drawTargetPixels[((rectangle.Y + y) * DrawTarget.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        protected override void PlatformDrawFilledRectangle(Func<Point, Pixel> pixelFunc, Rectangle rectangle, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[DrawTarget.Handle].Pixels;

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var pixel = pixelFunc(new Point(x, y));
                    DrawPixel(
                        ref drawTargetPixels[((rectangle.Y + y) * DrawTarget.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        protected override void PlatformDrawSprite(Surface surface, in Vector2 destination, Rectangle source, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[DrawTarget.Handle].Pixels;
            var surfacePixels = _surfaces[surface.Handle].Pixels;

            for (int y = 0; y < source.Height; y++)
            {
                int srcY = source.Y + y;

                if (srcY < 0)
                    continue;

                if (srcY >= surface.Height)
                    break;

                int destY = (int)(destination.Y + y);

                if (destY < 0)
                    continue;

                if (destY >= DrawTarget.Height)
                    break;

                for (int x = 0; x < source.Width; x++)
                {
                    int srcX = source.X + x;

                    if (srcX < 0)
                        continue;

                    if (srcX >= surface.Width)
                        break;

                    int destX = (int)(destination.X + x);

                    if (destX < 0)
                        continue;

                    if (destX >= DrawTarget.Width)
                        break;

                    DrawPixel(
                        ref drawTargetPixels[destY * DrawTarget.Width + destX],
                        ref surfacePixels[srcY * surface.Width + srcX],
                        pixelMode);
                }
            }
        }
    }
}
