﻿using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services.Interfaces;

namespace GameOfLife.API.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IGameOfLifeRepository _repository;
        private readonly IGameOfLifeComputeService _computeService;

        public GameOfLifeService(IGameOfLifeRepository repository, IGameOfLifeComputeService computeService)
        {
            _repository = repository;
            _computeService = computeService;
        }

        public async Task<Guid> UploadBoard(HashSet<(int, int)> board)
        {
            var gameBoard = new GameOfLifeBoard(board);
            await _repository.SaveBoard(gameBoard);
            return gameBoard.Id;
        }

        public async Task<HashSet<(int, int)>> GetNextState(Guid id)
        {
            var gameBoard = await _repository.GetBoard(id);
            if (gameBoard == null) return null;

            var nextState = _computeService.ComputeNextState(gameBoard.Board);
            gameBoard.Board = nextState;
            await _repository.SaveBoard(gameBoard);
            return nextState;
        }

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
    }
}
