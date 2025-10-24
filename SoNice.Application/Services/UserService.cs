using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Entities;
using SoNice.Domain.Enums;
using SoNice.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using System.Text.Json;

namespace SoNice.Application.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IEmailService _emailService;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly string _clientUrl;

    public UserService(
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailService = emailService;
        _jwtSecret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");
        _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
        _clientUrl = configuration["ClientUrl"] ?? "https://camcrew.vercel.app";
    }

    public async Task<ApiResponseDto<LoginResponseDto>> RegisterAsync(RegisterUserDto registerDto)
    {
        return await RegisterUserAsync(registerDto);
    }

    public async Task<ApiResponseDto<LoginResponseDto>> RegisterUserAsync(RegisterUserDto registerDto)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("User already registered with this email", 400);
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create user
            var user = new User
            {
                Email = registerDto.Email,
                Password = hashedPassword,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                RoleName = registerDto.Role ?? UserRole.Customer,
                IsVerified = false
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Create cart for user
            var cart = new Cart
            {
                UserId = user.Id,
                CartItemsList = new List<CartItem>()
            };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            // Generate verification token
            var verifyToken = GenerateJwtToken(user.Id, user.Email);

            // Send verification email
            var verifyLink = $"{_clientUrl}/verify?upn={verifyToken}";
            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName ?? user.Email, verifyLink);

            return ApiResponseDto<LoginResponseDto>.SuccessResult(
                new LoginResponseDto
                {
                    AccessToken = verifyToken,
                    User = MapToUserResponseDto(user)
                },
                "Registration successful! Please check your email to verify your account."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return ApiResponseDto<LoginResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginUserDto loginDto)
    {
        return await LoginUserAsync(loginDto);
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginUserAsync(LoginUserDto loginDto)
    {
        try
        {
            // Find user by email or phone
            var user = await _unitOfWork.Users.GetByEmailOrPhoneAsync(loginDto.Identifier);
            if (user == null)
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult($"User not found with email or phone number {loginDto.Identifier}", 404);
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("Invalid email/phone or password", 401);
            }

            // Note: Status check removed as Status property no longer exists in User entity

            // Check if email is verified
            if (!user.IsVerified)
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("Your account email is not verified. Please check your email to verify your account.", 401);
            }

            // Ensure cart exists
            var existingCart = await _unitOfWork.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (existingCart == null)
            {
                var cart = new Cart
                {
                    UserId = user.Id,
                    CartItemsList = new List<CartItem>()
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            // Generate access token
            var accessToken = GenerateJwtToken(user.Id, user.Email, user.RoleName.ToString());

            return ApiResponseDto<LoginResponseDto>.SuccessResult(
                new LoginResponseDto
                {
                    AccessToken = accessToken,
                    User = MapToUserResponseDto(user)
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return ApiResponseDto<LoginResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginGoogleAsync(GoogleLoginDto googleLoginDto)
    {
        try
        {
            // Verify Google OAuth token
            var googlePayload = await VerifyGoogleTokenAsync(googleLoginDto.Token);
            if (googlePayload == null)
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("Invalid Google token", 401);
            }

            // Find or create user
            var user = await _unitOfWork.Users.GetByEmailAsync(googlePayload.Email);
            if (user == null)
            {
                // Create new user from Google data
                user = new User
                {
                    Email = googlePayload.Email,
                    FullName = googlePayload.Name,
                    AvatarUrl = googlePayload.Picture,
                    RoleName = UserRole.Customer,
                    IsVerified = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Create cart for new user
                var cart = new Cart
                {
                    UserId = user.Id,
                    CartItemsList = new List<CartItem>()
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Note: Status check removed as Status property no longer exists in User entity

                // Ensure cart exists for existing user
                var existingCart = await _unitOfWork.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (existingCart == null)
                {
                    var cart = new Cart
                    {
                        UserId = user.Id,
                        CartItemsList = new List<CartItem>()
                    };
                    await _unitOfWork.Carts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            // Generate access token
            var accessToken = GenerateJwtToken(user.Id, user.Email, user.RoleName.ToString());

            return ApiResponseDto<LoginResponseDto>.SuccessResult(
                new LoginResponseDto
                {
                    AccessToken = accessToken,
                    User = MapToUserResponseDto(user)
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return ApiResponseDto<LoginResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<LoginResponseDto>> VerifyEmailAsync(string token)
    {
        try
        {
            var userId = ValidateJwtToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("Invalid or expired verification token", 400);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<LoginResponseDto>.ErrorResult("User not found", 404);
            }

            if (user.IsVerified)
            {
                var accessToken = GenerateJwtToken(user.Id, user.Email, user.RoleName.ToString());
                return ApiResponseDto<LoginResponseDto>.SuccessResult(
                    new LoginResponseDto
                    {
                        AccessToken = accessToken,
                        User = MapToUserResponseDto(user)
                    },
                    "Account already verified."
                );
            }

            user.IsVerified = true;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var newAccessToken = GenerateJwtToken(user.Id, user.Email, user.RoleName.ToString());
            return ApiResponseDto<LoginResponseDto>.SuccessResult(
                new LoginResponseDto
                {
                    AccessToken = newAccessToken,
                    User = MapToUserResponseDto(user)
                },
                "Email verification successful!"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return ApiResponseDto<LoginResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public Task<ApiResponseDto> LogoutAsync()
    {
        // In JWT-based authentication, logout is typically handled client-side
        // by removing the token from storage
        return Task.FromResult(ApiResponseDto.SuccessResult("Logout successful"));
    }

    public async Task<ApiResponseDto<UserResponseDto>> GetCurrentUserAsync(string userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("User not found", 404);
            }

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> GetUserByIdAsync(string id, string currentUserId, string currentUserRole)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("User not found", 404);
            }

            // Check permissions
            if (currentUserRole != UserRole.Admin.ToString() && currentUserId != user.Id)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("You don't have permission to access this user information", 403);
            }

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<PaginatedResponseDto<UserResponseDto>>> GetUsersAsync(int page, int limit, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto<PaginatedResponseDto<UserResponseDto>>.ErrorResult("Only Admin can access all customer accounts", 403);
            }

            var users = await _unitOfWork.Users.GetUsersByRoleAsync(UserRole.Customer, page, limit);
            var total = await _unitOfWork.Users.GetUsersCountByRoleAsync(UserRole.Customer);

            var userDtos = users.Select(MapToUserResponseDto);

            var paginatedResponse = new PaginatedResponseDto<UserResponseDto>
            {
                Data = userDtos,
                Pagination = new PaginationDto
                {
                    PageIndex = page,
                    PageSize = limit,
                    TotalPages = (int)Math.Ceiling((double)total / limit),
                    TotalResults = total
                }
            };

            return ApiResponseDto<PaginatedResponseDto<UserResponseDto>>.SuccessResult(paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return ApiResponseDto<PaginatedResponseDto<UserResponseDto>>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> UpdateUserAsync(string userId, UpdateUserDto updateDto, string currentUserId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("User not found", 404);
            }

            if (currentUserId != user.Id)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("You don't have permission to update this user information", 403);
            }

            // Check phone number uniqueness if provided
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber) && updateDto.PhoneNumber != user.PhoneNumber)
            {
                var existingUser = await _unitOfWork.Users.GetByPhoneNumberAsync(updateDto.PhoneNumber);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return ApiResponseDto<UserResponseDto>.ErrorResult("Phone number already exists", 400);
                }
            }

            // Update allowed fields
            if (!string.IsNullOrEmpty(updateDto.FullName))
                user.FullName = updateDto.FullName;
            if (updateDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateDto.DateOfBirth;
            if (!string.IsNullOrEmpty(updateDto.Gender))
                user.Gender = updateDto.Gender;
            if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
                user.AvatarUrl = updateDto.AvatarUrl;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;
            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> DeleteUserAsync(string id, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto.ErrorResult("You don't have permission to delete users", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto.SuccessResult("User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> DeleteUserNoAuthAsync(string id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto.SuccessResult("User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> CheckOldPasswordAsync(string userId, CheckOldPasswordDto checkDto, string currentUserId)
    {
        try
        {
            if (currentUserId != userId)
            {
                return ApiResponseDto.ErrorResult("You don't have permission to check password for other users", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            if (!BCrypt.Net.BCrypt.Verify(checkDto.Password, user.Password))
            {
                return ApiResponseDto.ErrorResult("Old password is incorrect", 401);
            }

            return ApiResponseDto.SuccessResult("Old password is correct");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking old password");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> ChangePasswordAsync(string userId, ChangePasswordDto changeDto, string currentUserId)
    {
        try
        {
            if (currentUserId != userId)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("You don't have permission to change password for other users", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("User not found", 404);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(changeDto.Password);
            user.Password = hashedPassword;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(forgotDto.Email);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            // Generate OTP
            var otp = new Random().Next(100000, 999999);
            user.Otp = otp;
            user.OtpExpired = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Send OTP email
            await _emailService.SendOtpEmailAsync(user.Email, user.FullName ?? user.Email, otp.ToString());

            return ApiResponseDto.SuccessResult("OTP has been sent to your email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending forgot password OTP");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> VerifyOtpAsync(VerifyOtpDto verifyDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(verifyDto.Email);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            if (user.Otp?.ToString() != verifyDto.Otp.ToString())
            {
                return ApiResponseDto.ErrorResult("Invalid OTP! Please try again", 400);
            }

            if (user.OtpExpired == null || DateTime.UtcNow.Subtract(user.OtpExpired.Value).TotalMinutes > 10)
            {
                return ApiResponseDto.ErrorResult("OTP has expired! Please try again", 400);
            }

            return ApiResponseDto.SuccessResult("OTP is valid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordDto resetDto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(resetDto.Email);
            if (user == null)
            {
                return ApiResponseDto.ErrorResult("User not found", 404);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            user.Password = hashedPassword;
            user.Otp = null;
            user.OtpExpired = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto.SuccessResult("Password reset successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return ApiResponseDto.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<IEnumerable<AccountStatisticsDto>>> GetAccountStatisticsAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.RoleName == UserRole.Customer);
            if (!users.Any())
            {
                return ApiResponseDto<IEnumerable<AccountStatisticsDto>>.SuccessResult(new List<AccountStatisticsDto>());
            }

            // Note: Status-based statistics removed as Status property no longer exists in User entity
            var activeCount = users.Count(); // All users are considered active
            var inactiveCount = 0; // No inactive users since Status property was removed

            var statistics = new List<AccountStatisticsDto>
            {
                new() { Key = "Active", Value = activeCount },
                new() { Key = "InActive", Value = inactiveCount }
            };

            return ApiResponseDto<IEnumerable<AccountStatisticsDto>>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account statistics");
            return ApiResponseDto<IEnumerable<AccountStatisticsDto>>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<IEnumerable<UserResponseDto>>> SearchUsersByEmailAsync(string email, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto<IEnumerable<UserResponseDto>>.ErrorResult("Only Admin can search accounts", 403);
            }

            var users = await _unitOfWork.Users.SearchUsersByEmailAsync(email, UserRole.Customer);
            var userDtos = users.Select(MapToUserResponseDto);

            return ApiResponseDto<IEnumerable<UserResponseDto>>.SuccessResult(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users by email");
            return ApiResponseDto<IEnumerable<UserResponseDto>>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> BanAccountAsync(string accountId, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Only Admin can ban accounts", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(accountId);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Account not found", 404);
            }

            if (user.RoleName == UserRole.Admin)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Cannot ban admin account", 400);
            }

            // Note: Ban functionality removed as Status property no longer exists in User entity
            // Consider implementing alternative blocking mechanism if needed
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning account");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> UnbanAccountAsync(string accountId, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Only Admin can unban accounts", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(accountId);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Account not found", 404);
            }

            if (user.RoleName == UserRole.Admin)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Cannot unban admin account", 400);
            }

            // Note: Unban functionality removed as Status property no longer exists in User entity
            // Consider implementing alternative unblocking mechanism if needed
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning account");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }

    public async Task<ApiResponseDto<UserResponseDto>> UpdateUserForAdminAsync(string id, UpdateUserDto updateDto, string currentUserRole)
    {
        try
        {
            if (currentUserRole != UserRole.Admin.ToString())
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("Only Admin can update user information", 403);
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponseDto<UserResponseDto>.ErrorResult("User not found", 404);
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateDto.FullName))
                user.FullName = updateDto.FullName;
            if (updateDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateDto.DateOfBirth;
            if (!string.IsNullOrEmpty(updateDto.Gender))
                user.Gender = updateDto.Gender;
            if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
                user.AvatarUrl = updateDto.AvatarUrl;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;
            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<UserResponseDto>.SuccessResult(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user for admin");
            return ApiResponseDto<UserResponseDto>.ErrorResult("Internal server error", 500, ex.Message);
        }
    }


    #region Private Methods

    private async Task<GooglePayload?> VerifyGoogleTokenAsync(string token)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={token}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var googlePayload = JsonSerializer.Deserialize<GooglePayload>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return googlePayload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Google token");
            return null;
        }
    }

    private string GenerateJwtToken(string userId, string email, string? role = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", userId),
                new Claim("email", email),
                new Claim("role", role ?? UserRole.Customer.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string? ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken.Claims.First(x => x.Type == "userId").Value;
        }
        catch
        {
            return null;
        }
    }

    private static UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            IsVerified = user.IsVerified,
            RoleName = user.RoleName,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    #endregion
}
