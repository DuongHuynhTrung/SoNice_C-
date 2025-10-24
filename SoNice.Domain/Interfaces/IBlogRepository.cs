using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Blog repository interface with specific blog operations
/// </summary>
public interface IBlogRepository : IRepository<Blog>
{
    Task<IEnumerable<Blog>> GetPublishedBlogsAsync(int page, int limit);
    Task<int> GetPublishedBlogsCountAsync();
}
