using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Notification repository interface with specific notification operations
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId, int page, int limit);
    Task<int> GetUnreadNotificationsCountByUserIdAsync(string userId);
    Task MarkAllAsReadByUserIdAsync(string userId);
    Task<List<Notification>> GetByUserIdAsync(string userId);
}
