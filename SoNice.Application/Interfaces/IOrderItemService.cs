using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// OrderItem service interface - matches Node.js OrderItemController logic
/// </summary>
public interface IOrderItemService
{
    Task<ServiceResult<PagedResult<OrderItemResponseDto>>> GetAllOrderItemsAsync(int page = 1, int limit = 10);
    Task<ServiceResult<OrderItemResponseDto>> GetOrderItemByIdAsync(string id);
    Task<ServiceResult<OrderItemResponseDto>> CreateOrderItemAsync(OrderItemDto dto);
    Task<ServiceResult<OrderItemResponseDto>> UpdateOrderItemAsync(string id, OrderItemDto dto);
    Task<ServiceResult<OrderItemResponseDto>> DeleteOrderItemAsync(string id);
}
