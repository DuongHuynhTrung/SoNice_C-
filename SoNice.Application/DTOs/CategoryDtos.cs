namespace SoNice.Application.DTOs;

/// <summary>
/// Create category DTO
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update category DTO
/// </summary>
public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Category response DTO
/// </summary>
public class CategoryResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
