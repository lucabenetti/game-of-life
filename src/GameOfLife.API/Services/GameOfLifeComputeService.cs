﻿using GameOfLife.API.Constants;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.API.Services
{
    /// <summary>
    /// Service responsible for computing the next state of the Game of Life grid.
    /// </summary>
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
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
        public int[][] ComputeNextState(int[][] board)
        {
            try
            {
                if (!IsValidBoard(board))
                {
                    _logger.LogWarning(ValidationMessages.ComputeNextStateFailed, ValidationMessages.InvalidBoardStructure);
                    return null;
                }

                int rows = board.Length;
                int cols = board[0].Length;
                int[][] nextState = CreateEmptyBoard(rows, cols);

                // Parallel execution for each row
                Parallel.For(0, rows, i =>
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int aliveNeighbors = CountAliveNeighbors(board, i, j);
                        nextState[i][j] = board[i][j] == 1
                            ? (aliveNeighbors >= GameOfLifeComputeConstants.MinSurvivalNeighbors && aliveNeighbors <= GameOfLifeComputeConstants.MaxSurvivalNeighbors ? 1 : 0)
                            : (aliveNeighbors == GameOfLifeComputeConstants.RequiredReproductionNeighbors ? 1 : 0);
                    }
                });

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
        public int GetBoardHash(int[][] board)
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
                        hash = hash * GameOfLifeComputeConstants.HashMultiplier + (cell == 1 ? GameOfLifeComputeConstants.AliveCellValue : GameOfLifeComputeConstants.DeadCellValue);
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
        public int CountAliveNeighbors(int[][] board, int x, int y)
        {
            try
            {
                if (!IsValidBoard(board))
                {
                    _logger.LogWarning(ValidationMessages.CountAliveNeighborsFailed, ValidationMessages.InvalidBoardStructure);
                    return 0;
                }

                int rows = board.Length;
                int cols = board[0].Length;

                if (x < 0 || x >= rows || y < 0 || y >= cols)
                {
                    _logger.LogWarning(ValidationMessages.CountAliveNeighborsFailed, string.Format(ValidationMessages.InvalidCellCoordinates, x, y));
                    return 0;
                }

                int count = 0;

                for (int i = 0; i < GameOfLifeComputeConstants.TotalNeighbors; i++)
                {
                    int nx = x + GameOfLifeComputeConstants.NeighborXOffsets[i];
                    int ny = y + GameOfLifeComputeConstants.NeighborYOffsets[i];

                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols && board[nx][ny] == 1)
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
        private bool IsValidBoard(int[][] board)
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
        private int[][] CreateEmptyBoard(int rows, int cols)
        {
            return Enumerable.Range(0, rows).Select(_ => new int[cols]).ToArray();
        }
    }
}
