namespace GameOfLife.API.Constants
{
    public static class ValidationMessages
    {
        // General Errors
        public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
        public const string InvalidRequest = "Invalid request parameters.";

        // Board Validations
        public const string NullBoard = "The board cannot be null.";
        public const string EmptyBoard = "The board cannot be empty.";
        public const string NullBoardRows = "The board contains null rows.";
        public const string InconsistentRowSize = "Row {0} is null or inconsistent in size.";
        public const string InvalidBoardStructure = "The board has an inconsistent row structure.";
        public const string InvalidBoardDimensions = "Board dimensions must be greater than zero.";
        public const string BoardSizeExceeded = "Board size exceeds limit. Max allowed size is {0} x {1}.";
        public const string InvalidCellValue = "Board contains invalid values. Only 0 and 1 are allowed.";

        // Failure Messages
        public const string ComputeNextStateFailed = "ComputeNextState failed: {0}";
        public const string GetBoardHashFailed = "GetBoardHash failed: {0}";
        public const string CountAliveNeighborsFailed = "CountAliveNeighbors failed: {0}";
        public const string UploadFailed = "Upload failed: {0}";
        public const string FinalStateRequestFailed = "Final state request failed: {0}";
        public const string ValidationFailed = "Validation failed: {0}";

        // Board Processing Errors
        public const string BoardNotFound = "Board not found.";
        public const string NextStateFailed = "Failed to compute the next state.";
        public const string FinalStateFailed = "Failed to compute the final state.";
        public const string NoFinalStateReached = "No final state reached within the given attempts.";

        // Computation Errors
        public const string StateComputationError = "An error occurred while computing the next state.";
        public const string HashGenerationError = "An error occurred while generating the board hash.";
        public const string CountAliveNeighborsError = "An error occurred while counting alive neighbors.";
        public const string BoardStateCacheUsed = "Returning cached board state.";
        public const string BoardStateComputed = "Next state computed successfully.";

        // Input Validation Errors
        public const string InvalidCellCoordinates = "The provided cell coordinates (x: {0}, y: {1}) are out of bounds.";
        public const string InvalidMaxAttempts = "maxAttempts must be between {0} and {1}.";
        public const string OutOfBounds = "The provided index is out of bounds.";
        public const string InvalidGuid = "The provided GUID is invalid.";
    }
}
