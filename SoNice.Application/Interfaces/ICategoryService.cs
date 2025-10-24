using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Category service interface - matches Node.js CategoryController logic
/// </summary>
public interface ICategoryService
{
    Task<ServiceResult<List<CategoryResponseDto>>> GetAllCategoriesAsync();
    Task<ServiceResult<CategoryResponseDto>> GetCategoryByIdAsync(string id);
    Task<ServiceResult<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ServiceResult<CategoryResponseDto>> UpdateCategoryAsync(string id, UpdateCategoryDto dto);
    Task<ServiceResult<CategoryResponseDto>> DeleteCategoryAsync(string id);
}
