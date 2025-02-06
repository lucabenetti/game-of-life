using GameOfLife.API.Configurations;
using GameOfLife.API.Constants;
using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Repositories.Interfaces;
using GameOfLife.API.Services.Interfaces;
using Microsoft.Extensions.Options;

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
        private readonly GameOfLifeSettings _settings;
        private readonly ILogger<GameOfLifeService> _logger;

        private const int MinAllowedAttempts = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameOfLifeService"/> class.
        /// </summary>
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

        /// <inheritdoc />
        public async Task<Result<Guid>> UploadBoard(bool[][] board)
        {
            try
            {
                // Validate board existence
                if (board == null)
                {
                    _logger.LogWarning(ValidationMessages.UploadFailed, ValidationMessages.NullBoard);
                    return Result<Guid>.Failure(ValidationMessages.NullBoard);
                }

                // Validate board is not empty
                if (board.Length == 0 || board[0] == null || board[0].Length == 0)
                {
                    _logger.LogWarning(ValidationMessages.UploadFailed, ValidationMessages.EmptyBoard);
                    return Result<Guid>.Failure(ValidationMessages.EmptyBoard);
                }

                int rows = board.Length;
                int cols = board[0].Length;

                // Validate board size
                if (rows > _settings.MaxBoardHeight || cols > _settings.MaxBoardWidth)
                {
                    _logger.LogWarning(ValidationMessages.UploadFailed, ValidationMessages.BoardSizeExceeded);
                    return Result<Guid>.Failure(string.Format(ValidationMessages.BoardSizeExceeded, _settings.MaxBoardHeight, _settings.MaxBoardWidth));
                }

                // Validate board structure (null rows or inconsistent row sizes)
                for (int i = 0; i < rows; i++)
                {
                    if (board[i] == null || board[i].Length != cols)
                    {
                        _logger.LogWarning(ValidationMessages.UploadFailed, string.Format(ValidationMessages.InconsistentRowSize, i));
                        return Result<Guid>.Failure(string.Format(ValidationMessages.InconsistentRowSize, i));
                    }
                }

                // Validate board contains at least one live cell
                if (board.All(row => row.All(cell => !cell)))
                {
                    _logger.LogWarning(ValidationMessages.UploadFailed, ValidationMessages.EmptyBoard);
                    return Result<Guid>.Failure(ValidationMessages.EmptyBoard);
                }

                var gameBoard = new GameOfLifeBoard(board);
                await _repository.SaveBoard(gameBoard);

                _logger.LogInformation("Board uploaded successfully. ID: {id}", gameBoard.Id);
                return Result<Guid>.Success(gameBoard.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ValidationMessages.UnexpectedError);
                return Result<Guid>.Failure(ValidationMessages.UnexpectedError);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool[][]>> GetNextState(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.InvalidGuid);
                    return Result<bool[][]>.Failure(ValidationMessages.InvalidGuid);
                }

                var gameBoard = await _repository.GetBoard(id);
                if (gameBoard == null)
                {
                    _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.BoardNotFound);
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
                _logger.LogError(ex, ValidationMessages.NextStateFailed);
                return Result<bool[][]>.Failure(ValidationMessages.NextStateFailed);
            }
        }

        /// <inheritdoc />
        public async Task<Result<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.InvalidGuid);
                    return Result<FinalStateResultDto>.Failure(ValidationMessages.InvalidGuid);
                }

                if (maxAttempts < MinAllowedAttempts || maxAttempts > _settings.MaxAllowedAttempts)
                {
                    _logger.LogWarning(ValidationMessages.FinalStateRequestFailed, ValidationMessages.InvalidMaxAttempts);
                    return Result<FinalStateResultDto>.Failure(string.Format(ValidationMessages.InvalidMaxAttempts, MinAllowedAttempts, _settings.MaxAllowedAttempts));
                }

                var gameBoard = await _repository.GetBoard(id);
                if (gameBoard == null)
                {
                    _logger.LogWarning(ValidationMessages.ValidationFailed, ValidationMessages.BoardNotFound);
                    return Result<FinalStateResultDto>.Failure(ValidationMessages.BoardNotFound);
                }

                var seenStates = new HashSet<int>();
                for (int i = 0; i < maxAttempts; i++)
                {
                    int hash = _computeService.GetBoardHash(gameBoard.Board);
                    if (seenStates.Contains(hash))
                    {
                        _logger.LogInformation("Final state reached for ID: {id} in {attempts} attempts.", id, i);
                        return Result<FinalStateResultDto>.Success(new FinalStateResultDto (gameBoard.Board, true));
                    }

                    seenStates.Add(hash);
                    gameBoard.Board = _computeService.ComputeNextState(gameBoard.Board);
                }

                _logger.LogWarning(ValidationMessages.FinalStateRequestFailed, ValidationMessages.NoFinalStateReached);
                return Result<FinalStateResultDto>.Failure(ValidationMessages.NoFinalStateReached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ValidationMessages.FinalStateFailed);
                return Result<FinalStateResultDto>.Failure(ValidationMessages.FinalStateFailed);
            }
        }

        /// <summary>
        /// Checks whether the board is completely empty (all cells are dead).
        /// </summary>
        private bool IsBoardEmpty(bool[][] board)
        {
            return board.All(row => row.All(cell => !cell));
        }
    }
}
