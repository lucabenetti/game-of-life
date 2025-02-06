using GameOfLife.API.Constants;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GameOfLife.API.Services
{
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
        private readonly Dictionary<int, bool[][]> _stateCache = new();
        private readonly ILogger<GameOfLifeComputeService> _logger;

        public GameOfLifeComputeService(ILogger<GameOfLifeComputeService> logger)
        {
            _logger = logger;
        }

        public bool[][] ComputeNextState(bool[][] board)
        {
            try
            {
                if (board == null || board.Length == 0 || board[0] == null)
                {
                    _logger.LogWarning("ComputeNextState failed: Board is null or empty.");
                    return null;
                }

                int boardHash = GetBoardHash(board);
                if (_stateCache.TryGetValue(boardHash, out var cachedState))
                {
                    _logger.LogInformation("Returning cached board state.");
                    return cachedState;
                }

                int rows = board.Length;
                int cols = board[0].Length;
                bool[][] nextState = new bool[rows][];
                for (int i = 0; i < rows; i++)
                {
                    nextState[i] = new bool[cols];
                }

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
                _logger.LogInformation("Next state computed successfully.");
                return nextState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while computing the next state.");
                return null;
            }
        }

        public int GetBoardHash(bool[][] board)
        {
            try
            {
                if (board == null)
                {
                    _logger.LogWarning("GetBoardHash failed: Board is null.");
                    return 0;
                }

                int hash = GameOfLifeComputeConstants.HashSeed;
                foreach (var row in board)
                {
                    if (row == null) continue;

                    foreach (var cell in row)
                    {
                        hash = hash * GameOfLifeComputeConstants.HashMultiplier + (cell ? GameOfLifeComputeConstants.AliveCellValue : GameOfLifeComputeConstants.DeadCellValue);
                    }
                }

                return hash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating board hash.");
                return 0;
            }
        }

        public int CountAliveNeighbors(bool[][] board, int x, int y)
        {
            try
            {
                if (board == null || board.Length == 0 || board[0] == null)
                {
                    _logger.LogWarning("CountAliveNeighbors failed: Board is null or empty.");
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
                _logger.LogError(ex, "An error occurred while counting alive neighbors.");
                return 0;
            }
        }
    }
}
