using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Statistic controller - matches Node.js StatisticController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService _statisticService;
    private readonly ILogger<StatisticController> _logger;

    public StatisticController(IStatisticService statisticService, ILogger<StatisticController> logger)
    {
        _statisticService = statisticService;
        _logger = logger;
    }

    /// <summary>
    /// Get order statistics - matches Node.js getOrderStatistics exactly
    /// </summary>
    [HttpGet("orders")]
    [Authorize]
    public async Task<IActionResult> GetOrderStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _statisticService.GetOrderStatisticsAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrderStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get product statistics - matches Node.js getProductStatistics exactly
    /// </summary>
    [HttpGet("products")]
    [Authorize]
    public async Task<IActionResult> GetProductStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _statisticService.GetProductStatisticsAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get user statistics - matches Node.js getUserStatistics exactly
    /// </summary>
    [HttpGet("users")]
    [Authorize]
    public async Task<IActionResult> GetUserStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _statisticService.GetUserStatisticsAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get revenue statistics - matches Node.js getRevenueStatistics exactly
    /// </summary>
    [HttpGet("revenue")]
    [Authorize]
    public async Task<IActionResult> GetRevenueStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _statisticService.GetRevenueStatisticsAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRevenueStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get dashboard statistics - matches Node.js getDashboardStatistics exactly
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        try
        {
            var result = await _statisticService.GetDashboardStatisticsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDashboardStatistics");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Export statistics to Excel - matches Node.js exportStatisticsToExcel exactly
    /// </summary>
    [HttpGet("export")]
    [Authorize]
    public async Task<IActionResult> ExportStatisticsToExcel([FromQuery] string type, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var result = await _statisticService.ExportStatisticsToExcelAsync(type, startDate, endDate);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return File(result.Data.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Data.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExportStatisticsToExcel");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    #region Helper Methods

    private UserRole GetUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Customer;
    }

    #endregion
}
