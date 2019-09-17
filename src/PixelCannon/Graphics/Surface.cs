using System;
using System.IO;
using System.Numerics;
using PixelCannon.Images;

namespace PixelCannon.Graphics
{
    public class Surface
    {
        public GraphicsContext Graphics { get; }

        public int Width { get; }

        public int Height { get; }

        internal Pixel[] Pixels { get; }

        public int Length => Pixels.Length;

        public ref Pixel this[int i] => ref Pixels[i];

        public ref Pixel this[int x, int y] => ref Pixels[y * Width + x];

        internal Surface(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Pixel[width * height];
        }

        public Surface(GraphicsContext graphics, int width, int height, Pixel[] pixels)
        {
            // At this point and time there is no actual need to have a reference to the GraphicsContext.
            // The reference exists for future proofing in the event that something needs to be created on the GPU
            // in the future.
            Graphics = graphics ??
                throw new ArgumentNullException(nameof(graphics));

            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            var pixelsLength = width * height;
            if (pixels.Length != pixelsLength)
                throw new ArgumentException($"Expected length of pixels array to be {pixelsLength} but was {pixels.Length}.", nameof(pixels));

            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public static Surface FromFile(GraphicsContext graphics, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            using (var file = File.OpenRead(fileName))
                return FromStream(graphics, file);
        }

        public static Surface FromStream(GraphicsContext graphics, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            ImageData imageData = null;

            if (PNG.IsPNGImage(stream))
            {
                imageData = PNG.Load(stream);
            }

            if (imageData == null)
                throw new PixelCannonException("Unknown image format.");

            return new Surface(graphics, imageData.Width, imageData.Height, imageData.Pixels);
        }

        public void SavePng(string fileName)
        {
        }

        public Pixel[] AsArray() => Pixels;

        public void Clear(Pixel pixel)
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = pixel;
            }
        }

        public void Blit(Surface surface, Point destination, Rectangle? source = null)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            if (source == null)
                source = new Rectangle(0, 0, surface.Width, surface.Height);

            for (int y = 0; y < source.Value.Height; y++)
            {
                for (int x = 0; x < source.Value.Width; x++)
                {
                    this[destination.X + x, destination.Y + y] = surface[source.Value.X + x, source.Value.Y + y];
                }
            }
        }

        private void DrawPixel(ref Pixel source, ref Pixel destination, PixelMode pixelMode)
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

        public void FillRect(Pixel pixel, Rectangle rectangle, PixelMode pixelMode = PixelMode.Overwrite)
        {
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    DrawPixel(ref pixel, ref this[rectangle.X + x, rectangle.Y + y], pixelMode);
                }
            }
        }

        public void FillRect(Func<Point, Pixel> pixelFunc, Rectangle rectangle, PixelMode pixelMode = PixelMode.Overwrite)
        {
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var pixel = pixelFunc(new Point(x, y));
                    DrawPixel(ref pixel, ref this[rectangle.X + x, rectangle.Y + y], pixelMode);
                }
            }
        }

        public void DrawSprite(Surface sourceSurface, Vector2 destination, Rectangle? source = null, PixelMode pixelMode = PixelMode.Overwrite)
        {
            if (sourceSurface == null)
                throw new ArgumentNullException(nameof(sourceSurface));

            if (source == null)
                source = new Rectangle(0, 0, sourceSurface.Width, sourceSurface.Height);

            for (int y = 0; y < source.Value.Height; y++)
            {
                for (int x = 0; x < source.Value.Width; x++)
                {
                    DrawPixel(
                        ref sourceSurface[source.Value.X + x, source.Value.Y + y],
                        ref this[(int)(destination.X + x), (int)(destination.Y + y)],
                        pixelMode);
                }
            }
        }
    }
}
