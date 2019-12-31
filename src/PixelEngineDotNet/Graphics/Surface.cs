using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using PixelEngineDotNet.Images;

namespace PixelEngineDotNet.Graphics
{
    public class Surface
    {
        public GraphicsContext Graphics { get; }

        public int Width { get; }

        public int Height { get; }

        internal Pixel[] Pixels { get; }

        internal Surface(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Pixel[width * height];
        }

        public Surface(GraphicsContext graphics, int width, int height, Pixel[] pixels = null)
        {
            // At this point and time there is no actual need to have a reference to the GraphicsContext.
            // The reference exists for future proofing in the event that something needs to be created on the GPU
            // in the future.
            Graphics = graphics ??
                throw new ArgumentNullException(nameof(graphics));

            if (pixels == null)
                pixels = new Pixel[width * height];

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
                throw new PixelEngineDotNetException("Unknown image format.");

            return new Surface(graphics, imageData.Width, imageData.Height, imageData.Pixels);
        }

        public void SavePng(string fileName)
        {
        }

        public Pixel[] GetPixels() => Pixels;
    }
}
