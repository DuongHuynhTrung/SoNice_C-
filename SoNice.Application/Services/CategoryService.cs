using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// Category service implementation - matches Node.js CategoryController logic exactly
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<List<CategoryResponseDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoryDtos = categories.Select(MapToResponseDto).ToList();

            return ServiceResult<List<CategoryResponseDto>>.SuccessResult(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCategoriesAsync");
            return ServiceResult<List<CategoryResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CategoryResponseDto>> GetCategoryByIdAsync(string id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryResponseDto>.Failure("Không tìm thấy danh mục với ID đã cho");
            }

            var categoryDto = MapToResponseDto(category);
            return ServiceResult<CategoryResponseDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCategoryByIdAsync");
            return ServiceResult<CategoryResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            // Check if category name already exists
            var existingCategory = await _unitOfWork.Categories.GetByNameAsync(dto.Name);
            if (existingCategory != null)
            {
                return ServiceResult<CategoryResponseDto>.Failure("Tên danh mục đã tồn tại");
            }

            var category = new Category
            {
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = MapToResponseDto(category);
            return ServiceResult<CategoryResponseDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateCategoryAsync");
            return ServiceResult<CategoryResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CategoryResponseDto>> UpdateCategoryAsync(string id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryResponseDto>.Failure("Không tìm thấy danh mục để cập nhật");
            }

            // Check if new name already exists (excluding current category)
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name)
            {
                var existingCategory = await _unitOfWork.Categories.GetByNameAsync(dto.Name);
                if (existingCategory != null && existingCategory.Id != id)
                {
                    return ServiceResult<CategoryResponseDto>.Failure("Tên danh mục đã tồn tại");
                }
            }

            // Update fields exactly like Node.js
            if (!string.IsNullOrEmpty(dto.Name))
                category.Name = dto.Name;
            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = MapToResponseDto(category);
            return ServiceResult<CategoryResponseDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateCategoryAsync");
            return ServiceResult<CategoryResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CategoryResponseDto>> DeleteCategoryAsync(string id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryResponseDto>.Failure("Không tìm thấy danh mục để xóa");
            }

            // Check if category has products
            var products = await _unitOfWork.Products.GetByCategoryIdAsync(id);
            if (products.Any())
            {
                return ServiceResult<CategoryResponseDto>.Failure("Không thể xóa danh mục có sản phẩm");
            }

            await _unitOfWork.Categories.DeleteAsync(category.Id);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = MapToResponseDto(category);
            return ServiceResult<CategoryResponseDto>.SuccessResult(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteCategoryAsync");
            return ServiceResult<CategoryResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    #endregion
}
