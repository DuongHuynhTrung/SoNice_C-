using SoNice.Domain.Enums;

namespace SoNice.Application.DTOs;

/// <summary>
/// Create notification DTO
/// </summary>
public class CreateNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Update notification DTO
/// </summary>
public class UpdateNotificationDto
{
    public bool? IsRead { get; set; }
}

/// <summary>
/// Notification response DTO
/// </summary>
public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Unread count response DTO
/// </summary>
public class UnreadCountResponseDto
{
    public int UnreadCount { get; set; }
}
