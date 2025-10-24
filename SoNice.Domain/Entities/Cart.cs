using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// Cart domain entity - matches Node.js Cart model exactly
/// </summary>
public class Cart : BaseEntity
{
    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("cart_items_list")]
    public List<CartItem> CartItemsList { get; set; } = new();
}

/// <summary>
/// CartItem embedded document - matches Node.js cart item structure exactly
/// </summary>
public class CartItem
{
    [BsonElement("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }
}