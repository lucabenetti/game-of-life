namespace GameOfLife.API.DTOs
{
    public class FinalStateResultDto
    {
        public bool[][]? Board { get; set; }
        public bool Completed { get; set; }
    }
}
