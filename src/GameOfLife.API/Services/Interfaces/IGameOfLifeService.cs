using GameOfLife.API.DTOs;

namespace GameOfLife.API.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for handling Game of Life board state management and computations.
    /// </summary>
    public interface IGameOfLifeService
    {
        /// <summary>
        /// Uploads a new Game of Life board and stores it in memory or a persistent store.
        /// Returns a unique identifier for tracking the board state.
        /// </summary>
        /// <param name="board">The initial state of the board as a 2D boolean array.</param>
        /// <returns>A unique <see cref="Guid"/> representing the stored board.</returns>
        Task<Guid> UploadBoard(bool[][] board);

        /// <summary>
        /// Computes and retrieves the next state of the board associated with the given identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The next state of the board as a 2D boolean array.</returns>
        Task<bool[][]> GetNextState(Guid id);

        /// <summary>
        /// Computes the board state iteratively until a stable or repeating pattern is detected,
        /// or the maximum number of attempts is reached.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <param name="maxAttempts">The maximum number of iterations before stopping.</param>
        /// <returns>A <see cref="FinalStateResultDto"/> containing the final state and metadata.</returns>
        Task<FinalStateResultDto> GetFinalState(Guid id, int maxAttempts);
    }
}
