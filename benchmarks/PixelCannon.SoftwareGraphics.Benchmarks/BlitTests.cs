using BenchmarkDotNet.Attributes;
using PixelEngineDotNet;
using PixelEngineDotNet.Graphics;
using PixelEngineDotNet.Platforms.Software;

namespace PixelCannon.SoftwareGraphics.Benchmarks
{
    public class BlitTests
    {
        private Win32GameWindow _window;
        private SoftwareGraphicsContext _graphics;
        private Surface _surface;

        public BlitTests()
        {
            _window = new Win32GameWindow(new Size(1024, 768));
            _graphics = new SoftwareGraphicsContext(_window, new Size(1024, 768));
            _surface = new Surface(_graphics, 1024, 768);

            _graphics.BeginDraw(_surface);
            _graphics.DrawFilledRectangle(new Pixel(255, 255, 255, 255), new Rectangle(0, 0, _surface.Width, _surface.Height));
            _graphics.EndDraw();
        }

        [Benchmark]
        public void Blit() => _graphics.Blit(_graphics.BackBuffer, _surface, Point.Zero);
    }
}
