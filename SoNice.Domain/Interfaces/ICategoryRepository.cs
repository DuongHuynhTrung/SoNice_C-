using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Category repository interface with specific category operations
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(int page, int limit);
    Task<int> GetActiveCategoriesCountAsync();
    Task<Category?> GetByNameAsync(string name);
}
