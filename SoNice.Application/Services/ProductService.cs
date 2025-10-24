using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Domain.Enums;

namespace SoNice.Application.Services;

/// <summary>
/// Product service implementation - matches Node.js ProductController logic exactly
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetAllProductsAsync(int page = 1, int limit = 10, string? categoryId = null, string? search = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        try
        {
            var products = (await _unitOfWork.Products.GetAllAsync()).ToList();
            
            // Apply filters exactly like Node.js
            if (!string.IsNullOrEmpty(categoryId))
            {
                products = products.Where(p => p.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.ToLower().Contains(search.ToLower())).ToList();
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Amount >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Amount <= maxPrice.Value).ToList();
            }

            // Pagination
            var total = products.Count;
            var skip = (page - 1) * limit;
            var pagedProducts = products.Skip(skip).Take(limit).ToList();

            var productDtos = pagedProducts.Select((Product p) => MapToResponseDto(p)).ToList();

            var result = new PagedResult<ProductResponseDto>
            {
                Data = productDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<ProductResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllProductsAsync");
            return ServiceResult<PagedResult<ProductResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(string id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductResponseDto>.Failure("Không tìm thấy sản phẩm với ID đã cho");
            }

            var productDto = MapToResponseDto(product);
            return ServiceResult<ProductResponseDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductByIdAsync");
            return ServiceResult<ProductResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<ProductResponseDto>> CreateProductAsync(CreateProductDto dto)
    {
        try
        {
            // Validate category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return ServiceResult<ProductResponseDto>.Failure("Không tìm thấy danh mục với ID đã cho");
            }

            var product = new Product
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Amount = dto.Amount,
                Description = dto.Description,
                StockQuantity = dto.StockQuantity,
                ImageUrlList = dto.ImageUrlList ?? new List<string>(),
                IsActive = dto.IsActive
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = MapToResponseDto(product);
            return ServiceResult<ProductResponseDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateProductAsync");
            return ServiceResult<ProductResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<ProductResponseDto>> UpdateProductAsync(string id, UpdateProductDto dto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductResponseDto>.Failure("Không tìm thấy sản phẩm để cập nhật");
            }

            // Validate category exists if provided
            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    return ServiceResult<ProductResponseDto>.Failure("Không tìm thấy danh mục với ID đã cho");
                }
                product.CategoryId = dto.CategoryId;
            }

            // Update fields exactly like Node.js
            if (!string.IsNullOrEmpty(dto.Name))
                product.Name = dto.Name;
            if (dto.Amount.HasValue)
                product.Amount = dto.Amount.Value;
            if (!string.IsNullOrEmpty(dto.Description))
                product.Description = dto.Description;
            if (dto.StockQuantity.HasValue)
                product.StockQuantity = dto.StockQuantity.Value;
            if (dto.ImageUrlList != null)
                product.ImageUrlList = dto.ImageUrlList;
            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = MapToResponseDto(product);
            return ServiceResult<ProductResponseDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateProductAsync");
            return ServiceResult<ProductResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<ProductResponseDto>> DeleteProductAsync(string id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult<ProductResponseDto>.Failure("Không tìm thấy sản phẩm để xóa");
            }

            await _unitOfWork.Products.DeleteAsync(product.Id);
            await _unitOfWork.SaveChangesAsync();

            var productDto = MapToResponseDto(product);
            return ServiceResult<ProductResponseDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteProductAsync");
            return ServiceResult<ProductResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Amount = product.Amount,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            ImageUrlList = product.ImageUrlList,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    #endregion
}
