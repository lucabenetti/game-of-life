namespace GameOfLife.API.Constants
{
    public static class GameOfLifeComputeConstants
    {
        public const int HashSeed = 17;
        public const int HashMultiplier = 31;
        public const int AliveCellValue = 1;
        public const int DeadCellValue = 0;
        public const int MinSurvivalNeighbors = 2;
        public const int MaxSurvivalNeighbors = 3;
        public const int RequiredReproductionNeighbors = 3;
        public const int TotalNeighbors = 8;

        public static readonly int[] NeighborXOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
        public static readonly int[] NeighborYOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };
    }
}
