namespace SoNice.Application.Common;

/// <summary>
/// Generic service result wrapper
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult<T> SuccessResult(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResult<T> Failure(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message
        };
    }

    public static ServiceResult<T> Failure(string message, List<string> errors)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
