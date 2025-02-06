namespace GameOfLife.API.Configurations
{
    public class GameOfLifeSettings
    {
        public int DefaultMaxAttempts { get; set; } = 200;
        public int MaxAllowedAttempts { get; set; } = 1000;
        public int MaxBoardWidth { get; set; } = 500;
        public int MaxBoardHeight { get; set; } = 500;
    }
}
