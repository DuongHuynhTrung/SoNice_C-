using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Domain.Enums;

namespace SoNice.Application.Services;

/// <summary>
/// Order service implementation - matches Node.js OrderController logic exactly
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    private readonly INotificationService _notificationService;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<ServiceResult<PagedResult<OrderResponseDto>>> GetAllOrdersAsync(int page = 1, int limit = 10)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var ordersList = orders.ToList();
            
            // Pagination
            var total = ordersList.Count;
            var skip = (page - 1) * limit;
            var pagedOrders = ordersList.Skip(skip).Take(limit).ToList();

            var orderDtos = new List<OrderResponseDto>();
            foreach (var order in pagedOrders)
            {
                orderDtos.Add(await MapToResponseDtoAsync(order));
            }

            var result = new PagedResult<OrderResponseDto>
            {
                Data = orderDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<OrderResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllOrdersAsync");
            return ServiceResult<PagedResult<OrderResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderResponseDto>> GetOrderByIdAsync(string id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return ServiceResult<OrderResponseDto>.Failure("Không tìm thấy đơn hàng với ID đã cho");
            }

            var orderDto = await MapToResponseDtoAsync(order);
            return ServiceResult<OrderResponseDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderByIdAsync");
            return ServiceResult<OrderResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto)
    {
        try
        {
            // Validate user exists if provided
            if (!string.IsNullOrEmpty(dto.UserId))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    return ServiceResult<OrderResponseDto>.Failure("Không tìm thấy người dùng với ID đã cho");
                }
            }

            // Process order items exactly like Node.js
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in dto.OrderItemList)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return ServiceResult<OrderResponseDto>.Failure($"Không tìm thấy sản phẩm với ID: {itemDto.ProductId}");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    return ServiceResult<OrderResponseDto>.Failure($"Số lượng sản phẩm {product.Name} không đủ");
                }

                var totalPrice = product.Amount * itemDto.Quantity;
                totalAmount += totalPrice;

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    TotalPrice = totalPrice
                };

                orderItems.Add(orderItem);

                // Update stock quantity
                product.StockQuantity -= itemDto.Quantity;
                await _unitOfWork.Products.UpdateAsync(product);
            }

            // Apply voucher discount if provided
            if (!string.IsNullOrEmpty(dto.VoucherUsageId))
            {
                var voucherUsage = await _unitOfWork.VoucherUsages.GetByIdAsync(dto.VoucherUsageId);
                if (voucherUsage != null)
                {
                    totalAmount -= voucherUsage.DiscountAmount;
                    if (totalAmount < 0) totalAmount = 0;
                }
            }

            // Generate order code exactly like Node.js
            var orderCode = GenerateOrderCode();

            // Save order items and collect their IDs
            var orderItemIds = new List<string>();
            foreach (var orderItem in orderItems)
            {
                var savedOrderItem = await _unitOfWork.OrderItems.AddAsync(orderItem);
                orderItemIds.Add(savedOrderItem.Id);
            }

            var order = new Order
            {
                UserId = dto.UserId,
                OrderItemList = orderItemIds,
                OrderCode = orderCode,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                PaymentMethod = dto.PaymentMethod,
                ShippingAddress = dto.ShippingAddress,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                Notes = dto.Notes,
                VoucherUsageId = dto.VoucherUsageId
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Create notification exactly like Node.js
            if (!string.IsNullOrEmpty(dto.UserId))
            {
                await _notificationService.CreateAndEmitNotificationAsync(
                    dto.UserId, 
                    "order_requested", 
                    $"Đơn hàng {orderCode} đã được tạo thành công"
                );
            }

            var orderDto = await MapToResponseDtoAsync(order);
            return ServiceResult<OrderResponseDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateOrderAsync");
            return ServiceResult<OrderResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderResponseDto>> UpdateOrderAsync(string id, UpdateOrderDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return ServiceResult<OrderResponseDto>.Failure("Không tìm thấy đơn hàng để cập nhật");
            }

            // Update fields exactly like Node.js
            if (dto.Status.HasValue)
            {
                var oldStatus = order.Status;
                order.Status = dto.Status.Value;

                // Handle status changes exactly like Node.js
                if (dto.Status.Value == OrderStatus.Cancelled && !string.IsNullOrEmpty(dto.CancellationReason))
                {
                    order.CancellationReason = dto.CancellationReason;
                    
                    // Restore stock quantity
                    foreach (var itemId in order.OrderItemList)
                    {
                        var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(itemId);
                        if (orderItem != null)
                        {
                            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity += orderItem.Quantity;
                                await _unitOfWork.Products.UpdateAsync(product);
                            }
                        }
                    }
                }

                // Create notification for status change
                if (!string.IsNullOrEmpty(order.UserId))
                {
                    var notificationType = GetNotificationTypeForStatus(dto.Status.Value);
                    var notificationContent = GetNotificationContentForStatus(dto.Status.Value, order.OrderCode);
                    
                    await _notificationService.CreateAndEmitNotificationAsync(
                        order.UserId, 
                        notificationType, 
                        notificationContent
                    );
                }
            }

            if (!string.IsNullOrEmpty(dto.Notes))
                order.Notes = dto.Notes;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var orderDto = await MapToResponseDtoAsync(order);
            return ServiceResult<OrderResponseDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateOrderAsync");
            return ServiceResult<OrderResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<OrderResponseDto>> DeleteOrderAsync(string id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return ServiceResult<OrderResponseDto>.Failure("Không tìm thấy đơn hàng để xóa");
            }

            await _unitOfWork.Orders.DeleteAsync(order.Id);
            await _unitOfWork.SaveChangesAsync();

            var orderDto = await MapToResponseDtoAsync(order);
            return ServiceResult<OrderResponseDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteOrderAsync");
            return ServiceResult<OrderResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<string>> GeneratePaymentLinkAsync(string orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return ServiceResult<string>.Failure("Không tìm thấy đơn hàng");
            }

            // This would integrate with PayOS API exactly like Node.js
            // For now, return a placeholder
            var paymentLink = $"https://payos.vn/payment/{order.OrderCode}";
            
            return ServiceResult<string>.SuccessResult(paymentLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GeneratePaymentLinkAsync");
            return ServiceResult<string>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private string GenerateOrderCode()
    {
        // Generate order code exactly like Node.js
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return $"ORD{timestamp}{random}";
    }

    private string GetNotificationTypeForStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => "order_confirmed",
            OrderStatus.Processing => "order_processing",
            OrderStatus.Shipping => "order_shipping",
            OrderStatus.Delivered => "order_delivered",
            _ => "order_requested"
        };
    }

    private string GetNotificationContentForStatus(OrderStatus status, string orderCode)
    {
        return status switch
        {
            OrderStatus.Confirmed => $"Đơn hàng {orderCode} đã được xác nhận",
            OrderStatus.Processing => $"Đơn hàng {orderCode} đang được xử lý",
            OrderStatus.Shipping => $"Đơn hàng {orderCode} đang được vận chuyển",
            OrderStatus.Delivered => $"Đơn hàng {orderCode} đã được giao thành công",
            _ => $"Đơn hàng {orderCode} đã được tạo"
        };
    }

    private async Task<OrderResponseDto> MapToResponseDtoAsync(Order order)
    {
        var orderItemDtos = new List<OrderItemResponseDto>();
        
        if (order.OrderItemList != null)
        {
            foreach (var itemId in order.OrderItemList)
            {
                var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(itemId);
                if (orderItem != null)
                {
                    orderItemDtos.Add(new OrderItemResponseDto
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        Quantity = orderItem.Quantity,
                        TotalPrice = orderItem.TotalPrice,
                        CreatedAt = orderItem.CreatedAt,
                        UpdatedAt = orderItem.UpdatedAt
                    });
                }
            }
        }

        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderItemList = orderItemDtos,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = order.ShippingAddress,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            Notes = order.Notes,
            CancellationReason = order.CancellationReason,
            VoucherUsageId = order.VoucherUsageId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    #endregion
}
