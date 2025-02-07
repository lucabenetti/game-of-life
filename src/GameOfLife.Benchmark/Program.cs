using BenchmarkDotNet.Running;

namespace GameOfLife.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<GameOfLifeBenchmark>();
        }
    }
}
