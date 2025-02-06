using GameOfLife.API.Constants;
using GameOfLife.API.Services.Interfaces;

namespace GameOfLife.API.Services
{
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
        private readonly Dictionary<int, bool[][]> _stateCache = new();

        public bool[][] ComputeNextState(bool[][] board)
        {
            int boardHash = GetBoardHash(board);
            if (_stateCache.TryGetValue(boardHash, out var cachedState))
            {
                return cachedState;
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
                    nextState[i][j] = board[i][j]
                        ? (aliveNeighbors >= GameOfLifeComputeConstants.MinSurvivalNeighbors && aliveNeighbors <= GameOfLifeComputeConstants.MaxSurvivalNeighbors)
                        : (aliveNeighbors == GameOfLifeComputeConstants.RequiredReproductionNeighbors);
                }
            }

            _stateCache[boardHash] = nextState;
            return nextState;
        }

        public int GetBoardHash(bool[][] board)
        {
            int hash = GameOfLifeComputeConstants.HashSeed;
            foreach (var row in board)
            {
                foreach (var cell in row)
                {
                    hash = hash * GameOfLifeComputeConstants.HashMultiplier + (cell ? GameOfLifeComputeConstants.AliveCellValue : GameOfLifeComputeConstants.DeadCellValue);
                }
            }
            return hash;
        }

        public int CountAliveNeighbors(bool[][] board, int x, int y)
        {
            int rows = board.Length;
            int cols = board[0].Length;
            int count = 0;

            for (int i = 0; i < GameOfLifeComputeConstants.TotalNeighbors; i++)
            {
                int nx = x + GameOfLifeComputeConstants.NeighborXOffsets[i];
                int ny = y + GameOfLifeComputeConstants.NeighborYOffsets[i];

                if (nx >= 0 && nx < rows && ny >= 0 && ny < cols && board[nx][ny])
                {
                    count++;
                }
            }
            return count;
        }
    }
}
