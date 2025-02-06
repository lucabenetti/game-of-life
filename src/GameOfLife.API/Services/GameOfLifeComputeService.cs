using GameOfLife.API.Services.Interfaces;

namespace GameOfLife.API.Services
{
    /// <summary>
    /// Service responsible for computing the next state of the Game of Life grid.
    /// Implements memoization to cache previously computed states.
    /// </summary>
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
        /// <summary>
        /// Cache for memoization to store previously computed board states.
        /// This helps in avoiding redundant computations for the same state.
        /// </summary>
        private readonly Dictionary<int, bool[][]> _stateCache = new();

        /// <inheritdoc />
        public bool[][] ComputeNextState(bool[][] board)
        {
            int boardHash = GetBoardHash(board);
            if (_stateCache.TryGetValue(boardHash, out var cachedState))
            {
                return cachedState; // Return memoized state if available
            }

            int rows = board.Length;
            int cols = board[0].Length;
            bool[][] nextState = new bool[rows][];
            for (int i = 0; i < rows; i++)
            {
                nextState[i] = new bool[cols];
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int aliveNeighbors = CountAliveNeighbors(board, i, j);
                    nextState[i][j] = board[i][j] ? (aliveNeighbors == 2 || aliveNeighbors == 3) : (aliveNeighbors == 3);
                }
            }

            _stateCache[boardHash] = nextState; // Store computed state in cache
            return nextState;
        }

        /// <inheritdoc />
        public int GetBoardHash(bool[][] board)
        {
            int hash = 17;
            foreach (var row in board)
            {
                foreach (var cell in row)
                {
                    hash = hash * 31 + (cell ? 1 : 0); // Generate a unique hash for the board state
                }
            }
            return hash;
        }

        /// <summary>
        /// Counts the number of live neighbors for a given cell in the board.
        /// A neighbor is considered "alive" if its value is true.
        /// </summary>
        /// <param name="board">Current state of the board as a 2D boolean array.</param>
        /// <param name="x">Row index of the cell.</param>
        /// <param name="y">Column index of the cell.</param>
        /// <returns>Number of live neighbors around the specified cell.</returns>
        private int CountAliveNeighbors(bool[][] board, int x, int y)
        {
            int rows = board.Length;
            int cols = board[0].Length;
            int count = 0;
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++) // Check all 8 neighbors
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx >= 0 && nx < rows && ny >= 0 && ny < cols && board[nx][ny])
                {
                    count++;
                }
            }
            return count;
        }
    }
}
