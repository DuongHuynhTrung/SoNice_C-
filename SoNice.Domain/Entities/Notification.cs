using MongoDB.Bson.Serialization.Attributes;
using SoNice.Domain.Enums;

namespace SoNice.Domain.Entities;

/// <summary>
/// Notification domain entity - matches Node.js Notification model exactly
/// </summary>
public class Notification : BaseEntity
{
    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public NotificationType Type { get; set; }

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("is_read")]
    public bool IsRead { get; set; } = false;
}