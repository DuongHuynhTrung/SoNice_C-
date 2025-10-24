using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Enums;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Order repository implementation with specific order operations
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(MongoDbContext context, ILogger<OrderRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Order?> GetByOrderCodeAsync(string orderCode)
    {
        try
        {
            var filter = Builders<Order>.Filter.Eq(x => x.OrderCode, orderCode);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by order code: {OrderCode}", orderCode);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId, int page, int limit)
    {
        try
        {
            var filter = Builders<Order>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int page, int limit)
    {
        try
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Status, status);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status: {Status}", status);
            throw;
        }
    }

    public async Task<int> GetOrdersCountByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Order>.Filter.Eq(x => x.UserId, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders count by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status)
    {
        try
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Status, status);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders count by status: {Status}", status);
            throw;
        }
    }
}
