using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Cart controller - matches Node.js CartController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Get all carts with pagination - matches Node.js getAllCarts exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllCarts([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _cartService.GetAllCartsAsync(page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCarts");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get cart by ID - matches Node.js getCartById exactly
    /// </summary>
    [HttpGet("{cartId}")]
    [Authorize]
    public async Task<IActionResult> GetCartById(string cartId)
    {
        try
        {
            var result = await _cartService.GetCartByIdAsync(cartId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy cart với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCartById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new cart - matches Node.js createCart exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCart([FromBody] CreateCartDto dto)
    {
        try
        {
            var result = await _cartService.CreateCartAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateCart");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update cart by ID - matches Node.js updateCartById exactly
    /// </summary>
    [HttpPut("{cartId}")]
    [Authorize]
    public async Task<IActionResult> UpdateCartById(string cartId, [FromBody] UpdateCartDto dto)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _cartService.UpdateCartAsync(cartId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy cart để cập nhật" });
                if (result.Message.Contains("quyền"))
                    return Forbid("Bạn không có quyền cập nhật giỏ hàng này");
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateCartById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete cart by ID (Admin only) - matches Node.js deleteCartById exactly
    /// </summary>
    [HttpDelete("{cartId}")]
    [Authorize]
    public async Task<IActionResult> DeleteCartById(string cartId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _cartService.DeleteCartAsync(cartId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy cart để xóa" });
            }
            return Ok(new { message = "Đã xóa cart thành công", cart = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteCartById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get my cart - matches Node.js getMyCart exactly
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyCart()
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.GetCurrentUserCartAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMyCart");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    #region Helper Methods

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    private UserRole GetUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Customer;
    }

    #endregion
}
