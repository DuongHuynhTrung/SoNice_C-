namespace SoNice.Application.DTOs;

/// <summary>
/// Create blog DTO
/// </summary>
public class CreateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public bool IsPublished { get; set; } = false;
}

/// <summary>
/// Update blog DTO
/// </summary>
public class UpdateBlogDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CoverUrl { get; set; }
    public bool? IsPublished { get; set; }
}

/// <summary>
/// Blog response DTO
/// </summary>
public class BlogResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
