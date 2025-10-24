using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// User controller - matches Node.js UserController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only) - matches Node.js getUsers exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền truy xuất thông tin tất cả tài khoản khách hàng");
            }

            var result = await _userService.GetUsersAsync(page, limit, UserRole.Admin.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUsers");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get current user - matches Node.js currentUser exactly
    /// </summary>
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetUserId();
            var result = await _userService.GetUserByIdAsync(userId, userId, GetUserRole().ToString());
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentUser");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get user by ID - matches Node.js getUserById exactly
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id, GetUserId(), GetUserRole().ToString());
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            var userRole = GetUserRole();
            var userEmail = GetUserEmail();
            if (userEmail != result.Data!.Email && userRole != UserRole.Admin)
            {
                return Forbid("Bạn không có quyền truy cập thông tin người dùng");
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update user - matches Node.js updateUsers exactly
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _userService.UpdateUserAsync(userId, dto, userId);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("quyền"))
                    return Forbid(result.Message);
                if (result.Message.Contains("tồn tại"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUser");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete user (Admin only) - matches Node.js deleteUsers exactly
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Bạn không có quyền cập nhật thông tin người dùng");
            }

            var result = await _userService.DeleteUserAsync(id, GetUserRole().ToString());
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteUser");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete user no auth - matches Node.js deleteUsersNoAuth exactly
    /// </summary>
    [HttpDelete("delete-no-auth/{id}")]
    public async Task<IActionResult> DeleteUserNoAuth(string id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id, GetUserRole().ToString());
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteUserNoAuth");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Check old password - matches Node.js checkOldPassword exactly
    /// </summary>
    [HttpPost("checkOldPassword/{id}")]
    [Authorize]
    public async Task<IActionResult> CheckOldPassword(string id, [FromBody] CheckOldPasswordDto dto)
    {
        try
        {
            var result = await _userService.CheckOldPasswordAsync(id, dto, GetUserId());
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("không chính xác"))
                    return Unauthorized(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckOldPassword");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Change password - matches Node.js changePassword exactly
    /// </summary>
    [HttpPut("changePassword/{id}")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId != id)
            {
                return Forbid("Bạn không có quyền thay đổi mật khẩu của người khác!");
            }

            var result = await _userService.ChangePasswordAsync(id, dto, GetUserId());
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("không khớp") || result.Message.Contains("empty"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChangePassword");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get account statistics (Admin only) - matches Node.js statisticsAccountByStatus exactly
    /// </summary>
    [HttpGet("statisticsAccount")]
    [Authorize]
    public async Task<IActionResult> GetAccountStatistics()
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid();
            }

            var result = await _userService.GetAccountStatisticsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAccountStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Search accounts by email (Admin only) - matches Node.js searchAccountByEmail exactly
    /// </summary>
    [HttpGet("searchAccountByEmail")]
    [Authorize]
    public async Task<IActionResult> SearchAccountByEmail([FromQuery] string searchEmail)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(searchEmail))
            {
                return BadRequest(new { message = "Không được để trống thông tin yêu cầu" });
            }

            var result = await _userService.SearchUsersByEmailAsync(searchEmail, GetUserRole().ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchAccountByEmail");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Ban account by admin - matches Node.js banAccountByAdmin exactly
    /// </summary>
    [HttpPatch("banAccountByAdmin/{accountId}")]
    [Authorize]
    public async Task<IActionResult> BanAccountByAdmin(string accountId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid();
            }

            var result = await _userService.BanAccountAsync(accountId, GetUserRole().ToString());
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("admin") || result.Message.Contains("khóa"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BanAccountByAdmin");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Unban account by admin - matches Node.js unBanAccountByAdmin exactly
    /// </summary>
    [HttpPatch("unBanAccountByAdmin/{accountId}")]
    [Authorize]
    public async Task<IActionResult> UnbanAccountByAdmin(string accountId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid();
            }

            var result = await _userService.UnbanAccountAsync(accountId, GetUserRole().ToString());
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("admin") || result.Message.Contains("không bị khóa"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnbanAccountByAdmin");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update user info for admin - matches Node.js updateUserInfoForAdmin exactly
    /// </summary>
    [HttpPut("admin/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUserInfoForAdmin(string id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid();
            }

            var result = await _userService.UpdateUserForAdminAsync(id, dto, GetUserRole().ToString());
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateUserInfoForAdmin");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Forgot password - matches Node.js forgotPassword exactly
    /// </summary>
    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new { message = "Email không hợp lệ" });
            }

            var result = await _userService.ForgotPasswordAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "OTP đã được gửi đến email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ForgotPassword");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Verify OTP - matches Node.js verifyOtp exactly
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email) || dto.Otp == 0)
            {
                return BadRequest(new { message = "Email hoặc OTP không hợp lệ" });
            }

            var result = await _userService.VerifyOtpAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("sai") || result.Message.Contains("hết hạn"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "OTP hợp lệ" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyOtp");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Reset password - matches Node.js resetPassword exactly
    /// </summary>
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.NewPassword))
            {
                return BadRequest(new { message = "Email hoặc mật khẩu mới không hợp lệ" });
            }

            var result = await _userService.ResetPasswordAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResetPassword");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    // /// <summary>
    // /// Upgrade membership by account balance - matches Node.js upMembershipByAccountBalance exactly
    // /// </summary>
    // [HttpPost("upMembershipByAccountBalance")]
    // [Authorize]
    // public async Task<IActionResult> UpgradeMembershipByAccountBalance([FromBody] UpgradeMembershipDto dto)
    // {
    //     try
    //     {
    //         var result = await _userService.UpgradeMembershipByAccountBalanceAsync(dto);
    //         if (!result.Success)
    //         {
    //             if (result.Message.Contains("Không tìm thấy"))
    //                 return NotFound(new { message = result.Message });
    //             if (result.Message.Contains("không đủ"))
    //                 return BadRequest(new { message = result.Message });
    //             return BadRequest(new { message = result.Message });
    //         }
    //         return Ok(result.Data);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error in UpgradeMembershipByAccountBalance");
    //         return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
    //     }
    // }

    #region Helper Methods

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    private UserRole GetUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Customer;
    }

    #endregion
}