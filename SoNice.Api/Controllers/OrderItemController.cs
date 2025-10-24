using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// OrderItem controller - matches Node.js OrderItemController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;
    private readonly ILogger<OrderItemController> _logger;

    public OrderItemController(IOrderItemService orderItemService, ILogger<OrderItemController> logger)
    {
        _orderItemService = orderItemService;
        _logger = logger;
    }

    /// <summary>
    /// Get all order items with pagination - matches Node.js getAllOrderItems exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllOrderItems([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _orderItemService.GetAllOrderItemsAsync(page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllOrderItems");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get order item by ID - matches Node.js getOrderItemById exactly
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderItemById(string id)
    {
        try
        {
            var result = await _orderItemService.GetOrderItemByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy order item với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderItemById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new order item - matches Node.js createOrderItem exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrderItem([FromBody] OrderItemDto dto)
    {
        try
        {
            var result = await _orderItemService.CreateOrderItemAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateOrderItem");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update order item by ID - matches Node.js updateOrderItemById exactly
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderItemById(string id, [FromBody] OrderItemDto dto)
    {
        try
        {
            var result = await _orderItemService.UpdateOrderItemAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy order item để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateOrderItemById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete order item by ID (Admin only) - matches Node.js deleteOrderItemById exactly
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteOrderItemById(string id)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _orderItemService.DeleteOrderItemAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy order item để xóa" });
            }
            return Ok(new { message = "Đã xóa order item thành công", orderItem = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteOrderItemById");
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
