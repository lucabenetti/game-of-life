using GameOfLife.API.DTOs;
using System;
using System.Threading.Tasks;

namespace GameOfLife.API.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for handling Game of Life board state management and computations.
    /// </summary>
    public interface IGameOfLifeService
    {
        /// <summary>
        /// Uploads a new Game of Life board and stores it in memory or a persistent store.
        /// Performs validation on board size and structure.
        /// </summary>
        /// <param name="board">The initial state of the board as a 2D boolean array.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the unique <see cref="Guid"/> representing the stored board 
        /// if successful, or an error message if validation fails.
        /// </returns>
        Task<Result<Guid>> UploadBoard(bool[][] board);

        /// <summary>
        /// Computes and retrieves the next state of the board associated with the given identifier.
        /// Returns an error if the board does not exist.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the next state of the board as a 2D boolean array
        /// if successful, or an error message if the board is not found.
        /// </returns>
        Task<Result<bool[][]>> GetNextState(Guid id);

        /// <summary>
        /// Computes the board state iteratively until a stable or repeating pattern is detected,
        /// or the maximum number of attempts is reached. Ensures the number of attempts does not exceed limits.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <param name="maxAttempts">The maximum number of iterations before stopping.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a <see cref="FinalStateResultDto"/> 
        /// with the final state if a stable configuration is reached, or an error message if not.
        /// </returns>
        Task<Result<FinalStateResultDto>> GetFinalState(Guid id, int maxAttempts);
    }
}
