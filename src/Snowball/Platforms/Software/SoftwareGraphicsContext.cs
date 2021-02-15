using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Snowball.Graphics;
using static Snowball.Platforms.OpenGL.GL;
using static Snowball.Platforms.Win32.WGL;

namespace Snowball.Platforms.Software
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

        protected override void PlatformDrawLine(Surface surface, Pixel pixel, Vector2 p1, Vector2 p2, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[surface.Handle].Pixels;

            if (p1.X == p2.X) // Is line vertical?
            {
                // We want to draw from top to bottom
                if (p1.Y > p2.Y)
                {
                    var temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                int x = (int)Math.Round(p1.X);
                int y = (int)Math.Round(p1.Y);
                int y2 = (int)Math.Round(p2.Y);

                if (y < 0)
                    y = 0;

                for (; y <= y2; y++)
                {
                    if (y >= surface.Height)
                        break;

                    DrawPixel(
                        ref drawTargetPixels[y * surface.Width + x],
                        ref pixel,
                        pixelMode);
                }
            }
            else
            {
                // We want to draw from left to right
                if (p1.X > p2.X)
                {
                    var temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                Vector2 delta = p2 - p1;
                float deltaError = Math.Abs(delta.Y / delta.X);

                float error = 0.0f;
                int x = (int)Math.Round(p1.X);
                int y = (int)Math.Round(p1.Y);

                int x2 = (int)Math.Round(p2.X);
                int ySign = Math.Sign(delta.Y);

                for (; x <= x2; x++)
                {
                    // If we've gone past the width of the surface we can quit.
                    if (x >= surface.Width)
                        break;

                    // Only plot the pixel if we're inside the bounds
                    if (x >= 0 && y >= 0 && y < surface.Height)
                    {
                        DrawPixel(
                            ref drawTargetPixels[y * surface.Width + x],
                            ref pixel,
                            pixelMode);
                    }

                    error += deltaError;
                    if (error >= 0.5f)
                    {
                        y += ySign;
                        error -= 1.0f;
                    }
                }
            }
        }

        protected override void PlatformDrawFilledRectangle(Surface surface, Pixel pixel, Rectangle rectangle, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[surface.Handle].Pixels;

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    DrawPixel(
                        ref drawTargetPixels[((rectangle.Y + y) * surface.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        protected override void PlatformDrawFilledRectangle(Surface surface, Func<Point, Pixel> pixelFunc, Rectangle rectangle, PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[surface.Handle].Pixels;

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var pixel = pixelFunc(new Point(x, y));
                    DrawPixel(
                        ref drawTargetPixels[((rectangle.Y + y) * surface.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        protected override void PlatformDrawSprite(
            Surface destinationSurface,
            Surface sourceSurface,
            in Vector2 destination,
            Rectangle source,
            PixelMode pixelMode)
        {
            var drawTargetPixels = _surfaces[destinationSurface.Handle].Pixels;
            var surfacePixels = _surfaces[sourceSurface.Handle].Pixels;

            for (int y = 0; y < source.Height; y++)
            {
                int srcY = source.Y + y;

                if (srcY < 0)
                    continue;

                if (srcY >= sourceSurface.Height)
                    break;

                int destY = (int)(destination.Y + y);

                if (destY < 0)
                    continue;

                if (destY >= destinationSurface.Height)
                    break;

                for (int x = 0; x < source.Width; x++)
                {
                    int srcX = source.X + x;

                    if (srcX < 0)
                        continue;

                    if (srcX >= sourceSurface.Width)
                        break;

                    int destX = (int)(destination.X + x);

                    if (destX < 0)
                        continue;

                    if (destX >= destinationSurface.Width)
                        break;

                    DrawPixel(
                        ref drawTargetPixels[destY * destinationSurface.Width + destX],
                        ref surfacePixels[srcY * sourceSurface.Width + srcX],
                        pixelMode);
                }
            }
        }

        protected override void PlatformFlush()
        {
            // Software renderer doesn't need to do anything in Flush().
        }
    }
}
