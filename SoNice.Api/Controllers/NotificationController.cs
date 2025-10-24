using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Notification controller - matches Node.js NotificationController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all notifications for current user - matches Node.js getNotifications exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId, page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNotifications");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get notification by ID - matches Node.js getNotificationById exactly
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetNotificationById(string id)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _notificationService.GetNotificationByIdAsync(id);
            if (!result.Success)
            {
                if (result.Message.Contains("không tồn tại"))
                    return NotFound(new { message = "Thông báo không tồn tại" });
                if (result.Message.Contains("quyền"))
                    return Forbid("Bạn không có quyền xem thông báo này");
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNotificationById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new notification (Admin only) - matches Node.js createNotification exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền tạo thông báo");
            }

            if (string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Content))
            {
                return BadRequest(new { message = "Vui lòng cung cấp đầy đủ thông tin thông báo" });
            }

            var result = await _notificationService.CreateNotificationAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("không tồn tại"))
                    return NotFound(new { message = "Người dùng không tồn tại" });
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateNotification");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update notification (mark as read) - matches Node.js updateNotification exactly
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateNotification(string id, [FromBody] UpdateNotificationDto dto)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _notificationService.UpdateNotificationAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("không tồn tại"))
                    return NotFound(new { message = "Thông báo không tồn tại" });
                if (result.Message.Contains("quyền"))
                    return Forbid("Bạn không có quyền cập nhật thông báo này");
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = "Cập nhật thông báo thành công", notification = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateNotification");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete notification - matches Node.js deleteNotification exactly
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _notificationService.DeleteNotificationAsync(id);
            if (!result.Success)
            {
                if (result.Message.Contains("không tồn tại"))
                    return NotFound(new { message = "Thông báo không tồn tại" });
                if (result.Message.Contains("quyền"))
                    return Forbid("Bạn không có quyền xóa thông báo này");
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = "Xóa thông báo thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteNotification");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Mark all notifications as read - matches Node.js markAllAsRead exactly
    /// </summary>
    [HttpPut("mark-all-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = "Đã đánh dấu tất cả thông báo là đã đọc" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MarkAllAsRead");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get unread notifications count - matches Node.js getUnreadCount exactly
    /// </summary>
    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new UnreadCountResponseDto { UnreadCount = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUnreadCount");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    #region Helper Methods

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private UserRole GetUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Customer;
    }

    #endregion
}
