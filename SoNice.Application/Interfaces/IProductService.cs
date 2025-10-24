using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Product service interface - matches Node.js ProductController logic
/// </summary>
public interface IProductService
{
    Task<ServiceResult<PagedResult<ProductResponseDto>>> GetAllProductsAsync(int page = 1, int limit = 10, string? categoryId = null, string? search = null, decimal? minPrice = null, decimal? maxPrice = null);
    Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(string id);
    Task<ServiceResult<ProductResponseDto>> CreateProductAsync(CreateProductDto dto);
    Task<ServiceResult<ProductResponseDto>> UpdateProductAsync(string id, UpdateProductDto dto);
    Task<ServiceResult<ProductResponseDto>> DeleteProductAsync(string id);
}
