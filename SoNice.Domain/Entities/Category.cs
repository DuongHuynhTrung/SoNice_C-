using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// Category domain entity - matches Node.js Category model exactly
/// </summary>
public class Category : BaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;
}