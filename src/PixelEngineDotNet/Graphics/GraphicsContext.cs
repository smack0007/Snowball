using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelEngineDotNet.Graphics
{
    public partial class GraphicsContext
    {
        public GameWindow Window { get; }

        public Surface BackBuffer { get; private set; }

        private readonly uint _backBufferTexture;

        private bool _drawInProgress;
        private Surface _drawTarget;

        public GraphicsContext(GameWindow window, Size backBufferSize)
        {
            Guard.NotNull(window, nameof(window));

            Window = window;
            PlatformInitialize(backBufferSize);
        }

        public void Present()
        {
            PlatformPresent();
        }

        public void Clear(Surface surface, Pixel pixel)
        {
            for (int i = 0; i < surface.Pixels.Length; i++)
            {
                surface.Pixels[i] = pixel;
            }
        }

        public void Blit(Surface destinationSurface, Surface sourceSurface, in Point destination, Rectangle? source = null)
        {
            Guard.NotNull(destinationSurface, nameof(destinationSurface));
            Guard.NotNull(sourceSurface, nameof(sourceSurface));

            if (source == null)
                source = new Rectangle(0, 0, sourceSurface.Width, sourceSurface.Height);

            for (int y = 0; y < source.Value.Height; y++)
            {
                int srcY = source.Value.Y + y;

                if (srcY < 0)
                    continue;

                if (srcY >= sourceSurface.Height)
                    break;

                int destY = (int)(destination.Y + y);

                if (destY < 0)
                    continue;

                if (destY >= destinationSurface.Height)
                    break;

                for (int x = 0; x < source.Value.Width; x++)
                {
                    int srcX = source.Value.X + x;

                    if (srcX < 0)
                        continue;

                    if (srcX >= sourceSurface.Width)
                        break;

                    int destX = (int)(destination.X + x);

                    if (destX < 0)
                        continue;

                    if (destX >= destinationSurface.Width)
                        break;

                    destinationSurface.Pixels[destY * destinationSurface.Width + destX] =
                        sourceSurface.Pixels[srcY * sourceSurface.Width + srcX];
                }
            }
        }

        public void BeginDraw(Surface surface)
        {
            Guard.NotNull(surface, nameof(surface));

            if (_drawInProgress)
                throw new PixelEngineDotNetException("Draw is already in progress.");

            _drawInProgress = true;
            _drawTarget = surface;
        }

        private void EnsureDrawInProgress()
        {
            if (!_drawInProgress)
                throw new PixelEngineDotNetException("Draw is not currently in progress.");
        }

        public void EndDraw()
        {
            EnsureDrawInProgress();

            _drawInProgress = false;
            _drawTarget = null;
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

        public void DrawFilledRectangle(Pixel pixel, Rectangle rectangle, PixelMode pixelMode = PixelMode.Overwrite)
        {
            EnsureDrawInProgress();

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    DrawPixel(
                        ref _drawTarget.Pixels[((rectangle.Y + y) * _drawTarget.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        public void DrawFilledRectangle(Func<Point, Pixel> pixelFunc, Rectangle rectangle, PixelMode pixelMode = PixelMode.Overwrite)
        {
            EnsureDrawInProgress();

            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var pixel = pixelFunc(new Point(x, y));
                    DrawPixel(
                        ref _drawTarget.Pixels[((rectangle.Y + y) * _drawTarget.Width) + rectangle.X + x],
                        ref pixel,
                        pixelMode);
                }
            }
        }

        public void DrawSprite(Surface surface, in Vector2 destination, Rectangle? source = null, PixelMode pixelMode = PixelMode.Overwrite)
        {
            Guard.NotNull(surface, nameof(surface));

            EnsureDrawInProgress();

            if (source == null)
                source = new Rectangle(0, 0, surface.Width, surface.Height);

            for (int y = 0; y < source.Value.Height; y++)
            {
                int srcY = source.Value.Y + y;

                if (srcY < 0)
                    continue;

                if (srcY >= surface.Height)
                    break;

                int destY = (int)(destination.Y + y);

                if (destY < 0)
                    continue;

                if (destY >= _drawTarget.Height)
                    break;

                for (int x = 0; x < source.Value.Width; x++)
                {
                    int srcX = source.Value.X + x;

                    if (srcX < 0)
                        continue;

                    if (srcX >= surface.Width)
                        break;

                    int destX = (int)(destination.X + x);

                    if (destX < 0)
                        continue;

                    if (destX >= _drawTarget.Width)
                        break;

                    DrawPixel(
                        ref _drawTarget.Pixels[destY * _drawTarget.Width + destX],
                        ref surface.Pixels[srcY * surface.Width + srcX],
                        pixelMode);
                }
            }
        }
    }
}
