using SoNice.Domain.Enums;

namespace SoNice.Application.DTOs;

/// <summary>
/// Order item DTO for creating orders
/// </summary>
public class OrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>
/// Order item response DTO
/// </summary>
public class OrderItemResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Create order DTO
/// </summary>
public class CreateOrderDto
{
    public string? UserId { get; set; }
    public List<OrderItemDto> OrderItemList { get; set; } = new();
    public OrderPaymentMethod PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? Notes { get; set; }
    public string? OrderCode { get; set; }
    public string? VoucherUsageId { get; set; }
    public List<string> VoucherList { get; set; } = new();
}

/// <summary>
/// Update order DTO
/// </summary>
public class UpdateOrderDto
{
    public OrderStatus? Status { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Order response DTO
/// </summary>
public class OrderResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public List<OrderItemResponseDto> OrderItemList { get; set; } = new();
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public OrderPaymentMethod PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public string? VoucherUsageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Create order response DTO
/// </summary>
public class CreateOrderResponseDto
{
    public OrderResponseDto Order { get; set; } = new();
    public string CheckoutUrl { get; set; } = string.Empty;
}
