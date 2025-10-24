using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// OrderItem service implementation - matches Node.js OrderItemController logic exactly
/// </summary>
public class OrderItemService : IOrderItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderItemService> _logger;

    public OrderItemService(IUnitOfWork unitOfWork, ILogger<OrderItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<OrderItemResponseDto>>> GetAllOrderItemsAsync(int page = 1, int limit = 10)
    {
        try
        {
            var orderItems = await _unitOfWork.OrderItems.GetAllAsync();
            var orderItemsList = orderItems.ToList();
            
            // Pagination
            var total = orderItemsList.Count;
            var skip = (page - 1) * limit;
            var pagedOrderItems = orderItemsList.Skip(skip).Take(limit).ToList();

            var orderItemDtos = pagedOrderItems.Select(MapToResponseDto).ToList();

            var result = new PagedResult<OrderItemResponseDto>
            {
                Data = orderItemDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<OrderItemResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllOrderItemsAsync");
            return ServiceResult<PagedResult<OrderItemResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderItemResponseDto>> GetOrderItemByIdAsync(string id)
    {
        try
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (orderItem == null)
            {
                return ServiceResult<OrderItemResponseDto>.Failure("Không tìm thấy order item với ID đã cho");
            }

            var orderItemDto = MapToResponseDto(orderItem);
            return ServiceResult<OrderItemResponseDto>.SuccessResult(orderItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderItemByIdAsync");
            return ServiceResult<OrderItemResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderItemResponseDto>> CreateOrderItemAsync(OrderItemDto dto)
    {
        try
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return ServiceResult<OrderItemResponseDto>.Failure("Không tìm thấy sản phẩm với ID đã cho");
            }

            if (dto.Quantity <= 0)
            {
                return ServiceResult<OrderItemResponseDto>.Failure("Số lượng sản phẩm phải lớn hơn 0");
            }

            var totalPrice = product.Amount * dto.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                TotalPrice = totalPrice
            };

            await _unitOfWork.OrderItems.AddAsync(orderItem);
            await _unitOfWork.SaveChangesAsync();

            var orderItemDto = MapToResponseDto(orderItem);
            return ServiceResult<OrderItemResponseDto>.SuccessResult(orderItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateOrderItemAsync");
            return ServiceResult<OrderItemResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderItemResponseDto>> UpdateOrderItemAsync(string id, OrderItemDto dto)
    {
        try
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (orderItem == null)
            {
                return ServiceResult<OrderItemResponseDto>.Failure("Không tìm thấy order item để cập nhật");
            }

            // Validate product exists if provided
            if (!string.IsNullOrEmpty(dto.ProductId))
            {
                var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                if (product == null)
                {
                    return ServiceResult<OrderItemResponseDto>.Failure("Không tìm thấy sản phẩm với ID đã cho");
                }
                orderItem.ProductId = dto.ProductId;
            }

            if (dto.Quantity > 0)
            {
                orderItem.Quantity = dto.Quantity;
                
                // Recalculate total price
                var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    orderItem.TotalPrice = product.Amount * dto.Quantity;
                }
            }

            await _unitOfWork.OrderItems.UpdateAsync(orderItem);
            await _unitOfWork.SaveChangesAsync();

            var orderItemDto = MapToResponseDto(orderItem);
            return ServiceResult<OrderItemResponseDto>.SuccessResult(orderItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateOrderItemAsync");
            return ServiceResult<OrderItemResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderItemResponseDto>> DeleteOrderItemAsync(string id)
    {
        try
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (orderItem == null)
            {
                return ServiceResult<OrderItemResponseDto>.Failure("Không tìm thấy order item để xóa");
            }

            await _unitOfWork.OrderItems.DeleteAsync(orderItem.Id);
            await _unitOfWork.SaveChangesAsync();

            var orderItemDto = MapToResponseDto(orderItem);
            return ServiceResult<OrderItemResponseDto>.SuccessResult(orderItemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteOrderItemAsync");
            return ServiceResult<OrderItemResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private OrderItemResponseDto MapToResponseDto(OrderItem orderItem)
    {
        return new OrderItemResponseDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            TotalPrice = orderItem.TotalPrice,
            CreatedAt = orderItem.CreatedAt,
            UpdatedAt = orderItem.UpdatedAt
        };
    }

    #endregion
}
