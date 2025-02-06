using GameOfLife.API.DTOs;
using GameOfLife.API.Models;
using GameOfLife.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
                return BadRequest(new ApiResponse<Guid>(default, result.ErrorMessage, false));
            }

            return CreatedAtAction(nameof(GetNextState), new { id = result.Value }, new ApiResponse<Guid>(result.Value, "Board uploaded successfully."));
        }

        [HttpGet("{id}/next")]
        public async Task<IActionResult> GetNextState(Guid id)
        {
            var result = await _gameOfLifeService.GetNextState(id);
            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse<int[][]>(null, result.ErrorMessage, false));
            }

            return Ok(new ApiResponse<int[][]>(result.Value, "Next state computed successfully."));
        }

        [HttpGet("{id}/final/{maxAttempts}")]
        public async Task<IActionResult> GetFinalState(Guid id, int maxAttempts)
        {
            var result = await _gameOfLifeService.GetFinalState(id, maxAttempts);
            if (!result.IsSuccess)
            {
                return BadRequest(new ApiResponse<FinalStateResultDto>(null, result.ErrorMessage, false));
            }

            return Ok(new ApiResponse<FinalStateResultDto>(result.Value, "Final state computation completed."));
        }
    }
}
