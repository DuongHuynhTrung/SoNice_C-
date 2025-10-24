using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Enums;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// User repository implementation with specific user operations
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context, ILogger<UserRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(x => x.PhoneNumber, phoneNumber);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by phone number: {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task<User?> GetByEmailOrPhoneAsync(string identifier)
    {
        try
        {
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(x => x.Email, identifier),
                Builders<User>.Filter.Eq(x => x.PhoneNumber, identifier)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email or phone: {Identifier}", identifier);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, int page, int limit)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(x => x.RoleName, role);
            var skip = (page - 1) * limit;
            
            return await _collection.Find(filter)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role: {Role}", role);
            throw;
        }
    }

    public async Task<int> GetUsersCountByRoleAsync(UserRole role)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(x => x.RoleName, role);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users count by role: {Role}", role);
            throw;
        }
    }

    public async Task<IEnumerable<User>> SearchUsersByEmailAsync(string email, UserRole role)
    {
        try
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Regex(x => x.Email, new MongoDB.Bson.BsonRegularExpression(email, "i")),
                Builders<User>.Filter.Eq(x => x.RoleName, role)
            );
            
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users by email: {Email}", email);
            throw;
        }
    }
}
