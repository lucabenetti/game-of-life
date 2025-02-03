using GameOfLife.API.DTOs;
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

        public GameOfLifeController(IGameOfLifeService service)
        {
            _gameOfLifeService = service;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<Guid>> Upload([FromBody] bool[][] board)
        {
            if (board == null || board.Length == 0)
            {
                return BadRequest("The board cannot be null or empty.");
            }

            int rows = board.Length;
            int cols = board[0].Length;

            if (rows > MaxBoardHeight || cols > MaxBoardWidth)
            {
                return BadRequest($"Board size exceeds limit. Max allowed size is {MaxBoardHeight} x {MaxBoardWidth}.");
            }

            for (int i = 0; i < rows; i++)
            {
                if (board[i] == null || board[i].Length != cols)
                {
                    return BadRequest($"Row {i} is null or inconsistent in size.");
                }
            }

            var id = await _gameOfLifeService.UploadBoard(board);
            return Ok(id);
        }

        [HttpGet("{id}/next")]
        public async Task<ActionResult<bool[][]>> GetNextState(Guid id)
        {
            var nextState = await _gameOfLifeService.GetNextState(id);
            return nextState != null ? Ok(nextState) : NotFound();
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<ActionResult<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts = DefaultMaxAttempts)
        {
            if (maxAttempts <= 0 || maxAttempts > MaxAllowedAttempts)
            {
                return BadRequest($"maxAttempts must be between 1 and {MaxAllowedAttempts}.");
            }

            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (result.Board == null)
            {
                return BadRequest("No final state reached within the given attempts.");
            }

            return Ok(result);
        }

        [HttpGet("error")]
        public IActionResult GetError()
        {
            throw new Exception("This is a test exception.");
        }
    }
}
