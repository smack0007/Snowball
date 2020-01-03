using System;
using System.Diagnostics;
using PixelEngineDotNet.Graphics;
using PixelEngineDotNet.Platforms;
using PixelEngineDotNet.Platforms.Software;

namespace PixelEngineDotNet
{
    public class Game : IDisposable
    {
        private const float TimeBetweenFrames = 1000.0f / 60.0f;

        private Stopwatch _stopwatch;
        private float _lastElapsed;
        private float _elapsedSinceLastFrame;

        private float _fpsElapsed;

        public GameWindow Window { get; }

        public GraphicsContext Graphics { get; }

        public int FramesPerSecond { get; private set; }

        public Game(
            Size windowSize,
            Size? backBufferSize = null
        )
        {
            if (backBufferSize == null)
                backBufferSize = windowSize;

            Window = PlatformFactory.CreateGameWindow(windowSize);

            Graphics = PlatformFactory.CreateGraphicsContext(Window, backBufferSize.Value);

            _stopwatch = new Stopwatch();
        }

        public void Dispose()
        {
            Window.Dispose();
        }

        public void Run()
        {
            _stopwatch.Restart();

            Window.Run(Tick);
        }

        private void Tick()
        {
            float currentElapsed = (float)_stopwatch.Elapsed.TotalMilliseconds;
            float deltaElapsed = currentElapsed - _lastElapsed;
            _elapsedSinceLastFrame += deltaElapsed;
            _lastElapsed = currentElapsed;

            if (_elapsedSinceLastFrame >= TimeBetweenFrames)
            {
                Update(_elapsedSinceLastFrame);
                Draw();

                Graphics.Present();

                _elapsedSinceLastFrame -= TimeBetweenFrames;
                FramesPerSecond++;
            }

            _fpsElapsed += deltaElapsed;

            if (_fpsElapsed >= 1000.0f)
            {
                _fpsElapsed -= 1000.0f;
                Window.Title = $"FPS: {FramesPerSecond}";
                FramesPerSecond = 0;
            }
        }

        protected virtual void Update(float elapsed)
        {
        }

        protected virtual void Draw()
        {
        }
    }
}
