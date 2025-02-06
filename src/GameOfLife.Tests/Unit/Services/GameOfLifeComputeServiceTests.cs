using GameOfLife.API.Constants;
using GameOfLife.API.Services;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameOfLife.Tests.Unit.Services
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
            int[][] initialState = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            };

            int[][] expectedNextState = new int[][]
            {
                new int[] { 0, 0, 0 },
                new int[] { 1, 0, 1 },
                new int[] { 0, 1, 1 }
            };


            // Act
            int[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

            // Assert
            Assert.Equal(expectedNextState, nextState);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnSameState_ForBlockPattern()
        {
            // Arrange
            int[][] initialState = new int[][]
            {
                new int[] { 0, 0, 0, 0 },
                new int[] { 0, 1, 1, 0 },
                new int[] { 0, 1, 1, 0 },
                new int[] { 0, 0, 0, 0 }
            };


            // Act
            int[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

            // Assert
            Assert.Equal(initialState, nextState);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnCorrectNextState_ForBlinkerPattern()
        {
            // Arrange
            int[][] initialState = new int[][]
            {
                new int[] { 0, 0, 0 },
                new int[] { 1, 1, 1 },
                new int[] { 0, 0, 0 }
            };

            int[][] expectedNextState = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 1, 0 },
                new int[] { 0, 1, 0 }
            };

            // Act
            int[][] nextState = _gameOfLifeComputeService.ComputeNextState(initialState);

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
            int[][] emptyBoard = new int[][] { };

            // Act
            var result = _gameOfLifeComputeService.ComputeNextState(emptyBoard);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ComputeNextState_ShouldReturnNull_WhenBoardHasInconsistentRowSizes()
        {
            // Arrange
            int[][] inconsistentBoard = new int[][]
            {
                new int[] { 1, 0 },
                new int[] { 1, 0, 1 }
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
            int[][] board = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
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
            int[][] board = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
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
            int[][] board = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
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
            int[][] board1 = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 1, 1 },
                new int[] { 1, 1, 1 }
            };

            int[][] board2 = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 1, 1 },
                new int[] { 1, 1, 1 }
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
            int[][] board1 = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 0, 1 },
                new int[] { 1, 1, 1 }
            };

            int[][] board2 = new int[][]
            {
                new int[] { 0, 1, 0 },
                new int[] { 0, 1, 1 },
                new int[] { 1, 1, 1 }
            };


            // Act
            int hash1 = _gameOfLifeComputeService.GetBoardHash(board1);
            int hash2 = _gameOfLifeComputeService.GetBoardHash(board2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}
