using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// User repository interface with specific user operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByEmailOrPhoneAsync(string identifier);
    Task<IEnumerable<User>> GetUsersByRoleAsync(Enums.UserRole role, int page, int limit);
    Task<int> GetUsersCountByRoleAsync(Enums.UserRole role);
    Task<IEnumerable<User>> SearchUsersByEmailAsync(string email, Enums.UserRole role);
}
