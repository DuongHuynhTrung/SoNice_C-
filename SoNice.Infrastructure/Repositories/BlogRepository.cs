using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Blog repository implementation with specific blog operations
/// </summary>
public class BlogRepository : Repository<Blog>, IBlogRepository
{
    public BlogRepository(MongoDbContext context, ILogger<BlogRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Blog>> GetPublishedBlogsAsync(int page, int limit)
    {
        try
        {
            var filter = Builders<Blog>.Filter.Eq(x => x.IsPublished, true);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published blogs");
            throw;
        }
    }

    public async Task<int> GetPublishedBlogsCountAsync()
    {
        try
        {
            var filter = Builders<Blog>.Filter.Eq(x => x.IsPublished, true);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published blogs count");
            throw;
        }
    }
}
