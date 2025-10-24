using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// OrderItem domain entity - matches Node.js OrderItem model exactly
/// </summary>
public class OrderItem : BaseEntity
{
    [BsonElement("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("total_price")]
    public decimal TotalPrice { get; set; }
}