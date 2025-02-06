using GameOfLife.API.Configurations;
using GameOfLife.API.Constants;
using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLife.API.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IGameOfLifeRepository _repository;
        private readonly IGameOfLifeComputeService _computeService;
        private readonly GameOfLifeSettings _settings;
        private readonly ILogger<GameOfLifeService> _logger;

        private const int MinAllowedAttempts = 1;

        public GameOfLifeService(
            IGameOfLifeRepository repository,
            IGameOfLifeComputeService computeService,
            IOptions<GameOfLifeSettings> settings,
            ILogger<GameOfLifeService> logger)
        {
            _repository = repository;
            _computeService = computeService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Result<Guid>> UploadBoard(bool[][] board)
        {
            try
            {
                if (board == null || board.Length == 0)
                {
                    _logger.LogWarning("Upload failed: Board is null or empty.");
                    return Result<Guid>.Failure(ValidationMessages.EmptyBoard);
                }

                int rows = board.Length;
                int cols = board[0].Length;

                if (rows > _settings.MaxBoardHeight || cols > _settings.MaxBoardWidth)
                {
                    _logger.LogWarning("Upload failed: Board size exceeds limit. Height: {rows}, Width: {cols}", rows, cols);
                    return Result<Guid>.Failure($"{ValidationMessages.BoardSizeExceeded} Max allowed size is {_settings.MaxBoardHeight} x {_settings.MaxBoardWidth}.");
                }

                for (int i = 0; i < rows; i++)
                {
                    if (board[i] == null || board[i].Length != cols)
                    {
                        _logger.LogWarning("Upload failed: Row {row} is inconsistent in size.", i);
                        return Result<Guid>.Failure(string.Format(ValidationMessages.InvalidBoardStructure, i));
                    }
                }

                var gameBoard = new GameOfLifeBoard(board);
                await _repository.SaveBoard(gameBoard);

                _logger.LogInformation("Board uploaded successfully. ID: {id}", gameBoard.Id);
                return Result<Guid>.Success(gameBoard.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the board.");
                return Result<Guid>.Failure("An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<Result<bool[][]>> GetNextState(Guid id)
        {
            try
            {
                var gameBoard = await _repository.GetBoard(id);
                if (gameBoard == null)
                {
                    _logger.LogWarning("Next state request failed: Board not found. ID: {id}", id);
                    return Result<bool[][]>.Failure(ValidationMessages.BoardNotFound);
                }

                if (IsBoardEmpty(gameBoard.Board))
                {
                    return Result<bool[][]>.Success(gameBoard.Board);
                }

                var nextState = _computeService.ComputeNextState(gameBoard.Board);
                gameBoard.Board = nextState;
                await _repository.SaveBoard(gameBoard);

                _logger.LogInformation("Next state computed successfully for ID: {id}", id);
                return Result<bool[][]>.Success(nextState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while computing the next state for ID: {id}", id);
                return Result<bool[][]>.Failure("An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<Result<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts)
        {
            try
            {
                if (maxAttempts < MinAllowedAttempts || maxAttempts > _settings.MaxAllowedAttempts)
                {
                    _logger.LogWarning("Final state request failed: Invalid maxAttempts for ID: {id}. Attempts: {attempts}", id, maxAttempts);
                    return Result<FinalStateResultDto>.Failure(string.Format(ValidationMessages.InvalidMaxAttempts, MinAllowedAttempts, _settings.MaxAllowedAttempts));
                }

                var gameBoard = await _repository.GetBoard(id);
                if (gameBoard == null)
                {
                    _logger.LogWarning("Final state request failed: Board not found. ID: {id}", id);
                    return Result<FinalStateResultDto>.Failure(ValidationMessages.BoardNotFound);
                }

                var seenStates = new HashSet<int>();
                for (int i = 0; i < maxAttempts; i++)
                {
                    int hash = _computeService.GetBoardHash(gameBoard.Board);
                    if (seenStates.Contains(hash))
                    {
                        _logger.LogInformation("Final state reached for ID: {id} in {attempts} attempts.", id, i);
                        return Result<FinalStateResultDto>.Success(new FinalStateResultDto { Board = gameBoard.Board, Completed = true });
                    }

                    seenStates.Add(hash);
                    gameBoard.Board = _computeService.ComputeNextState(gameBoard.Board);
                }

                _logger.LogWarning("Final state request failed: No stable state found within {maxAttempts} attempts for ID: {id}.", maxAttempts, id);
                return Result<FinalStateResultDto>.Failure(ValidationMessages.NoFinalStateReached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while computing the final state for ID: {id}", id);
                return Result<FinalStateResultDto>.Failure("An unexpected error occurred. Please try again later.");
            }
        }

        private bool IsBoardEmpty(bool[][] board)
        {
            return board.All(row => row.All(cell => !cell));
        }
    }
}
