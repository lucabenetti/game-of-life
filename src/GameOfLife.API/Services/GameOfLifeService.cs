using GameOfLife.API.Configurations;
using GameOfLife.API.Constants;
using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Options;

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
            if (board == null || board.Length == 0)
            {
                _logger.LogInformation("Request received to upload a null or empty board.");
                return Result<Guid>.Failure(ValidationMessage.EmptyBoard);
            }

            int rows = board.Length;
            int cols = board[0].Length;

            if (rows > _settings.MaxBoardHeight || cols > _settings.MaxBoardWidth)
            {
                _logger.LogInformation("Request received with invalid size: height={@rows}, width={@cols}", rows, cols);
                return Result<Guid>.Failure($"{ValidationMessage.BoardSizeExceeded} Max allowed size is {_settings.MaxBoardHeight} x {_settings.MaxBoardWidth}.");
            }

            for (int i = 0; i < rows; i++)
            {
                if (board[i] == null || board[i].Length != cols)
                {
                    _logger.LogInformation("Request received with inconsistent row sizes: row={@row}", i);
                    return Result<Guid>.Failure(string.Format(ValidationMessage.InvalidBoardStructure, i));
                }
            }

            var gameBoard = new GameOfLifeBoard(board);
            await _repository.SaveBoard(gameBoard);

            _logger.LogInformation("Game stored with id: {@id}", gameBoard.Id);
            return Result<Guid>.Success(gameBoard.Id);
        }

        public async Task<Result<bool[][]>> GetNextState(Guid id)
        {
            var gameBoard = await _repository.GetBoard(id);
            if (gameBoard == null)
            {
                _logger.LogInformation("Board not found for id: {@id}", id);
                return Result<bool[][]>.Failure(ValidationMessage.BoardNotFound);
            }

            if (IsBoardEmpty(gameBoard.Board))
            {
                return Result<bool[][]>.Success(gameBoard.Board);
            }

            var nextState = _computeService.ComputeNextState(gameBoard.Board);
            gameBoard.Board = nextState;
            await _repository.SaveBoard(gameBoard);

            _logger.LogInformation("Game updated to next state for id: {@id}", id);
            return Result<bool[][]>.Success(nextState);
        }

        public async Task<Result<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts)
        {
            if (maxAttempts < MinAllowedAttempts || maxAttempts > _settings.MaxAllowedAttempts)
            {
                _logger.LogInformation("Invalid maxAttempts: id={@id}, attempts={@attempts}", id, maxAttempts);
                return Result<FinalStateResultDto>.Failure(string.Format(ValidationMessage.InvalidMaxAttempts, MinAllowedAttempts, _settings.MaxAllowedAttempts));
            }

            var gameBoard = await _repository.GetBoard(id);
            if (gameBoard == null)
            {
                _logger.LogInformation("Board not found for final state: {@id}", id);
                return Result<FinalStateResultDto>.Failure(ValidationMessage.BoardNotFound);
            }

            var seenStates = new HashSet<int>();
            for (int i = 0; i < maxAttempts; i++)
            {
                int hash = _computeService.GetBoardHash(gameBoard.Board);
                if (seenStates.Contains(hash))
                {
                    return Result<FinalStateResultDto>.Success(new FinalStateResultDto { Board = gameBoard.Board, Completed = true });
                }

                seenStates.Add(hash);
                gameBoard.Board = _computeService.ComputeNextState(gameBoard.Board);
            }

            return Result<FinalStateResultDto>.Failure(ValidationMessage.NoFinalStateReached);
        }

        private bool IsBoardEmpty(bool[][] board)
        {
            return board.All(row => row.All(cell => !cell));
        }
    }
}
