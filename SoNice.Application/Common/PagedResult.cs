namespace SoNice.Application.Common;

/// <summary>
/// Paged result wrapper for paginated data
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
