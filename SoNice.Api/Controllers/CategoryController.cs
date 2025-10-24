using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Category controller - matches Node.js CategoryController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories with pagination - matches Node.js getAllCategories exactly
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCategories");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get category by ID - matches Node.js getCategoryById exactly
    /// </summary>
    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategoryById(string categoryId)
    {
        try
        {
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy category với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCategoryById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new category (Admin only) - matches Node.js createCategory exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            if (string.IsNullOrEmpty(dto.Name))
            {
                return BadRequest(new { message = "Vui lòng cung cấp tên danh mục" });
            }

            var result = await _categoryService.CreateCategoryAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateCategory");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update category by ID (Admin only) - matches Node.js updateCategoryById exactly
    /// </summary>
    [HttpPut("{categoryId}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategoryById(string categoryId, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _categoryService.UpdateCategoryAsync(categoryId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy category để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateCategoryById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete category by ID (Admin only) - matches Node.js deleteCategoryById exactly
    /// </summary>
    [HttpDelete("{categoryId}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategoryById(string categoryId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _categoryService.DeleteCategoryAsync(categoryId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy category để xóa" });
            }
            return Ok(new { message = "Đã xóa category thành công", category = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteCategoryById");
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
