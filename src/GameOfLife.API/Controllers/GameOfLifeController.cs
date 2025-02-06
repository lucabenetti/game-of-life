using GameOfLife.API.DTOs;
using GameOfLife.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.API.Controllers
{
    [ApiController]
    [Route("api/gameoflife")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly IGameOfLifeService _gameOfLifeService;

        public GameOfLifeController(IGameOfLifeService service)
        {
            _gameOfLifeService = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] bool[][] board)
        {
            var result = await _gameOfLifeService.UploadBoard(board);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id}/next")]
        public async Task<IActionResult> GetNextState(Guid id)
        {
            var result = await _gameOfLifeService.GetNextState(id);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<IActionResult> GetFinalState(Guid id, int maxAttempts)
        {
            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
