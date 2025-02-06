using GameOfLife.API.Constants;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameOfLife.Tests.Services
{
    public class GameOfLifeComputeServiceTests
    {
        private readonly GameOfLifeComputeService _gameOfLifeComputeService;
        private readonly Mock<ILogger<GameOfLifeComputeService>> _loggerMock;

        public GameOfLifeComputeServiceTests()
        {
            _loggerMock = new Mock<ILogger<GameOfLifeComputeService>>();
            _gameOfLifeComputeService = new GameOfLifeComputeService(_loggerMock.Object);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnCorrectNextState_ForGliderPattern()
        {
            // Arrange
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            bool[][] expectedNextState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { true, false, true },
                new bool[] { false, true, true }
            };

            // Act
            bool[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

            // Assert
            Assert.Equal(expectedNextState, nextState);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnSameState_ForBlockPattern()
        {
            // Arrange
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false, false },
                new bool[] { false, true, true, false },
                new bool[] { false, true, true, false },
                new bool[] { false, false, false, false }
            };

            // Act
            bool[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

            // Assert
            Assert.Equal(initialState, nextState);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnCorrectNextState_ForBlinkerPattern()
        {
            // Arrange
            bool[][] initialState = new bool[][]
            {
                new bool[] { false, false, false },
                new bool[] { true, true, true },
                new bool[] { false, false, false }
            };

            bool[][] expectedNextState = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, true, false },
                new bool[] { false, true, false }
            };

            // Act
            bool[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

            // Assert
            Assert.Equal(expectedNextState, nextState);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnNull_WhenBoardIsNull()
        {
            // Act
            var result = _gameOfLifeComputeService.ComputeNextState(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnNull_WhenBoardIsEmpty()
        {
            // Arrange
            bool[][] emptyBoard = new bool[][] { };

            // Act
            var result = _gameOfLifeComputeService.ComputeNextState(emptyBoard);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnNull_WhenBoardHasInconsistentRowSizes()
        {
            // Arrange
            bool[][] inconsistentBoard = new bool[][]
            {
                new bool[] { true, false },
                new bool[] { true, false, true }
            };

            // Act
            var result = _gameOfLifeComputeService.ComputeNextState(inconsistentBoard);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnCachedState_WhenStateAlreadyComputed()
        {
            // Arrange
            bool[][] board = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            // Act
            var firstCall = _gameOfLifeComputeService.ComputeNextState(board);
            var secondCall = _gameOfLifeComputeService.ComputeNextState(board);

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public void CountAliveNeighbors_ShouldReturnCorrectCount()
        {
            // Arrange
            bool[][] board = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            // Act & Assert
            Assert.Equal(1, _gameOfLifeComputeService.CountAliveNeighbors(board, 0, 0));
            Assert.Equal(3, _gameOfLifeComputeService.CountAliveNeighbors(board, 1, 2));
            Assert.Equal(2, _gameOfLifeComputeService.CountAliveNeighbors(board, 2, 2));
        }

        [Fact]
        public void CountAliveNeighbors_ShouldReturnZero_WhenCellIsOutOfBounds()
        {
            // Arrange
            bool[][] board = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            // Act
            int neighbors = _gameOfLifeComputeService.CountAliveNeighbors(board, -1, 2); // Out of bounds

            // Assert
            Assert.Equal(0, neighbors);
        }

        [Fact]
        public void GetBoardHash_ShouldReturnSameHash_ForIdenticalBoards()
        {
            // Arrange
            bool[][] board1 = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            bool[][] board2 = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            // Act
            int hash1 = _gameOfLifeComputeService.GetBoardHash(board1);
            int hash2 = _gameOfLifeComputeService.GetBoardHash(board2);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetBoardHash_ShouldReturnDifferentHash_ForDifferentBoards()
        {
            // Arrange
            bool[][] board1 = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, false, true },
                new bool[] { true, true, true }
            };

            bool[][] board2 = new bool[][]
            {
                new bool[] { false, true, false },
                new bool[] { false, true, true },
                new bool[] { true, true, true }
            };

            // Act
            int hash1 = _gameOfLifeComputeService.GetBoardHash(board1);
            int hash2 = _gameOfLifeComputeService.GetBoardHash(board2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}
