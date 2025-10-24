using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Category repository implementation with specific category operations
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(MongoDbContext context, ILogger<CategoryRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(int page, int limit)
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(x => x.IsActive, true);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            throw;
        }
    }

    public async Task<int> GetActiveCategoriesCountAsync()
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(x => x.IsActive, true);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories count");
            throw;
        }
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        try
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Name, name);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by name: {Name}", name);
            throw;
        }
    }
}
