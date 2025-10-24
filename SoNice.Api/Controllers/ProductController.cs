using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Product controller - matches Node.js ProductController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination and filtering - matches Node.js getAllProducts exactly
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        try
        {
            var result = await _productService.GetAllProductsAsync(page, limit, categoryId, search, minPrice, maxPrice);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllProducts");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get product by ID - matches Node.js getProductById exactly
    /// </summary>
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductById(string productId)
    {
        try
        {
            var result = await _productService.GetProductByIdAsync(productId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy product với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new product (Admin only) - matches Node.js createProduct exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _productService.CreateProductAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateProduct");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update product by ID (Admin only) - matches Node.js updateProductById exactly
    /// </summary>
    [HttpPut("{productId}")]
    [Authorize]
    public async Task<IActionResult> UpdateProductById(string productId, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _productService.UpdateProductAsync(productId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy product để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateProductById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete product by ID (Admin only) - matches Node.js deleteProductById exactly
    /// </summary>
    [HttpDelete("{productId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductById(string productId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _productService.DeleteProductAsync(productId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy product để xóa" });
            }
            return Ok(new { message = "Đã xóa product thành công", product = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteProductById");
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
