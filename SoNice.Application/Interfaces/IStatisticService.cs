using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Statistic service interface - matches Node.js StatisticController logic
/// </summary>
public interface IStatisticService
{
    Task<ServiceResult<object>> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate);
    Task<ServiceResult<object>> GetProductStatisticsAsync(DateTime? startDate, DateTime? endDate);
    Task<ServiceResult<object>> GetUserStatisticsAsync(DateTime? startDate, DateTime? endDate);
    Task<ServiceResult<object>> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate);
    Task<ServiceResult<object>> GetDashboardStatisticsAsync();
    Task<ServiceResult<(byte[] Data, string FileName)>> ExportStatisticsToExcelAsync(string type, DateTime? startDate, DateTime? endDate);
}
