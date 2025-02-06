using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLife.API.Services
{
    /// <summary>
    /// Service responsible for managing Game of Life board states, computing next states, 
    /// and determining the final stable or repeating state.
    /// </summary>
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IGameOfLifeRepository _repository;
        private readonly IGameOfLifeComputeService _computeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameOfLifeService"/> class.
        /// </summary>
        /// <param name="repository">The repository responsible for storing and retrieving board states.</param>
        /// <param name="computeService">The compute service responsible for generating next states.</param>
        public GameOfLifeService(IGameOfLifeRepository repository, IGameOfLifeComputeService computeService)
        {
            _repository = repository;
            _computeService = computeService;
        }

        /// <inheritdoc />
        public async Task<Guid> UploadBoard(bool[][] board)
        {
            var gameBoard = new GameOfLifeBoard(board);
            await _repository.SaveBoard(gameBoard);
            return gameBoard.Id;
        }

        /// <inheritdoc />
        public async Task<bool[][]> GetNextState(Guid id)
        {
            var gameBoard = await _repository.GetBoard(id);
            if (gameBoard == null) return null;

            // If all cells are dead, return the board immediately
            if (IsBoardEmpty(gameBoard.Board))
            {
                return gameBoard.Board;
            }

            var nextState = _computeService.ComputeNextState(gameBoard.Board);
            gameBoard.Board = nextState;
            await _repository.SaveBoard(gameBoard);
            return nextState;
        }

        /// <inheritdoc />
        public async Task<FinalStateResultDto> GetFinalState(Guid id, int maxAttempts)
        {
            var gameBoard = await _repository.GetBoard(id);
            if (gameBoard == null)
            {
                return new FinalStateResultDto { Board = null, Completed = false };
            }

            var seenStates = new HashSet<int>();

            for (int i = 0; i < maxAttempts; i++)
            {
                int hash = _computeService.GetBoardHash(gameBoard.Board);
                if (seenStates.Contains(hash))
                {
                    return new FinalStateResultDto { Board = gameBoard.Board, Completed = true };
                }

                seenStates.Add(hash);
                gameBoard.Board = _computeService.ComputeNextState(gameBoard.Board);
            }

            return new FinalStateResultDto { Board = null, Completed = false };
        }

        /// <summary>
        /// Checks whether the board is completely empty (all cells are dead).
        /// </summary>
        /// <param name="board">The board state as a 2D boolean array.</param>
        /// <returns><c>true</c> if all cells are dead; otherwise, <c>false</c>.</returns>
        private bool IsBoardEmpty(bool[][] board)
        {
            foreach (var row in board)
            {
                if (row.Any(cell => cell))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
