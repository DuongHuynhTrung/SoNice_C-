using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for MongoDB
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(MongoDbContext context, ILogger<Repository<T>> logger)
    {
        _logger = logger;
        _collection = GetCollection(context);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID: {Id}", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _collection.Find(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate");
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first entity with predicate");
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity");
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity: {Id}", entity.Id);
            throw;
        }
    }

    public virtual async Task DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity: {Id}", id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(predicate);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking entity existence");
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            if (predicate == null)
                return (int)await _collection.CountDocumentsAsync(_ => true);
            
            return (int)await _collection.CountDocumentsAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities");
            throw;
        }
    }

    protected virtual IMongoCollection<T> GetCollection(MongoDbContext context)
    {
        var collectionName = typeof(T).Name.ToLower() + "s";
        return context._database.GetCollection<T>(collectionName);
    }
}
