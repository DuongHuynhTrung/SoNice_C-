using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Cart repository implementation with specific cart operations
/// </summary>
public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(MongoDbContext context, ILogger<CartRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Cart>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Cart>> GetCartsByUserIdAsync(string userId, int page, int limit)
    {
        try
        {
            var filter = Builders<Cart>.Filter.Eq(x => x.UserId, userId);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting carts by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetCartsCountByUserIdAsync(string userId)
    {
        try
        {
            var filter = Builders<Cart>.Filter.Eq(x => x.UserId, userId);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting carts count by user ID: {UserId}", userId);
            throw;
        }
    }
}
