using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Cart service interface - matches Node.js CartController logic
/// </summary>
public interface ICartService
{
    Task<ServiceResult<PagedResult<CartResponseDto>>> GetAllCartsAsync(int page = 1, int limit = 10);
    Task<ServiceResult<CartResponseDto>> GetCartByIdAsync(string id);
    Task<ServiceResult<CartResponseDto>> CreateCartAsync(CreateCartDto dto);
    Task<ServiceResult<CartResponseDto>> UpdateCartAsync(string id, UpdateCartDto dto);
    Task<ServiceResult<CartResponseDto>> DeleteCartAsync(string id);
    Task<ServiceResult<CartResponseDto>> GetCurrentUserCartAsync(string userId);
}
