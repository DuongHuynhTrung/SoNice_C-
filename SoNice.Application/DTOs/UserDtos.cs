using SoNice.Domain.Enums;

namespace SoNice.Application.DTOs;

/// <summary>
/// User registration DTO
/// </summary>
public class RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole? Role { get; set; }
}

/// <summary>
/// User login DTO
/// </summary>
public class LoginUserDto
{
    public string Identifier { get; set; } = string.Empty; // Email or phone number
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Google login DTO
/// </summary>
public class GoogleLoginDto
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Update user DTO
/// </summary>
public class UpdateUserDto
{
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Change password DTO
/// </summary>
public class ChangePasswordDto
{
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Check old password DTO
/// </summary>
public class CheckOldPasswordDto
{
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Forgot password DTO
/// </summary>
public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Verify OTP DTO
/// </summary>
public class VerifyOtpDto
{
    public string Email { get; set; } = string.Empty;
    public int Otp { get; set; }
}

/// <summary>
/// Reset password DTO
/// </summary>
public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Upgrade membership DTO - DEPRECATED: Membership functionality removed
/// </summary>
[Obsolete("Membership functionality has been removed from User entity")]
public class UpgradeMembershipDto
{
    public string UserId { get; set; } = string.Empty;
    public string Membership { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// User response DTO
/// </summary>
public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsVerified { get; set; }
    public UserRole RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Paginated response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationDto Pagination { get; set; } = new();
}

/// <summary>
/// Pagination DTO
/// </summary>
public class PaginationDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public UserResponseDto User { get; set; } = new();
}

/// <summary>
/// Account statistics DTO
/// </summary>
public class AccountStatisticsDto
{
    public string Key { get; set; } = string.Empty;
    public int Value { get; set; }
}
