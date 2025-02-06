namespace GameOfLife.API.DTOs
{
    public record FinalStateResultDto(bool[][] Board, bool Completed);
}
