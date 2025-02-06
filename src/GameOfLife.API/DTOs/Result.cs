namespace GameOfLife.API.DTOs
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }

        private Result(T? value, bool isSuccess, string? errorMessage)
        {
            Value = value;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value) => new(value, true, null);
        public static Result<T> Failure(string errorMessage) => new(default, false, errorMessage);
    }
}
