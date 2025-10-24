using MongoDB.Bson.Serialization.Attributes;
using SoNice.Domain.Enums;

namespace SoNice.Domain.Entities;

/// <summary>
/// Order domain entity - matches Node.js Order model exactly
/// </summary>
public class Order : BaseEntity
{
    [BsonElement("user_id")]
    public string? UserId { get; set; }

    [BsonElement("order_item_list")]
    public List<string> OrderItemList { get; set; } = new();

    [BsonElement("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [BsonElement("total_amount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [BsonElement("payment_method")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public OrderPaymentMethod PaymentMethod { get; set; }

    [BsonElement("shipping_address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [BsonElement("customer_name")]
    public string CustomerName { get; set; } = string.Empty;

    [BsonElement("customer_phone")]
    public string CustomerPhone { get; set; } = string.Empty;

    [BsonElement("customer_email")]
    public string? CustomerEmail { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("cancellation_reason")]
    public string? CancellationReason { get; set; }

    [BsonElement("voucher_usage_id")]
    public string? VoucherUsageId { get; set; }
}
