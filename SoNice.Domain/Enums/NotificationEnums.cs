namespace SoNice.Domain.Enums;

/// <summary>
/// Notification type enumeration - matches Node.js NotificationTypeEnum exactly
/// </summary>
public enum NotificationType
{
    OrderRequested = 0,
    OrderConfirmed = 1,
    OrderProcessing = 2,
    OrderShipping = 3,
    OrderDelivered = 4
}