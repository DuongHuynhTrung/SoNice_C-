using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Blog controller - matches Node.js BlogController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ILogger<BlogController> _logger;

    public BlogController(IBlogService blogService, ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    /// Get all blogs with pagination - matches Node.js getAllBlogs exactly
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllBlogs([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _blogService.GetAllBlogsAsync(page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllBlogs");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get blog by ID - matches Node.js getBlogById exactly
    /// </summary>
    [HttpGet("{blogId}")]
    public async Task<IActionResult> GetBlogById(string blogId)
    {
        try
        {
            var result = await _blogService.GetBlogByIdAsync(blogId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy blog với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetBlogById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new blog (Admin only) - matches Node.js createBlog exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            if (string.IsNullOrEmpty(dto.Title) || string.IsNullOrEmpty(dto.Content))
            {
                return BadRequest(new { message = "Vui lòng cung cấp đầy đủ tiêu đề và nội dung blog" });
            }

            var result = await _blogService.CreateBlogAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateBlog");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update blog by ID (Admin only) - matches Node.js updateBlogById exactly
    /// </summary>
    [HttpPut("{blogId}")]
    [Authorize]
    public async Task<IActionResult> UpdateBlogById(string blogId, [FromBody] UpdateBlogDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _blogService.UpdateBlogAsync(blogId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy blog để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateBlogById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete blog by ID (Admin only) - matches Node.js deleteBlogById exactly
    /// </summary>
    [HttpDelete("{blogId}")]
    [Authorize]
    public async Task<IActionResult> DeleteBlogById(string blogId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _blogService.DeleteBlogAsync(blogId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy blog để xóa" });
            }
            return Ok(new { message = "Đã xóa blog thành công", blog = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteBlogById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    #region Helper Methods

    private UserRole GetUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Customer;
    }

    #endregion
}
