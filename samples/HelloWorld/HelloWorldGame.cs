using Snowball;
using Snowball.Graphics;
using System;
using System.Numerics;

namespace HelloWorld
{
    public class HelloWorldGame : Game
    {
        private const int SNOWBALL_COUNT = 300;
        private const float SNOWBALL_MOVEMENT_FACTOR = 10.0f;

        private Vector2 _center;
        private Surface _backgroundSurface;
        private Surface _snowballSurface;
        private Vector2[] _snowballVelocities;
        private Vector2[] _snowballPositions;

        public static void Main()
        {
            using (var game = new HelloWorldGame())
                game.Run();
        }

        HelloWorldGame()
            : base(
                windowSize: new Size(1024, 960),
                backBufferSize: new Size(256, 240))
        {
            var randomBuffer = new byte[3];

            var random = new Random();
            random.NextBytes(randomBuffer);

            _center = new Vector2(Graphics.BackBuffer.Width / 2.0f, Graphics.BackBuffer.Height / 2.0f);
            var totalDistance = Vector2.Distance(Vector2.Zero, _center);

            _backgroundSurface = new Surface(Graphics, Graphics.BackBuffer.Width, Graphics.BackBuffer.Height);
            Graphics.BeginDraw(_backgroundSurface);
            Graphics.DrawFilledRectangle((pos) =>
            {
                float factor = 1.0f - Vector2.Distance(pos.ToVector2(), _center) / totalDistance;
                return new Pixel
                {
                    R = unchecked((byte)(randomBuffer[0] * factor)),
                    G = unchecked((byte)(randomBuffer[1] * factor)),
                    B = unchecked((byte)(randomBuffer[2] * factor)),
                    A = 255
                };
            }, new Rectangle(0, 0, Graphics.BackBuffer.Width, Graphics.BackBuffer.Height));
            Graphics.EndDraw();

            _snowballSurface = Surface.FromFile(Graphics, "Snowball.png");
            _snowballVelocities = new Vector2[SNOWBALL_COUNT];
            for (int i = 0; i < SNOWBALL_COUNT; i++)
                _snowballVelocities[i] = new Vector2(
                    1.0f * (random.Next(101) / 100.0f) * (random.Next(101) % 2 == 0 ? 1.0f : -1.0f),
                    1.0f * (random.Next(101) / 100.0f) * (random.Next(101) % 2 == 0 ? 1.0f : -1.0f));

            _snowballPositions = new Vector2[SNOWBALL_COUNT];
            for (int i = 0; i < SNOWBALL_COUNT; i++)
                _snowballPositions[i] = _center;
        }

        protected override void Update(float elapsed)
        {
            for (int i = 0; i < SNOWBALL_COUNT; i++)
            {
                _snowballPositions[i].X += _snowballVelocities[i].X * SNOWBALL_MOVEMENT_FACTOR;
                _snowballPositions[i].Y += _snowballVelocities[i].Y * SNOWBALL_MOVEMENT_FACTOR;

                if (_snowballPositions[i].X < 0 ||
                    _snowballPositions[i].Y < 0 ||
                    _snowballPositions[i].X > Graphics.BackBuffer.Width ||
                    _snowballPositions[i].Y > Graphics.BackBuffer.Height)
                {
                    _snowballPositions[i] = _center;
                }
            }
        }

        protected override void Draw()
        {
            Graphics.Clear(Graphics.BackBuffer, new Pixel(255, 0, 255, 255));

            Graphics.Blit(Graphics.BackBuffer, _backgroundSurface, Point.Zero);

            Graphics.BeginDraw(Graphics.BackBuffer);

            for (int i = 0; i < SNOWBALL_COUNT; i++)
            {
                Graphics.DrawSprite(
                    _snowballSurface,
                    _snowballPositions[i],
                    pixelMode: PixelMode.AlphaBlend);
            }

            Graphics.EndDraw();
        }
    }
}
