using SoNice.Domain.Enums;

namespace SoNice.Application.DTOs;

/// <summary>
/// Create voucher DTO
/// </summary>
public class CreateVoucherDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VoucherType Type { get; set; }
    public decimal Value { get; set; }
    public int? UsageLimit { get; set; }
    public bool CanStack { get; set; } = false;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update voucher DTO
/// </summary>
public class UpdateVoucherDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public VoucherType? Type { get; set; }
    public decimal? Value { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsedCount { get; set; }
    public bool? CanStack { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Voucher response DTO
/// </summary>
public class VoucherResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VoucherType Type { get; set; }
    public decimal Value { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool CanStack { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
