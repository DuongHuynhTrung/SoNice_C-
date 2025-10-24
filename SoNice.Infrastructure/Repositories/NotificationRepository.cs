using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Notification repository implementation with specific notification operations
/// </summary>
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(MongoDbContext context, ILogger<NotificationRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId, int page, int limit)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUnreadNotificationsCountByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(x => x.UserId, userId),
                Builders<Notification>.Filter.Eq(x => x.IsRead, false)
            );
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications count by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task MarkAllAsReadByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(x => x.UserId, userId),
                Builders<Notification>.Filter.Eq(x => x.IsRead, false)
            );
            var update = Builders<Notification>.Update.Set(x => x.IsRead, true);
            await _collection.UpdateManyAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by user ID: {UserId}", userId);
            throw;
        }
    }
}
