namespace GameOfLife.API.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public ApiResponse(T? data, string? message = "", bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T?> SuccessResponse(T? data, string? message = "")
        {
            return new ApiResponse<T?>(data, message, true);
        }

        public static ApiResponse<T> FailureResponse(string? message)
        {
            return new ApiResponse<T>(default, message, false);
        }
    }
}
