namespace SoNice.Application.DTOs;

/// <summary>
/// Generic response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string? Error { get; set; }

    public static ApiResponseDto<T> SuccessResult(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, int statusCode = 500, string? error = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Error = error
        };
    }
}

/// <summary>
/// Simple response DTO without data
/// </summary>
public class ApiResponseDto : ApiResponseDto<object>
{
    public static ApiResponseDto SuccessResult(string message = "Success", int statusCode = 200)
    {
        return new ApiResponseDto
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public new static ApiResponseDto ErrorResult(string message, int statusCode = 500, string? error = null)
    {
        return new ApiResponseDto
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Error = error
        };
    }
}
