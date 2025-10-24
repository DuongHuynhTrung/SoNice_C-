using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Cart repository interface with specific cart operations
/// </summary>
public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Cart>> GetCartsByUserIdAsync(string userId, int page, int limit);
    Task<int> GetCartsCountByUserIdAsync(string userId);
}
