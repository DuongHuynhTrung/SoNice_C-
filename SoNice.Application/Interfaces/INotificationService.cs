using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Notification service interface - matches Node.js NotificationController logic
/// </summary>
public interface INotificationService
{
    Task<ServiceResult<PagedResult<NotificationResponseDto>>> GetUserNotificationsAsync(string userId, int page = 1, int limit = 10);
    Task<ServiceResult<NotificationResponseDto>> GetNotificationByIdAsync(string id);
    Task<ServiceResult<NotificationResponseDto>> CreateNotificationAsync(CreateNotificationDto dto);
    Task<ServiceResult<NotificationResponseDto>> UpdateNotificationAsync(string id, UpdateNotificationDto dto);
    Task<ServiceResult<NotificationResponseDto>> DeleteNotificationAsync(string id);
    Task<ServiceResult<bool>> MarkAllAsReadAsync(string userId);
    Task<ServiceResult<int>> GetUnreadCountAsync(string userId);
    Task<ServiceResult<NotificationResponseDto>> CreateAndEmitNotificationAsync(string userId, string type, string content);
}
