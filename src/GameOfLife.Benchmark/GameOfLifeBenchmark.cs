using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using GameOfLife.API.Configurations;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GameOfLife.Benchmark
{
    [MemoryDiagnoser] // Enables memory tracking
    [Config(typeof(BenchmarkConfig))] // Use in-process execution to avoid antivirus interference
    public class GameOfLifeBenchmark
    {
        private class BenchmarkConfig : ManualConfig
        {
            public BenchmarkConfig()
            {
                AddJob(Job.Default.WithToolchain(InProcessNoEmitToolchain.Instance)); // Run inside the same process
            }
        }

        private GameOfLifeComputeService _computeService;
        private GameOfLifeService _gameOfLifeService;
        private int[][] _testBoard;
        private Guid _boardId;

        [GlobalSetup]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<GameOfLifeComputeService>>();
            _computeService = new GameOfLifeComputeService(loggerMock.Object);

            var gameServiceLoggerMock = new Mock<ILogger<GameOfLifeService>>();
            var repositoryMock = new Mock<IGameOfLifeRepository>();
            var computeServiceMock = new Mock<IGameOfLifeComputeService>();

            var settingsMock = new Mock<IOptions<GameOfLifeSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new GameOfLifeSettings
            {
                MaxBoardWidth = 1000,
                MaxBoardHeight = 1000,
                MaxAllowedAttempts = 500
            });

            _gameOfLifeService = new GameOfLifeService(repositoryMock.Object, computeServiceMock.Object, settingsMock.Object, gameServiceLoggerMock.Object);

            _testBoard = GenerateRandomBoard(100, 100);

            // Upload board to create a valid boardId for GetNextState and GetFinalState
            var uploadResult = _gameOfLifeService.UploadBoard(_testBoard).Result;
            if (uploadResult.IsSuccess)
            {
                _boardId = uploadResult.Value;
            }
        }

        [Benchmark]
        public void ComputeNextState()
        {
            _computeService.ComputeNextState(_testBoard);
        }

        private int[][] GenerateRandomBoard(int rows, int cols)
        {
            Random rand = new Random(42);
            return Enumerable.Range(0, rows)
                .Select(_ => Enumerable.Range(0, cols)
                    .Select(_ => rand.Next(2))
                    .ToArray())
                .ToArray();
        }
    }
}

