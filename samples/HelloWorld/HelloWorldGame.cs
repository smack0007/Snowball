using PixelEngineDotNet;
using PixelEngineDotNet.Graphics;
using System;
using System.Numerics;

namespace HelloWorld
{
    public class HelloWorldGame : Game
    {
        private readonly Random _random = new Random();
        private readonly byte[] _randomBuffer = new byte[3];

        private Surface _surface;

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
            _surface = Surface.FromFile(Graphics, "Snowball.png");
            _random.NextBytes(_randomBuffer);
        }

        protected override void Draw()
        {
            var center = new Vector2(Graphics.BackBuffer.Width / 2.0f, Graphics.BackBuffer.Height / 2.0f);
            var totalDistance = Vector2.Distance(Vector2.Zero, center);

            Graphics.BackBuffer.FillRect((pos) =>
            {
                float factor = 1.0f - Vector2.Distance(pos.ToVector2(), center) / totalDistance;
                return new Pixel
                {
                    R = unchecked((byte)(_randomBuffer[0] * factor)),
                    G = unchecked((byte)(_randomBuffer[1] * factor)),
                    B = unchecked((byte)(_randomBuffer[2] * factor)),
                    A = 255
                };
            }, new Rectangle(0, 0, Graphics.BackBuffer.Width, Graphics.BackBuffer.Height));

            Graphics.BackBuffer.DrawSprite(_surface, Vector2.Zero, pixelMode: PixelMode.AlphaBlend);
        }
    }
}
