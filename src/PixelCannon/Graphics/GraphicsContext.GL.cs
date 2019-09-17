#if GL

using System;
using static PixelCannon.Graphics.GL;
using static PixelCannon.Graphics.WGL;

namespace PixelCannon.Graphics
{
    public partial class GraphicsContext
    {
        private void PlatformInitialize(GameWindow window, Size backBufferSize)
        {
            wglInit(window.Handle, 2, 1);
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

            BackBuffer = new Surface(backBufferSize.Width, backBufferSize.Height);

            unsafe
            {
                fixed (void* surfacePtr = BackBuffer.Pixels)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, backBufferSize.Width, backBufferSize.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, surfacePtr);
                }
            }
        }

        private void PlatformPresent()
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

#endif
