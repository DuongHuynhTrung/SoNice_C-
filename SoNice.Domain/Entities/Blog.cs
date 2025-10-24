using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// Blog domain entity - matches Node.js Blog model exactly
/// </summary>
public class Blog : BaseEntity
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("cover_url")]
    public string? CoverUrl { get; set; }

    [BsonElement("is_published")]
    public bool IsPublished { get; set; } = false;
}