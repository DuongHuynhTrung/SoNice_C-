using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// Blog service implementation - matches Node.js BlogController logic exactly
/// </summary>
public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlogService> _logger;

    public BlogService(IUnitOfWork unitOfWork, ILogger<BlogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<BlogResponseDto>>> GetAllBlogsAsync(int page = 1, int limit = 10)
    {
        try
        {
            var blogs = await _unitOfWork.Blogs.GetAllAsync();
            var blogsList = blogs.ToList();
            
            // Pagination
            var total = blogsList.Count;
            var skip = (page - 1) * limit;
            var pagedBlogs = blogsList.Skip(skip).Take(limit).ToList();

            var blogDtos = pagedBlogs.Select(MapToResponseDto).ToList();

            var result = new PagedResult<BlogResponseDto>
            {
                Data = blogDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<BlogResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllBlogsAsync");
            return ServiceResult<PagedResult<BlogResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<BlogResponseDto>> GetBlogByIdAsync(string id)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
            {
                return ServiceResult<BlogResponseDto>.Failure("Không tìm thấy blog với ID đã cho");
            }

            var blogDto = MapToResponseDto(blog);
            return ServiceResult<BlogResponseDto>.SuccessResult(blogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetBlogByIdAsync");
            return ServiceResult<BlogResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<BlogResponseDto>> CreateBlogAsync(CreateBlogDto dto)
    {
        try
        {
            var blog = new Blog
            {
                Title = dto.Title,
                Content = dto.Content,
                CoverUrl = dto.CoverUrl,
                IsPublished = dto.IsPublished
            };

            await _unitOfWork.Blogs.AddAsync(blog);
            await _unitOfWork.SaveChangesAsync();

            var blogDto = MapToResponseDto(blog);
            return ServiceResult<BlogResponseDto>.SuccessResult(blogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateBlogAsync");
            return ServiceResult<BlogResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<BlogResponseDto>> UpdateBlogAsync(string id, UpdateBlogDto dto)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
            {
                return ServiceResult<BlogResponseDto>.Failure("Không tìm thấy blog để cập nhật");
            }

            // Update fields exactly like Node.js
            if (!string.IsNullOrEmpty(dto.Title))
                blog.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Content))
                blog.Content = dto.Content;
            if (!string.IsNullOrEmpty(dto.CoverUrl))
                blog.CoverUrl = dto.CoverUrl;
            if (dto.IsPublished.HasValue)
                blog.IsPublished = dto.IsPublished.Value;

            await _unitOfWork.Blogs.UpdateAsync(blog);
            await _unitOfWork.SaveChangesAsync();

            var blogDto = MapToResponseDto(blog);
            return ServiceResult<BlogResponseDto>.SuccessResult(blogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateBlogAsync");
            return ServiceResult<BlogResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<BlogResponseDto>> DeleteBlogAsync(string id)
    {
        try
        {
            var blog = await _unitOfWork.Blogs.GetByIdAsync(id);
            if (blog == null)
            {
                return ServiceResult<BlogResponseDto>.Failure("Không tìm thấy blog để xóa");
            }

            await _unitOfWork.Blogs.DeleteAsync(blog.Id);
            await _unitOfWork.SaveChangesAsync();

            var blogDto = MapToResponseDto(blog);
            return ServiceResult<BlogResponseDto>.SuccessResult(blogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteBlogAsync");
            return ServiceResult<BlogResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private BlogResponseDto MapToResponseDto(Blog blog)
    {
        return new BlogResponseDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            CoverUrl = blog.CoverUrl,
            IsPublished = blog.IsPublished,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }

    #endregion
}
