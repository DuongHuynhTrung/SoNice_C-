namespace SoNice.Application.DTOs;

/// <summary>
/// Create voucher usage DTO
/// </summary>
public class CreateVoucherUsageDto
{
    public List<string> VoucherList { get; set; } = new();
    public decimal DiscountAmount { get; set; }
}

/// <summary>
/// Voucher usage response DTO
/// </summary>
public class VoucherUsageResponseDto
{
    public string Id { get; set; } = string.Empty;
    public List<string> VoucherList { get; set; } = new();
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Update voucher usage DTO
/// </summary>
public class UpdateVoucherUsageDto
{
    public List<string>? VoucherList { get; set; }
    public decimal? DiscountAmount { get; set; }
}
