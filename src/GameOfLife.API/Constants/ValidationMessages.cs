namespace GameOfLife.API.Constants
{
    public static class ValidationMessages
    {
        public const string BoardNotFound = "Board not found.";
        public const string BoardSizeExceeded = "Board size exceeds limit.";
        public const string InvalidMaxAttempts = "maxAttempts must be between {0} and {1}.";
        public const string NoFinalStateReached = "No final state reached within the given attempts.";
        public const string InvalidBoardStructure = "Row {0} is null or inconsistent in size.";
        public const string EmptyBoard = "The board cannot be null or empty.";
    }
}
