using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with specific product operations
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(MongoDbContext context, ILogger<ProductRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId, int page, int limit)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsByNameAsync(string searchTerm, int page, int limit)
    {
        try
        {
            var filter = Builders<Product>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products by name: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int page, int limit)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(x => x.Amount, minPrice),
                Builders<Product>.Filter.Lte(x => x.Amount, maxPrice)
            );
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(int page, int limit)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(x => x.IsActive, true);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active products");
            throw;
        }
    }

    public async Task<int> GetProductsCountByCategoryAsync(string categoryId)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products count by category: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<int> GetActiveProductsCountAsync()
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(x => x.IsActive, true);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active products count");
            throw;
        }
    }

    public async Task<List<Product>> GetByCategoryIdAsync(string categoryId)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category ID: {CategoryId}", categoryId);
            throw;
        }
    }
}
