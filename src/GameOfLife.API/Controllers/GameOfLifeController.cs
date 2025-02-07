using GameOfLife.API.Constants;
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
        public async Task<IActionResult> Upload([FromBody] int[][] board)
        {
            var result = await _gameOfLifeService.UploadBoard(board);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<int[][]>.FailureResponse(result.ErrorMessage));
            }

            return CreatedAtAction(nameof(GetNextState), new { id = result.Value }, ApiResponse<Guid>.SuccessResponse(result.Value, ApiResponseMessages.BoardUploadedSuccessfully));
        }

        [HttpGet("{id}/next")]
        public async Task<IActionResult> GetNextState(Guid id)
        {
            var result = await _gameOfLifeService.GetNextState(id);
            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<int[][]>.FailureResponse(result.ErrorMessage));
            }

            return Ok(ApiResponse<int[][]>.SuccessResponse(result.Value, ApiResponseMessages.NextStateComputedSuccessfully));
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<IActionResult> GetFinalState(Guid id, int maxAttempts)
        {
            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<int[][]>.FailureResponse(result.ErrorMessage));
            }

            return Ok(ApiResponse<FinalStateResultDto>.SuccessResponse(result.Value, ApiResponseMessages.FinalStateComputationCompleted));
        }
    }
}
