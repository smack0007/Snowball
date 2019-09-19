using System;

namespace PixelEngineDotNet.Graphics
{
    public partial class GraphicsContext
    {
        public GameWindow Window { get; }

        public Surface BackBuffer { get; private set; }

        private readonly uint _backBufferTexture;

        public GraphicsContext(GameWindow window, Size backBufferSize)
        {
            Window = window ??
                throw new ArgumentNullException(nameof(window));

            PlatformInitialize(window, backBufferSize);
        }

        public void Present()
        {
            PlatformPresent();
        }
    }
}
