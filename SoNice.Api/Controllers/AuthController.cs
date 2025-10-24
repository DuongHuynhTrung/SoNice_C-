using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Authentication controller - matches Node.js AuthController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user - matches Node.js registerUser exactly
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { message = "Tất cả các trường không được để trống!" });
            }

            var result = await _userService.RegisterUserAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new RegisterResponseDto
            {
                Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Register");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Login user - matches Node.js login exactly
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Identifier) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { message = "Tất cả các trường không được để trống!" });
            }

            var result = await _userService.LoginUserAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                if (result.Message.Contains("khóa") || result.Message.Contains("xác thực"))
                    return Unauthorized(new { message = result.Message });
                return Unauthorized(new { message = result.Message });
            }

            // Set refresh token cookie (using access token as refresh token for now)
            Response.Cookies.Append("jwt", result.Data.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.Data.AccessToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Google login - matches Node.js loginGoogle exactly
    /// </summary>
    [HttpPost("loginGoogle")]
    public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginDto dto)
    {
        try
        {
            var result = await _userService.LoginGoogleAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("khóa"))
                    return Unauthorized(new { message = result.Message });
                return StatusCode(500, new { message = result.Message });
            }

            return Ok(new { accessToken = result.Data.AccessToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoginGoogle");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Verify email - matches Node.js verifyEmail exactly
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Token))
            {
                return BadRequest(new { message = "Token xác thực không hợp lệ!" });
            }

            var result = await _userService.VerifyEmailAsync(dto.Token);
            if (!result.Success)
            {
                if (result.Message.Contains("hết hạn") || result.Message.Contains("không hợp lệ"))
                    return BadRequest(new { message = result.Message });
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            return Ok(new VerifyEmailResponseDto
            {
                Message = result.Message,
                AccessToken = result.Data.AccessToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyEmail");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Logout - matches Node.js logout exactly
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            if (!Request.Cookies.ContainsKey("jwt"))
            {
                return NoContent();
            }

            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });

            return Ok(new LogoutResponseDto { Message = "Cookie đã được xóa" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Logout");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }
}