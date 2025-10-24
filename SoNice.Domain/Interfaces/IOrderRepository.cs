using SoNice.Domain.Entities;
using SoNice.Domain.Enums;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Order repository interface with specific order operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderCodeAsync(string orderCode);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId, int page, int limit);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int page, int limit);
    Task<int> GetOrdersCountByUserIdAsync(string userId);
    Task<int> GetOrdersCountByStatusAsync(OrderStatus status);
}
