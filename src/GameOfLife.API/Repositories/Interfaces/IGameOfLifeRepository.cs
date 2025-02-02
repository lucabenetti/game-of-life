using GameOfLife.API.Models;

namespace GameOfLife.API.Repositories.Interfaces
{
    public interface IGameOfLifeRepository
    {
        Task SaveBoard(GameOfLifeBoard board);
        Task<GameOfLifeBoard?> GetBoard(Guid id);
    }
}
