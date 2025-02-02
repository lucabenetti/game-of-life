namespace GameOfLife.API.DTOs
{
    public class FinalStateResultDto
    {
        public HashSet<(int, int)>? Board { get; set; }
        public bool Completed { get; set; }
    }
}
