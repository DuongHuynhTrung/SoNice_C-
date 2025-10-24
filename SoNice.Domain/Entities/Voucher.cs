using MongoDB.Bson.Serialization.Attributes;
using SoNice.Domain.Enums;

namespace SoNice.Domain.Entities;

/// <summary>
/// Voucher domain entity - matches Node.js Voucher model exactly
/// </summary>
public class Voucher : BaseEntity
{
    [BsonElement("code")]
    public string Code { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("type")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public VoucherType Type { get; set; }

    [BsonElement("value")]
    public decimal Value { get; set; }

    [BsonElement("usage_limit")]
    public int? UsageLimit { get; set; }

    [BsonElement("used_count")]
    public int UsedCount { get; set; } = 0;

    [BsonElement("can_stack")]
    public bool CanStack { get; set; } = false;

    [BsonElement("start_date")]
    public DateTime? StartDate { get; set; }

    [BsonElement("end_date")]
    public DateTime? EndDate { get; set; }

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;
}