using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Blog service interface - matches Node.js BlogController logic
/// </summary>
public interface IBlogService
{
    Task<ServiceResult<PagedResult<BlogResponseDto>>> GetAllBlogsAsync(int page = 1, int limit = 10);
    Task<ServiceResult<BlogResponseDto>> GetBlogByIdAsync(string id);
    Task<ServiceResult<BlogResponseDto>> CreateBlogAsync(CreateBlogDto dto);
    Task<ServiceResult<BlogResponseDto>> UpdateBlogAsync(string id, UpdateBlogDto dto);
    Task<ServiceResult<BlogResponseDto>> DeleteBlogAsync(string id);
}
