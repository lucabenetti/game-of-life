using Moq;
using GameOfLife.API.Configurations;
using GameOfLife.API.Constants;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameOfLife.Tests.Services
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
            var board = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
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
            bool[][] board = new bool[][] { };

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
            bool[][] board = new bool[][]
            {
                new bool[] { false, true },
                new bool[] { false, true, false }
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
            bool[][] board = new bool[][]
            {
                new bool[] { false, false },
                new bool[] { false, false }
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
            var initialState = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            var nextState = new bool[][]
            {
                new bool[] { false, false, true },
                new bool[] { true, false, true },
                new bool[] { false, true, true }
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
            var initialState = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
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
    }
}
