using System;
using BenchmarkDotNet.Running;

namespace PixelCannon.SoftwareGraphics.Benchmarks
{
    public class Program
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<BlitTests>();
            Console.WriteLine(summary);
        }
    }
}
