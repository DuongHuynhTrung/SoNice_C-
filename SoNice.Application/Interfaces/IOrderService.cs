using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Order service interface - matches Node.js OrderController logic
/// </summary>
public interface IOrderService
{
    Task<ServiceResult<PagedResult<OrderResponseDto>>> GetAllOrdersAsync(int page = 1, int limit = 10);
    Task<ServiceResult<OrderResponseDto>> GetOrderByIdAsync(string id);
    Task<ServiceResult<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto);
    Task<ServiceResult<OrderResponseDto>> UpdateOrderAsync(string id, UpdateOrderDto dto);
    Task<ServiceResult<OrderResponseDto>> DeleteOrderAsync(string id);
    Task<ServiceResult<string>> GeneratePaymentLinkAsync(string orderId);
}
