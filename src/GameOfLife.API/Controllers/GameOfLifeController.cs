using GameOfLife.API.Configurations;
using GameOfLife.API.DTOs;
using GameOfLife.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GameOfLife.API.Controllers
{
    [ApiController]
    [Route("api/gameoflife")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly GameOfLifeSettings _settings;
        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly ILogger<GameOfLifeController> _logger;

        public GameOfLifeController(IGameOfLifeService service, ILogger<GameOfLifeController> logger, IOptions<GameOfLifeSettings> settings)
        {
            _gameOfLifeService = service;
            _logger = logger;
            _settings = settings.Value;
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

            if (rows > _settings.MaxBoardHeight || cols > _settings.MaxBoardWidth)
            {
                _logger.LogInformation("Request received with height: {@rows}, width: {@cols}", rows, cols);
                return BadRequest($"Board size exceeds limit. Max allowed size is {_settings.MaxBoardHeight} x {_settings.MaxBoardWidth}.");
            }

            var id = await _gameOfLifeService.UploadBoard(board);

            _logger.LogInformation("Game stored with id: {@id}", id);

            return Ok(id);
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<ActionResult<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts)
        {
            if (maxAttempts <= 0 || maxAttempts > _settings.MaxAllowedAttempts)
            {
                _logger.LogInformation("Max attempts surpasses allowed: {@id}, attempts: {@attempts}", id, maxAttempts);
                return BadRequest($"maxAttempts must be between 1 and {_settings.MaxAllowedAttempts}.");
            }

            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (result.Board == null)
            {
                _logger.LogInformation("No final state reached within the given attempts: {@id}, attempts: {@attempts}", id, maxAttempts);
                return BadRequest("No final state reached within the given attempts.");
            }

            _logger.LogInformation("Game updated to final state id: {@id}", id);

            return Ok(result);
        }
    }
}
