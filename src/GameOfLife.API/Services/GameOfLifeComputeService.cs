using GameOfLife.API.Services.Interfaces;

namespace GameOfLife.API.Services
{
    public class GameOfLifeComputeService : IGameOfLifeComputeService
    {
        private readonly HashSet<(int, int)> _nextStateBuffer = new();

        public HashSet<(int, int)> ComputeNextState(HashSet<(int, int)> aliveCells)
        {
            _nextStateBuffer.Clear();
            var neighborCounts = new Dictionary<(int, int), int>();

            foreach (var (x, y) in aliveCells)
            {
                int count = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        var neighbor = (x + dx, y + dy);
                        if (aliveCells.Contains(neighbor))
                            count++;
                        else
                            neighborCounts[neighbor] = neighborCounts.GetValueOrDefault(neighbor, 0) + 1;
                    }
                }

                if (count == 2 || count == 3)
                    _nextStateBuffer.Add((x, y));
            }

            foreach (var (pos, count) in neighborCounts)
            {
                if (count == 3)
                    _nextStateBuffer.Add(pos);
            }

            return new HashSet<(int, int)>(_nextStateBuffer);
        }

        public int GetBoardHash(HashSet<(int, int)> board)
        {
            int hash = 17;
            foreach (var (x, y) in board)
            {
                hash = hash * 31 + x;
                hash = hash * 31 + y;
            }
            return hash;
        }
    }
}
