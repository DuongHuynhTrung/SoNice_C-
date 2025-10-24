using SoNice.Application.DTOs;
using SoNice.Domain.Entities;

namespace SoNice.Application.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<ApiResponseDto<LoginResponseDto>> RegisterAsync(RegisterUserDto registerDto);
    Task<ApiResponseDto<LoginResponseDto>> RegisterUserAsync(RegisterUserDto registerDto);
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginUserDto loginDto);
    Task<ApiResponseDto<LoginResponseDto>> LoginUserAsync(LoginUserDto loginDto);
    Task<ApiResponseDto<LoginResponseDto>> LoginGoogleAsync(GoogleLoginDto googleLoginDto);
    Task<ApiResponseDto<LoginResponseDto>> VerifyEmailAsync(string token);
    Task<ApiResponseDto> LogoutAsync();
    Task<ApiResponseDto<UserResponseDto>> GetCurrentUserAsync(string userId);
    Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(string id, string currentUserId, string currentUserRole);
    Task<ApiResponseDto<PaginatedResponseDto<UserResponseDto>>> GetUsersAsync(int page, int limit, string currentUserRole);
    Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto updateDto, string currentUserId);
    Task<ApiResponseDto> DeleteUserAsync(string id, string currentUserRole);
    Task<ApiResponseDto> DeleteUserNoAuthAsync(string id);
    Task<ApiResponseDto> CheckOldPasswordAsync(string userId, CheckOldPasswordDto checkDto, string currentUserId);
    Task<ApiResponseDto<UserResponseDto>> ChangePasswordAsync(string userId, ChangePasswordDto changeDto, string currentUserId);
    Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotDto);
    Task<ApiResponseDto> VerifyOtpAsync(VerifyOtpDto verifyDto);
    Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordDto resetDto);
    Task<ApiResponseDto<IEnumerable<AccountStatisticsDto>>> GetAccountStatisticsAsync();
    Task<ApiResponseDto<IEnumerable<UserResponseDto>>> SearchUsersByEmailAsync(string email, string currentUserRole);
    Task<ApiResponseDto<UserResponseDto>> BanAccountAsync(string accountId, string currentUserRole);
    Task<ApiResponseDto<UserResponseDto>> UnbanAccountAsync(string accountId, string currentUserRole);
    Task<ApiResponseDto<UserResponseDto>> UpdateUserForAdminAsync(string id, UpdateUserDto updateDto, string currentUserRole);
}
