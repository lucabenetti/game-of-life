using Moq;
using GameOfLife.API.Configurations;
using GameOfLife.API.Constants;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameOfLife.Tests.Unit.Services
{
    public class GameOfLifeServiceTests
    {
        private readonly Mock<IGameOfLifeRepository> _repositoryMock;
        private readonly Mock<IGameOfLifeComputeService> _computeServiceMock;
        private readonly Mock<ILogger<GameOfLifeService>> _loggerMock;
        private readonly GameOfLifeService _gameOfLifeService;

        public GameOfLifeServiceTests()
        {
            _repositoryMock = new Mock<IGameOfLifeRepository>();
            _computeServiceMock = new Mock<IGameOfLifeComputeService>();
            _loggerMock = new Mock<ILogger<GameOfLifeService>>();

            var settings = Options.Create(new GameOfLifeSettings
            {
                DefaultMaxAttempts = 200,
                MaxAllowedAttempts = 1000,
                MaxBoardWidth = 500,
                MaxBoardHeight = 500
            });

            _gameOfLifeService = new GameOfLifeService(
                _repositoryMock.Object,
                _computeServiceMock.Object,
                settings,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task UploadBoard_ShouldSaveBoardAndReturnId()
        {
            // Arrange
            var board = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            };


            // Act
            var result = await _gameOfLifeService.UploadBoard(board);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.SaveBoard(It.IsAny<GameOfLifeBoard>()), Times.Once);
        }

        [Fact]
        public async Task UploadBoard_ShouldFail_WhenBoardIsNull()
        {
            // Act
            var result = await _gameOfLifeService.UploadBoard(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.NullBoard, result.ErrorMessage);
        }

        [Fact]
        public async Task UploadBoard_ShouldFail_WhenBoardIsEmpty()
        {
            // Arrange
            int[][] board = new int[][] { };

            // Act
            var result = await _gameOfLifeService.UploadBoard(board);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.EmptyBoard, result.ErrorMessage);
        }

        [Fact]
        public async Task UploadBoard_ShouldFail_WhenBoardHasInconsistentRowSizes()
        {
            // Arrange
            int[][] board = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 0, 1, 0 }
            };


            // Act
            var result = await _gameOfLifeService.UploadBoard(board);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Row", result.ErrorMessage);
        }

        [Fact]
        public async Task UploadBoard_ShouldFail_WhenBoardHasNoLiveCells()
        {
            // Arrange
            int[][] board = new int[][]
            {
                new int[] { 0, 0 },
                new int[] { 0, 0 }
            };


            // Act
            var result = await _gameOfLifeService.UploadBoard(board);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.EmptyBoard, result.ErrorMessage);
        }

        [Fact]
        public async Task GetNextState_ShouldReturnNextState_WhenBoardExists()
        {
            // Arrange
            var initialState = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            };

            var nextState = new int[][]
            {
                new int[] { 0, 0, 1 },
                new int[] { 1, 0, 1 },
                new int[] { 0, 1, 1 }
            };

            var gameBoard = new GameOfLifeBoard(initialState);
            _repositoryMock.Setup(r => r.GetBoard(It.IsAny<Guid>())).ReturnsAsync(gameBoard);
            _computeServiceMock.Setup(c => c.ComputeNextState(initialState)).Returns(nextState);

            // Act
            var result = await _gameOfLifeService.GetNextState(Guid.NewGuid());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(nextState, result.Value);
            _repositoryMock.Verify(r => r.SaveBoard(It.Is<GameOfLifeBoard>(b => b.Board == nextState)), Times.Once);
        }

        [Fact]
        public async Task GetNextState_ShouldFail_WhenBoardDoesNotExist()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetBoard(It.IsAny<Guid>())).ReturnsAsync((GameOfLifeBoard?)null);

            // Act
            var result = await _gameOfLifeService.GetNextState(Guid.NewGuid());

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.BoardNotFound, result.ErrorMessage);
        }

        [Fact]
        public async Task GetNextState_ShouldFail_WhenGuidIsEmpty()
        {
            // Act
            var result = await _gameOfLifeService.GetNextState(Guid.Empty);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.InvalidGuid, result.ErrorMessage);
        }

        [Fact]
        public async Task GetFinalState_ShouldReturnCompleted_WhenLoopDetected()
        {
            // Arrange
            var initialState = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            };

            var gameBoard = new GameOfLifeBoard(initialState);
            _repositoryMock.Setup(r => r.GetBoard(It.IsAny<Guid>())).ReturnsAsync(gameBoard);
            _computeServiceMock.Setup(c => c.GetBoardHash(initialState)).Returns(12345);
            _computeServiceMock.Setup(c => c.ComputeNextState(initialState)).Returns(initialState);

            // Act
            var result = await _gameOfLifeService.GetFinalState(Guid.NewGuid(), 5);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Completed);
            Assert.Equal(initialState, result.Value.Board);
        }

        [Fact]
        public async Task GetFinalState_ShouldFail_WhenMaxAttemptsExceedsLimit()
        {
            // Act
            var result = await _gameOfLifeService.GetFinalState(Guid.NewGuid(), 5000);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task GetFinalState_ShouldFail_WhenGuidIsEmpty()
        {
            // Act
            var result = await _gameOfLifeService.GetFinalState(Guid.Empty, 5);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.InvalidGuid, result.ErrorMessage);
        }

        [Fact]
        public async Task UploadBoard_ShouldReturnFailure_WhenBoardContainsInvalidValues()
        {
            int[][] board = new int[][]
            {
                new int[] { 1, 0, 2 }, // 2 is not valid
                new int[] { 0, -1, 1 }, // -1 is not valid
                new int[] { 1, 0, 1 }
            };

            var result = await _gameOfLifeService.UploadBoard(board);

            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.InvalidCellValue, result.ErrorMessage);
        }

        [Fact]
        public async Task UploadBoard_ShouldReturnFailure_WhenBoardIsAllDeadCells()
        {
            int[][] board = new int[][]
            {
                new int[] { 0, 0, 0 },
                new int[] { 0, 0, 0 },
                new int[] { 0, 0, 0 }
            };

            var result = await _gameOfLifeService.UploadBoard(board);

            Assert.False(result.IsSuccess);
            Assert.Equal(ValidationMessages.EmptyBoard, result.ErrorMessage);
        }
    }
}
