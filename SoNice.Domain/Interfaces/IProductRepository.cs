using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Product repository interface with specific product operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId, int page, int limit);
    Task<IEnumerable<Product>> SearchProductsByNameAsync(string searchTerm, int page, int limit);
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int page, int limit);
    Task<IEnumerable<Product>> GetActiveProductsAsync(int page, int limit);
    Task<int> GetProductsCountByCategoryAsync(string categoryId);
    Task<int> GetActiveProductsCountAsync();
    Task<List<Product>> GetByCategoryIdAsync(string categoryId);
}
