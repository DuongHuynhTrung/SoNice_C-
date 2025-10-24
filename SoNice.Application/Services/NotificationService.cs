using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Domain.Enums;

namespace SoNice.Application.Services;

/// <summary>
/// Notification service implementation - matches Node.js NotificationController logic exactly
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<NotificationResponseDto>>> GetUserNotificationsAsync(string userId, int page = 1, int limit = 10)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            
            // Pagination
            var total = notifications.Count;
            var skip = (page - 1) * limit;
            var pagedNotifications = notifications.Skip(skip).Take(limit).ToList();

            var notificationDtos = pagedNotifications.Select(MapToResponseDto).ToList();

            var result = new PagedResult<NotificationResponseDto>
            {
                Data = notificationDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<NotificationResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserNotificationsAsync");
            return ServiceResult<PagedResult<NotificationResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<NotificationResponseDto>> GetNotificationByIdAsync(string id)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
            {
                return ServiceResult<NotificationResponseDto>.Failure("Không tìm thấy thông báo với ID đã cho");
            }

            var notificationDto = MapToResponseDto(notification);
            return ServiceResult<NotificationResponseDto>.SuccessResult(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNotificationByIdAsync");
            return ServiceResult<NotificationResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<NotificationResponseDto>> CreateNotificationAsync(CreateNotificationDto dto)
    {
        try
        {
            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return ServiceResult<NotificationResponseDto>.Failure("Không tìm thấy người dùng với ID đã cho");
            }

            var notification = new Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Content = dto.Content,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            var notificationDto = MapToResponseDto(notification);
            return ServiceResult<NotificationResponseDto>.SuccessResult(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateNotificationAsync");
            return ServiceResult<NotificationResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<NotificationResponseDto>> UpdateNotificationAsync(string id, UpdateNotificationDto dto)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
            {
                return ServiceResult<NotificationResponseDto>.Failure("Không tìm thấy thông báo để cập nhật");
            }

            // Update fields exactly like Node.js
            if (dto.IsRead.HasValue)
                notification.IsRead = dto.IsRead.Value;

            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            var notificationDto = MapToResponseDto(notification);
            return ServiceResult<NotificationResponseDto>.SuccessResult(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateNotificationAsync");
            return ServiceResult<NotificationResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<NotificationResponseDto>> DeleteNotificationAsync(string id)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
            {
                return ServiceResult<NotificationResponseDto>.Failure("Không tìm thấy thông báo để xóa");
            }

            await _unitOfWork.Notifications.DeleteAsync(notification.Id);
            await _unitOfWork.SaveChangesAsync();

            var notificationDto = MapToResponseDto(notification);
            return ServiceResult<NotificationResponseDto>.SuccessResult(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteNotificationAsync");
            return ServiceResult<NotificationResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<bool>> MarkAllAsReadAsync(string userId)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                await _unitOfWork.Notifications.UpdateAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MarkAllAsReadAsync");
            return ServiceResult<bool>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<int>> GetUnreadCountAsync(string userId)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            var unreadCount = notifications.Count(n => !n.IsRead);

            return ServiceResult<int>.SuccessResult(unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUnreadCountAsync");
            return ServiceResult<int>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<NotificationResponseDto>> CreateAndEmitNotificationAsync(string userId, string type, string content)
    {
        try
        {
            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<NotificationResponseDto>.Failure("Không tìm thấy người dùng với ID đã cho");
            }

            // Validate notification type
            if (!Enum.TryParse<NotificationType>(type, out var notificationType))
            {
                return ServiceResult<NotificationResponseDto>.Failure("Loại thông báo không hợp lệ");
            }

            var notification = new Notification
            {
                UserId = userId,
                Type = notificationType,
                Content = content,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Emit notification via SignalR (equivalent to Socket.IO in Node.js)
            // This would be implemented with SignalR hub
            await EmitNotificationAsync(userId, notification);

            var notificationDto = MapToResponseDto(notification);
            return ServiceResult<NotificationResponseDto>.SuccessResult(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAndEmitNotificationAsync");
            return ServiceResult<NotificationResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private async Task EmitNotificationAsync(string userId, Notification notification)
    {
        // This would integrate with SignalR hub to emit notifications
        // Equivalent to Socket.IO in Node.js
        // Implementation would depend on SignalR setup
        _logger.LogInformation($"Emitting notification to user {userId}: {notification.Content}");
    }

    private NotificationResponseDto MapToResponseDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Content = notification.Content,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }

    #endregion
}
