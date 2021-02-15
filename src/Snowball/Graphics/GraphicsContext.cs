using System;
using System.Numerics;

namespace Snowball.Graphics
{
    public abstract class GraphicsContext : IDisposable
    {
        public GameWindow Window { get; }

        public Surface BackBuffer { get; private set; }

        internal GraphicsContext(GameWindow window, Size backBufferSize)
        {
            Guard.NotNull(window, nameof(window));

            Window = window;
            
            var result = PlatformInitialize(backBufferSize);

            BackBuffer = result.BackBuffer;
        }

        protected struct PlatformInitializeResult
        {
            public Surface BackBuffer { get; set; }
        }

        protected abstract PlatformInitializeResult PlatformInitialize(Size backBufferSize);

        public void Dispose()
        {
            PlatformDispose();
        }

        protected abstract void PlatformDispose();

        public void Present()
        {
            PlatformPresent();
        }

        protected abstract void PlatformPresent();

        internal uint AllocateSurface(int width, int height, Pixel[]? pixels = null)
        {
            if (pixels != null)
            {
                var pixelsLength = width * height;
             
                if (pixels.Length != pixelsLength)
                    throw new ArgumentException($"Expected length of pixels array to be {pixelsLength} but was {pixels.Length}.", nameof(pixels));
            }
            else
            {
                pixels = new Pixel[width * height];
            }

            return PlatformAllocateSurface(width, height, pixels);
        }

        protected abstract uint PlatformAllocateSurface(int width, int height, Pixel[] pixels);

        internal Pixel[] GetSurfacePixels(Surface surface)
        {
            return PlatformGetSurfacePixels(surface);
        }

        protected abstract Pixel[] PlatformGetSurfacePixels(Surface surface);

        public void Clear(Surface surface, Pixel pixel)
        {
            Guard.NotNull(surface, nameof(surface));

            PlatformClear(surface, pixel);
        }

        protected abstract void PlatformClear(Surface surface, Pixel pixel);

        public void Blit(Surface destinationSurface, Surface sourceSurface, in Point destination, Rectangle? source = null)
        {
            Guard.NotNull(destinationSurface, nameof(destinationSurface));
            Guard.NotNull(sourceSurface, nameof(sourceSurface));

            if (source == null)
                source = new Rectangle(0, 0, sourceSurface.Width, sourceSurface.Height);

            PlatformBlit(destinationSurface, sourceSurface, destination, source.Value);
        }

        protected abstract void PlatformBlit(Surface destinationSurface, Surface sourceSurface, in Point destination, Rectangle source);

        public void DrawFilledRectangle(Surface surface, Pixel pixel, Rectangle rectangle, PixelMode pixelMode = PixelMode.Overwrite)
        {
            Guard.NotNull(surface, nameof(surface));

            PlatformDrawFilledRectangle(surface, pixel, rectangle, pixelMode);
        }

        protected abstract void PlatformDrawFilledRectangle(Surface surface, Pixel pixel, Rectangle rectangle, PixelMode pixelMode);

        public void DrawFilledRectangle(
            Surface surface,
            Func<Point, Pixel> pixelFunc,
            Rectangle rectangle,
            PixelMode pixelMode = PixelMode.Overwrite)
        {
            Guard.NotNull(surface, nameof(surface));
            Guard.NotNull(pixelFunc, nameof(pixelFunc));            

            PlatformDrawFilledRectangle(surface, pixelFunc, rectangle, pixelMode);
        }

        protected abstract void PlatformDrawFilledRectangle(
            Surface surface,
            Func<Point, Pixel> pixelFunc,
            Rectangle rectangle,
            PixelMode pixelMode);

        public void DrawSprite(
            Surface destinationSurface,
            Surface sourceSurface,
            in Vector2 destination,
            Rectangle? source = null,
            PixelMode pixelMode = PixelMode.Overwrite)
        {
            Guard.NotNull(destinationSurface, nameof(destinationSurface));
            Guard.NotNull(sourceSurface, nameof(sourceSurface));

            if (source == null)
                source = new Rectangle(0, 0, sourceSurface.Width, sourceSurface.Height);

            PlatformDrawSprite(destinationSurface, sourceSurface, destination, source.Value, pixelMode);
        }

        protected abstract void PlatformDrawSprite(
            Surface destinationSurface,
            Surface sourceSurface,
            in Vector2 destination,
            Rectangle source,
            PixelMode pixelMode);

        public void Flush()
        {
            PlatformFlush();
        }

        protected abstract void PlatformFlush();
    }
}
