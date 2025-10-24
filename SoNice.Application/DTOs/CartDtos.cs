namespace SoNice.Application.DTOs;

/// <summary>
/// Cart item DTO
/// </summary>
public class CartItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>
/// Create cart DTO
/// </summary>
public class CreateCartDto
{
    public string UserId { get; set; } = string.Empty;
    public List<CartItemDto> CartItemsList { get; set; } = new();
}

/// <summary>
/// Update cart DTO
/// </summary>
public class UpdateCartDto
{
    public string? UserId { get; set; }
    public List<CartItemDto>? CartItemsList { get; set; }
}

/// <summary>
/// Cart response DTO
/// </summary>
public class CartResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<CartItemDto> CartItemsList { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
