using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// Product domain entity - matches Node.js Product model exactly
/// </summary>
public class Product : BaseEntity
{
    [BsonElement("category_id")]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("stock_quantity")]
    public int StockQuantity { get; set; } = 0;

    [BsonElement("img_url_list")]
    public List<string> ImageUrlList { get; set; } = new();

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;
}
