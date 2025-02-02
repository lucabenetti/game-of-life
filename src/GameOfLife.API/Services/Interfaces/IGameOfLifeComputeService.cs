namespace GameOfLife.API.Services.Interfaces
{
    public interface IGameOfLifeComputeService
    {
        HashSet<(int, int)> ComputeNextState(HashSet<(int, int)> aliveCells);
        int GetBoardHash(HashSet<(int, int)> board);
    }
}
