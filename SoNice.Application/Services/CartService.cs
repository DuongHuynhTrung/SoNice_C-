using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// Cart service implementation - matches Node.js CartController logic exactly
/// </summary>
public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartService> _logger;

    public CartService(IUnitOfWork unitOfWork, ILogger<CartService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<CartResponseDto>>> GetAllCartsAsync(int page = 1, int limit = 10)
    {
        try
        {
            var carts = await _unitOfWork.Carts.GetAllAsync();
            var cartsList = carts.ToList();
            
            // Pagination
            var total = cartsList.Count;
            var skip = (page - 1) * limit;
            var pagedCarts = cartsList.Skip(skip).Take(limit).ToList();

            var cartDtos = pagedCarts.Select(MapToResponseDto).ToList();

            var result = new PagedResult<CartResponseDto>
            {
                Data = cartDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<CartResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCartsAsync");
            return ServiceResult<PagedResult<CartResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CartResponseDto>> GetCartByIdAsync(string id)
    {
        try
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(id);
            if (cart == null)
            {
                return ServiceResult<CartResponseDto>.Failure("Không tìm thấy giỏ hàng với ID đã cho");
            }

            var cartDto = MapToResponseDto(cart);
            return ServiceResult<CartResponseDto>.SuccessResult(cartDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCartByIdAsync");
            return ServiceResult<CartResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CartResponseDto>> CreateCartAsync(CreateCartDto dto)
    {
        try
        {
            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return ServiceResult<CartResponseDto>.Failure("Không tìm thấy người dùng với ID đã cho");
            }

            // Check if user already has a cart
            var existingCart = await _unitOfWork.Carts.GetByUserIdAsync(dto.UserId);
            if (existingCart != null)
            {
                return ServiceResult<CartResponseDto>.Failure("Người dùng đã có giỏ hàng");
            }

            // Validate cart items
            var cartItems = new List<CartItem>();
            foreach (var itemDto in dto.CartItemsList)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return ServiceResult<CartResponseDto>.Failure($"Không tìm thấy sản phẩm với ID: {itemDto.ProductId}");
                }

                if (itemDto.Quantity <= 0)
                {
                    return ServiceResult<CartResponseDto>.Failure("Số lượng sản phẩm phải lớn hơn 0");
                }

                var cartItem = new CartItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };

                cartItems.Add(cartItem);
            }

            var cart = new Cart
            {
                UserId = dto.UserId,
                CartItemsList = cartItems
            };

            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            var cartDto = MapToResponseDto(cart);
            return ServiceResult<CartResponseDto>.SuccessResult(cartDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateCartAsync");
            return ServiceResult<CartResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CartResponseDto>> UpdateCartAsync(string id, UpdateCartDto dto)
    {
        try
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(id);
            if (cart == null)
            {
                return ServiceResult<CartResponseDto>.Failure("Không tìm thấy giỏ hàng để cập nhật");
            }

            // Validate cart items
            var cartItems = new List<CartItem>();
            foreach (var itemDto in dto.CartItemsList)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return ServiceResult<CartResponseDto>.Failure($"Không tìm thấy sản phẩm với ID: {itemDto.ProductId}");
                }

                if (itemDto.Quantity <= 0)
                {
                    return ServiceResult<CartResponseDto>.Failure("Số lượng sản phẩm phải lớn hơn 0");
                }

                var cartItem = new CartItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };

                cartItems.Add(cartItem);
            }

            cart.CartItemsList = cartItems;

            await _unitOfWork.Carts.UpdateAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            var cartDto = MapToResponseDto(cart);
            return ServiceResult<CartResponseDto>.SuccessResult(cartDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateCartAsync");
            return ServiceResult<CartResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CartResponseDto>> DeleteCartAsync(string id)
    {
        try
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(id);
            if (cart == null)
            {
                return ServiceResult<CartResponseDto>.Failure("Không tìm thấy giỏ hàng để xóa");
            }

            await _unitOfWork.Carts.DeleteAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();

            var cartDto = MapToResponseDto(cart);
            return ServiceResult<CartResponseDto>.SuccessResult(cartDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteCartAsync");
            return ServiceResult<CartResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<CartResponseDto>> GetCurrentUserCartAsync(string userId)
    {
        try
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (cart == null)
            {
                return ServiceResult<CartResponseDto>.Failure("Không tìm thấy giỏ hàng của người dùng");
            }

            var cartDto = MapToResponseDto(cart);
            return ServiceResult<CartResponseDto>.SuccessResult(cartDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentUserCartAsync");
            return ServiceResult<CartResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private CartResponseDto MapToResponseDto(Cart cart)
    {
        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItemsList = cart.CartItemsList?.Select(item => new CartItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList() ?? new List<CartItemDto>(),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    #endregion
}
