using GameOfLife.API.DTOs;

namespace GameOfLife.API.Services.Interfaces
{
    public interface IGameOfLifeService
    {
        Task<Guid> UploadBoard(bool[][] board);
        Task<bool[][]> GetNextState(Guid id);
        Task<FinalStateResultDto> GetFinalState(Guid id, int maxAttempts);
    }
}
