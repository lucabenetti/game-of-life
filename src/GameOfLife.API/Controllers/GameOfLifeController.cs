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

        public GameOfLifeController(IGameOfLifeService gameOfLifeService)
        {
            _gameOfLifeService = gameOfLifeService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<Guid>> Upload([FromBody] HashSet<(int, int)> board)
        {
            var id = await _gameOfLifeService.UploadBoard(board);
            return Ok(id);
        }

        [HttpGet("{id}/next")]
        public async Task<ActionResult<HashSet<(int, int)>>> GetNextState(Guid id)
        {
            var nextState = await _gameOfLifeService.GetNextState(id);
            return nextState != null ? Ok(nextState) : NotFound();
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<ActionResult<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts)
        {
            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (result.Board == null)
            {
                return BadRequest("No final state reached within the given attempts.");
            }

            return Ok(result);
        }
    }
}
