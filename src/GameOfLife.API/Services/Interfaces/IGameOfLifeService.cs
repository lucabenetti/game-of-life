using GameOfLife.API.DTOs;

namespace GameOfLife.API.Services.Interfaces
{
    public interface IGameOfLifeService
    {
        Task<Guid> UploadBoard(HashSet<(int, int)> board);
        Task<HashSet<(int, int)>?> GetNextState(Guid id);
        Task<FinalStateResultDto> GetFinalState(Guid id, int maxAttempts);
    }
}
