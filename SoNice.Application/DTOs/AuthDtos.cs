namespace SoNice.Application.DTOs;

/// <summary>
/// Verify email DTO
/// </summary>
public class VerifyEmailDto
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Verify email response DTO
/// </summary>
public class VerifyEmailResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
}

/// <summary>
/// Register response DTO
/// </summary>
public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Logout response DTO
/// </summary>
public class LogoutResponseDto
{
    public string Message { get; set; } = string.Empty;
}
