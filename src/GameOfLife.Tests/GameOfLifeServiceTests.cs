using Moq;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;

namespace GameOfLife.Tests
{
    public class GameOfLifeServiceTests
    {
        private readonly Mock<IGameOfLifeRepository> _repositoryMock;
        private readonly Mock<IGameOfLifeComputeService> _computeServiceMock;
        private readonly GameOfLifeService _gameOfLifeService;

        public GameOfLifeServiceTests()
        {
            _repositoryMock = new Mock<IGameOfLifeRepository>();
            _computeServiceMock = new Mock<IGameOfLifeComputeService>();
            _gameOfLifeService = new GameOfLifeService(_repositoryMock.Object, _computeServiceMock.Object);
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
            _repositoryMock.Verify(r => r.SaveBoard(It.IsAny<GameOfLifeBoard>()), Times.Once);
            Assert.NotEqual(Guid.Empty, result);
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
            Assert.Equal(nextState, result);
            _repositoryMock.Verify(r => r.SaveBoard(It.Is<GameOfLifeBoard>(b => b.Board == nextState)), Times.Once);
        }

        [Fact]
        public async Task GetNextState_ShouldReturnNull_WhenBoardDoesNotExist()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetBoard(It.IsAny<Guid>())).ReturnsAsync((GameOfLifeBoard?)null);

            // Act
            var result = await _gameOfLifeService.GetNextState(Guid.NewGuid());

            // Assert
            Assert.Null(result);
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
            Assert.True(result.Completed);
            Assert.Equal(initialState, result.Board);
        }

        [Fact]
        public async Task GetFinalState_ShouldReturnNullBoard_WhenMaxAttemptsExceeded()
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
            _computeServiceMock.SetupSequence(c => c.ComputeNextState(It.IsAny<bool[][]>()))
                .Returns(nextState)
                .Returns(initialState);
            _computeServiceMock.SetupSequence(c => c.GetBoardHash(It.IsAny<bool[][]>()))
                .Returns(12345)
                .Returns(54321);

            // Act
            var result = await _gameOfLifeService.GetFinalState(Guid.NewGuid(), 1);

            // Assert
            Assert.False(result.Completed);
            Assert.Null(result.Board);
        }

        [Fact]
        public async Task GetFinalState_ShouldReturnNull_WhenBoardDoesNotExist()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetBoard(It.IsAny<Guid>())).ReturnsAsync((GameOfLifeBoard?)null);

            // Act
            var result = await _gameOfLifeService.GetFinalState(Guid.NewGuid(), 5);

            // Assert
            Assert.False(result.Completed);
            Assert.Null(result.Board);
        }
    }
}
