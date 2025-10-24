using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// OrderItem repository implementation with specific order item operations
/// </summary>
public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    private readonly MongoDbContext _context;

    public OrderItemRepository(MongoDbContext context, ILogger<OrderItemRepository> logger) 
        : base(context, logger)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId)
    {
        try
        {
            // First get the order to get the order item IDs
            var order = await _context.Orders.Find(Builders<Order>.Filter.Eq(x => x.Id, orderId)).FirstOrDefaultAsync();
            if (order == null || !order.OrderItemList.Any())
            {
                return new List<OrderItem>();
            }

            // Then get the order items by their IDs
            var filter = Builders<OrderItem>.Filter.In(x => x.Id, order.OrderItemList);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order items by order ID: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByProductIdAsync(string productId)
    {
        try
        {
            var filter = Builders<OrderItem>.Filter.Eq(x => x.ProductId, productId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order items by product ID: {ProductId}", productId);
            throw;
        }
    }
}
