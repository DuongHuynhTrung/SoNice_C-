namespace SoNice.Application.DTOs;

/// <summary>
/// Create product DTO
/// </summary>
public class CreateProductDto
{
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; } = 0;
    public List<string> ImageUrlList { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update product DTO
/// </summary>
public class UpdateProductDto
{
    public string? CategoryId { get; set; }
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public int? StockQuantity { get; set; }
    public List<string>? ImageUrlList { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Product response DTO
/// </summary>
public class ProductResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; }
    public List<string> ImageUrlList { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
