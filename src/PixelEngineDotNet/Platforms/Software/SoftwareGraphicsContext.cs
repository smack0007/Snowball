using PixelEngineDotNet.Graphics;
using static PixelEngineDotNet.Platforms.OpenGL.GL;
using static PixelEngineDotNet.Platforms.Win32.WGL;

namespace PixelEngineDotNet.Platforms.Software
{
    public class SoftwareGraphicsContext : GraphicsContext
    {
        private uint _backBufferTexture;

        public SoftwareGraphicsContext(GameWindow window, Size backBufferSize)
            : base(window, backBufferSize)
        {
        }

        protected override PlatformInitializeResult PlatformInitialize(Size backBufferSize)
        {
            wglInit(Window.Handle, 2, 1);
            glInit(wglGetProcAddress);

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
                fixed (void* surfacePtr = backBuffer.Pixels)
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
                fixed (void* surfacePtr = BackBuffer.Pixels)
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
    }
}
