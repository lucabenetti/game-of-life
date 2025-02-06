namespace GameOfLife.API.DTOs
{
    public record FinalStateResultDto(int[][] Board, bool Completed);
}
