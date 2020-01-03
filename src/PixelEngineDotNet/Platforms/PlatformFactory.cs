using System;
using System.Collections.Generic;
using System.Text;
using PixelEngineDotNet.Graphics;
using PixelEngineDotNet.Platforms.Software;

namespace PixelEngineDotNet.Platforms
{
    public static class PlatformFactory
    {
        public static GraphicsContext CreateGraphicsContext(GameWindow window, Size backBufferSize)
        {
            return new SoftwareGraphicsContext(window, backBufferSize);
        }
    }
}
