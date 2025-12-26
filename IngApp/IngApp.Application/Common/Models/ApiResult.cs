namespace IngApp.Application.Common.Models
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ApiResult Ok(object? data = null, string? message = null)
        {
            return new ApiResult
            {
                Success = true,
                Data = data,
                Message = message ?? string.Empty
            };
        }

        public static ApiResult Fail(string message)
        {
            return new ApiResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
