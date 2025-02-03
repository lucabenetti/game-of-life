using GameOfLife.API.DTOs;
using GameOfLife.API.Middlewares;
using GameOfLife.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.API.Controllers
{
    [ApiController]
    [Route("api/gameoflife")]
    public class GameOfLifeController : ControllerBase
    {
        private const int DefaultMaxAttempts = 200;
        private const int MaxAllowedAttempts = 1000;
        private const int MaxBoardWidth = 500;
        private const int MaxBoardHeight = 500;

        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly ILogger<GameOfLifeController> _logger;

        public GameOfLifeController(IGameOfLifeService service, ILogger<GameOfLifeController> logger)
        {
            _gameOfLifeService = service;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<Guid>> Upload([FromBody] bool[][] board)
        {
            if (board == null || board.Length == 0)
            {
                _logger.LogInformation("Request received to upload null board");
                return BadRequest("The board cannot be null or empty.");
            }

            int rows = board.Length;
            int cols = board[0].Length;

            if (rows > MaxBoardHeight || cols > MaxBoardWidth)
            {
                _logger.LogInformation("Request received with height: {@rows}, width: {@cols}", rows, cols);
                return BadRequest($"Board size exceeds limit. Max allowed size is {MaxBoardHeight} x {MaxBoardWidth}.");
            }

            for (int i = 0; i < rows; i++)
            {
                if (board[i] == null || board[i].Length != cols)
                {
                    _logger.LogInformation("Request received with inconsistent size, height: {@rows}, width: {@cols}, row: {@row}", rows, cols, i);
                    return BadRequest($"Row {i} is null or inconsistent in size.");
                }
            }

            var id = await _gameOfLifeService.UploadBoard(board);

            _logger.LogInformation("Game stored with id: {@id}", id);

            return Ok(id);
        }

        [HttpGet("{id}/next")]
        public async Task<ActionResult<bool[][]>> GetNextState(Guid id)
        {
            var nextState = await _gameOfLifeService.GetNextState(id);

            _logger.LogInformation("Game updated to next state id: {@id}", id);

            return nextState != null ? Ok(nextState) : NotFound();
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<ActionResult<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts = DefaultMaxAttempts)
        {
            if (maxAttempts <= 0 || maxAttempts > MaxAllowedAttempts)
            {
                _logger.LogInformation("Max attempts surpasses allowed: {@id}, attemps: {@attempts}", id, maxAttempts);
                return BadRequest($"maxAttempts must be between 1 and {MaxAllowedAttempts}.");
            }

            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (result.Board == null)
            {
                _logger.LogInformation("No final state reached within the given attempts: {@id}, attemps: {@attempts}", id, maxAttempts);
                return BadRequest("No final state reached within the given attempts.");
            }

            _logger.LogInformation("Game updated to final state id: {@id}", id);

            return Ok(result);
        }
    }
}
