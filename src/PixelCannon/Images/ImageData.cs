namespace PixelCannon.Images
{
    public class ImageData
    {
        public int Width { get; }

        public int Height { get; }

        public Pixel[] Pixels { get; }

        public ImageData(int width, int height, Pixel[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
    }
}
