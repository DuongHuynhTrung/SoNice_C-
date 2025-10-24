using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// OrderItem repository interface with specific order item operations
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId);
    Task<IEnumerable<OrderItem>> GetOrderItemsByProductIdAsync(string productId);
}
