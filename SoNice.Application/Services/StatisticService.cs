using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// Statistic service implementation - matches Node.js StatisticController logic exactly
/// </summary>
public class StatisticService : IStatisticService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatisticService> _logger;

    public StatisticService(IUnitOfWork unitOfWork, ILogger<StatisticService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<object>> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            
            // Apply date filter if provided
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();
            }

            var statistics = new
            {
                TotalOrders = orders.Count(),
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Pending),
                ConfirmedOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Confirmed),
                ProcessingOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Processing),
                ShippingOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Shipping),
                DeliveredOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled),
                PaymentFailedOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.PaymentFailed)
            };

            return ServiceResult<object>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderStatisticsAsync");
            return ServiceResult<object>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<object>> GetProductStatisticsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var orders = await _unitOfWork.Orders.GetAllAsync();
            
            // Apply date filter if provided
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();
            }

            var statistics = new
            {
                TotalProducts = products.Count(),
                ActiveProducts = products.Count(p => p.IsActive),
                InactiveProducts = products.Count(p => !p.IsActive),
                TotalStock = products.Sum(p => p.StockQuantity),
                LowStockProducts = products.Count(p => p.StockQuantity < 10),
                OutOfStockProducts = products.Count(p => p.StockQuantity == 0)
            };

            return ServiceResult<object>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductStatisticsAsync");
            return ServiceResult<object>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<object>> GetUserStatisticsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            
            // Apply date filter if provided
            if (startDate.HasValue)
            {
                users = users.Where(u => u.CreatedAt >= startDate.Value).ToList();
            }
            if (endDate.HasValue)
            {
                users = users.Where(u => u.CreatedAt <= endDate.Value).ToList();
            }

            var statistics = new
            {
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsVerified),
                BlockedUsers = 0, // User entity doesn't have Status property
                VerifiedUsers = users.Count(u => u.IsVerified),
                UnverifiedUsers = users.Count(u => !u.IsVerified),
                AdminUsers = users.Count(u => u.RoleName == Domain.Enums.UserRole.Admin),
                CustomerUsers = users.Count(u => u.RoleName == Domain.Enums.UserRole.Customer)
            };

            return ServiceResult<object>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserStatisticsAsync");
            return ServiceResult<object>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<object>> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            
            // Apply date filter if provided
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();
            }

            var deliveredOrders = orders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered).ToList();

            var statistics = new
            {
                TotalRevenue = deliveredOrders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count(),
                DeliveredOrders = deliveredOrders.Count(),
                AverageOrderValue = deliveredOrders.Any() ? deliveredOrders.Average(o => o.TotalAmount) : 0,
                BankPaymentRevenue = deliveredOrders.Where(o => o.PaymentMethod == Domain.Enums.OrderPaymentMethod.Bank).Sum(o => o.TotalAmount),
                CodPaymentRevenue = deliveredOrders.Where(o => o.PaymentMethod == Domain.Enums.OrderPaymentMethod.Cod).Sum(o => o.TotalAmount)
            };

            return ServiceResult<object>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRevenueStatisticsAsync");
            return ServiceResult<object>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<object>> GetDashboardStatisticsAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();
            var orders = await _unitOfWork.Orders.GetAllAsync();
            var categories = await _unitOfWork.Categories.GetAllAsync();

            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var statistics = new
            {
                TotalUsers = users.Count(),
                TotalProducts = products.Count(),
                TotalOrders = orders.Count(),
                TotalCategories = categories.Count(),
                TodayOrders = orders.Count(o => o.CreatedAt.Date == today),
                ThisMonthOrders = orders.Count(o => o.CreatedAt >= thisMonth),
                LastMonthOrders = orders.Count(o => o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth),
                TodayRevenue = orders.Where(o => o.CreatedAt.Date == today && o.Status == Domain.Enums.OrderStatus.Delivered).Sum(o => o.TotalAmount),
                ThisMonthRevenue = orders.Where(o => o.CreatedAt >= thisMonth && o.Status == Domain.Enums.OrderStatus.Delivered).Sum(o => o.TotalAmount),
                LastMonthRevenue = orders.Where(o => o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth && o.Status == Domain.Enums.OrderStatus.Delivered).Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Pending),
                LowStockProducts = products.Count(p => p.StockQuantity < 10),
                ActiveUsers = users.Count(u => u.IsVerified)
            };

            return ServiceResult<object>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDashboardStatisticsAsync");
            return ServiceResult<object>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<(byte[] Data, string FileName)>> ExportStatisticsToExcelAsync(string type, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            // This would integrate with Excel export library (like EPPlus or ClosedXML)
            // For now, return a placeholder
            var fileName = $"statistics_{type}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            var data = new byte[0]; // Placeholder for Excel file data

            return ServiceResult<(byte[], string)>.SuccessResult((data, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExportStatisticsToExcelAsync");
            return ServiceResult<(byte[], string)>.Failure("Lỗi máy chủ nội bộ");
        }
    }
}
