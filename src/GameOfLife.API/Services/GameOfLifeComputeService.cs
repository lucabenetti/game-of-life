using GameOfLife.API.Constants;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.API.Services
{
    /// <summary>
    /// Service responsible for computing the next state of the Game of Life grid.
    /// Implements memoization to cache previously computed states for efficiency.
    /// </summary>
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
        private readonly Dictionary<int, bool[][]> _stateCache = new();
        private readonly ILogger<GameOfLifeComputeService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameOfLifeComputeService"/> class.
        /// </summary>
        /// <param name="logger">Logger for tracking service operations.</param>
        public GameOfLifeComputeService(ILogger<GameOfLifeComputeService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public bool[][] ComputeNextState(bool[][] board)
        {
            try
            {
                if (!IsValidBoard(board))
                {
                    _logger.LogWarning(ValidationMessages.ComputeNextStateFailed, ValidationMessages.InvalidBoardStructure);
                    return null;
                }

                int boardHash = GetBoardHash(board);
                if (_stateCache.TryGetValue(boardHash, out var cachedState))
                {
                    _logger.LogInformation(ValidationMessages.BoardStateCacheUsed);
                    return cachedState;
                }

                int rows = board.Length;
                int cols = board[0].Length;
                bool[][] nextState = CreateEmptyBoard(rows, cols);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int aliveNeighbors = CountAliveNeighbors(board, i, j);
                        nextState[i][j] = board[i][j]
                            ? (aliveNeighbors >= GameOfLifeComputeConstants.MinSurvivalNeighbors && aliveNeighbors <= GameOfLifeComputeConstants.MaxSurvivalNeighbors)
                            : (aliveNeighbors == GameOfLifeComputeConstants.RequiredReproductionNeighbors);
                    }
                }

                _stateCache[boardHash] = nextState;
                _logger.LogInformation(ValidationMessages.BoardStateComputed);
                return nextState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ValidationMessages.StateComputationError);
                return null;
            }
        }

        /// <inheritdoc />
        public int GetBoardHash(bool[][] board)
        {
            try
            {
                if (!IsValidBoard(board))
                {
                    _logger.LogWarning(ValidationMessages.GetBoardHashFailed, ValidationMessages.InvalidBoardStructure);
                    return 0;
                }

                int hash = GameOfLifeComputeConstants.HashSeed;
                foreach (var row in board)
                {
                    foreach (var cell in row)
                    {
                        hash = hash * GameOfLifeComputeConstants.HashMultiplier + (cell ? GameOfLifeComputeConstants.AliveCellValue : GameOfLifeComputeConstants.DeadCellValue);
                    }
                }

                return hash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ValidationMessages.HashGenerationError);
                return 0;
            }
        }

        /// <inheritdoc />
        public int CountAliveNeighbors(bool[][] board, int x, int y)
        {
            try
            {
                if (!IsValidBoard(board))
                {
                    _logger.LogWarning(ValidationMessages.CountAliveNeighborsFailed, ValidationMessages.InvalidBoardStructure);
                    return 0;
                }

                if (x < 0 || y < 0 || x >= board.Length || y >= board[0].Length)
                {
                    _logger.LogWarning(ValidationMessages.CountAliveNeighborsFailed, string.Format(ValidationMessages.InvalidCellCoordinates, x, y));
                    return 0;
                }

                int rows = board.Length;
                int cols = board[0].Length;
                int count = 0;

                for (int i = 0; i < GameOfLifeComputeConstants.TotalNeighbors; i++)
                {
                    int nx = x + GameOfLifeComputeConstants.NeighborXOffsets[i];
                    int ny = y + GameOfLifeComputeConstants.NeighborYOffsets[i];

                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols && board[nx][ny])
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ValidationMessages.CountAliveNeighborsError);
                return 0;
            }
        }

        /// <summary>
        /// Validates if a board is well-formed and has consistent row sizes.
        /// </summary>
        private bool IsValidBoard(bool[][] board)
        {
            if (board == null)
            {
                _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.NullBoard);
                return false;
            }

            if (board.Length == 0)
            {
                _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.EmptyBoard);
                return false;
            }

            if (board.Any(row => row == null))
            {
                _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.NullBoardRows);
                return false;
            }

            int cols = board[0].Length;
            if (cols == 0 || board.Any(row => row.Length != cols))
            {
                _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.InvalidBoardStructure);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates an empty board with the specified dimensions.
        /// </summary>
        private bool[][] CreateEmptyBoard(int rows, int cols)
        {
            return Enumerable.Range(0, rows).Select(_ => new bool[cols]).ToArray();
        }
    }
}
