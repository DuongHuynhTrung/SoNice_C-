using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Order controller - matches Node.js OrderController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with pagination - matches Node.js getAllOrders exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _orderService.GetAllOrdersAsync(page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllOrders");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get order by ID - matches Node.js getOrderById exactly
    /// </summary>
    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(string orderId)
    {
        try
        {
            var userId = GetUserId();
            var userRole = GetUserRole();
            var isAdmin = userRole == UserRole.Admin;

            var result = await _orderService.GetOrderByIdAsync(orderId);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy order với ID đã cho" });
                if (result.Message.Contains("quyền"))
                    return Forbid("Bạn không có quyền xem đơn hàng này");
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new order - matches Node.js createOrder exactly
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            if (!dto.OrderItemList.Any())
            {
                return BadRequest(new { message = "order_item_list bắt buộc và phải có ít nhất 1 mục" });
            }

            if (string.IsNullOrEmpty(dto.PaymentMethod.ToString()))
            {
                return BadRequest(new { message = "payment_method là bắt buộc" });
            }

            if (string.IsNullOrEmpty(dto.ShippingAddress) || string.IsNullOrEmpty(dto.CustomerName) || string.IsNullOrEmpty(dto.CustomerPhone))
            {
                return BadRequest(new { message = "Thiếu shipping_address, customer_name hoặc customer_phone" });
            }

            var result = await _orderService.CreateOrderAsync(dto);
            if (!result.Success)
            {
                if (result.Message.Contains("không hợp lệ") || result.Message.Contains("phải là mảng"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }

            // Generate payment link if needed
            var paymentLinkResult = await _orderService.GeneratePaymentLinkAsync(result.Data.Id);
            var checkoutUrl = paymentLinkResult.Success ? paymentLinkResult.Data : string.Empty;

            return StatusCode(201, new CreateOrderResponseDto
            {
                Order = result.Data!,
                CheckoutUrl = checkoutUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateOrder");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update order by ID - matches Node.js updateOrderById exactly
    /// </summary>
    [HttpPut("{orderId}")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderById(string orderId, [FromBody] UpdateOrderDto dto)
    {
        try
        {
            var result = await _orderService.UpdateOrderAsync(orderId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy order để cập nhật" });
                if (result.Message.Contains("hủy") || result.Message.Contains("bắt buộc"))
                    return BadRequest(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateOrderById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete order by ID (Admin only) - matches Node.js deleteOrderById exactly
    /// </summary>
    [HttpDelete("{orderId}")]
    [Authorize]
    public async Task<IActionResult> DeleteOrderById(string orderId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _orderService.DeleteOrderAsync(orderId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy order để xóa" });
            }
            return Ok(new { message = "Đã xóa order thành công", order = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteOrderById");
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
