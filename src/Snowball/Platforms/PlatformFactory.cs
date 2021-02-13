using Snowball.Graphics;
using Snowball.Platforms.Software;

namespace Snowball.Platforms
{
    public static class PlatformFactory
    {
        public static GameWindow CreateGameWindow(Size windowSize)
        {
            return new Win32GameWindow(windowSize);
        }

        public static GraphicsContext CreateGraphicsContext(GameWindow window, Size backBufferSize)
        {
            return new SoftwareGraphicsContext(window, backBufferSize);
        }
    }
}
