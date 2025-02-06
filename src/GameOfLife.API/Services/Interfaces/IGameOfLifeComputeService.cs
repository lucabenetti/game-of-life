namespace GameOfLife.API.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for computing the next state in Conway's Game of Life.
    /// </summary>
    public interface IGameOfLifeComputeService
    {
        /// <summary>
        /// Computes the next state of the given Game of Life board based on the defined rules.
        /// Uses memoization to return a cached result if the same board state was previously computed.
        /// </summary>
        /// <param name="board">Current state of the board as a 2D boolean array.</param>
        /// <returns>Next state of the board as a 2D boolean array.</returns>
        bool[][] ComputeNextState(bool[][] board);

        /// <summary>
        /// Generates a unique hash code for the given board state.
        /// This hash is used for memoization to quickly retrieve precomputed states.
        /// </summary>
        /// <param name="board">Current state of the board as a 2D boolean array.</param>
        /// <returns>An integer hash representing the board state.</returns>
        int GetBoardHash(bool[][] board);
    }
}
