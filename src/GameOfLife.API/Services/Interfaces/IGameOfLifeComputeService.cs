namespace GameOfLife.API.Services.Interfaces
{
    public interface IGameOfLifeComputeService
    {
        bool[][] ComputeNextState(bool[][] board);
        int GetBoardHash(bool[][] board);
    }
}
