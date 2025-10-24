using MongoDB.Bson.Serialization.Attributes;

namespace SoNice.Domain.Entities;

/// <summary>
/// VoucherUsage domain entity - matches Node.js VoucherUsage model exactly
/// </summary>
public class VoucherUsage : BaseEntity
{
    [BsonElement("voucher_list")]
    public List<string> VoucherList { get; set; } = new();

    [BsonElement("discount_amount")]
    public decimal DiscountAmount { get; set; }
}