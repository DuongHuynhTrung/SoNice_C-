using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// VoucherUsage controller - matches Node.js VoucherUsageController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VoucherUsageController : ControllerBase
{
    private readonly IVoucherUsageService _voucherUsageService;
    private readonly ILogger<VoucherUsageController> _logger;

    public VoucherUsageController(IVoucherUsageService voucherUsageService, ILogger<VoucherUsageController> logger)
    {
        _voucherUsageService = voucherUsageService;
        _logger = logger;
    }

    /// <summary>
    /// Get all voucher usages with pagination - matches Node.js getAllVoucherUsages exactly
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllVoucherUsages([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _voucherUsageService.GetAllVoucherUsagesAsync(page, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllVoucherUsages");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get voucher usage by ID - matches Node.js getVoucherUsageById exactly
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetVoucherUsageById(string id)
    {
        try
        {
            var result = await _voucherUsageService.GetVoucherUsageByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy voucher usage với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetVoucherUsageById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new voucher usage - matches Node.js createVoucherUsage exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateVoucherUsage([FromBody] CreateVoucherUsageDto dto)
    {
        try
        {
            var result = await _voucherUsageService.CreateVoucherUsageAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateVoucherUsage");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update voucher usage by ID - matches Node.js updateVoucherUsageById exactly
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateVoucherUsageById(string id, [FromBody] UpdateVoucherUsageDto dto)
    {
        try
        {
            var result = await _voucherUsageService.UpdateVoucherUsageAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy voucher usage để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateVoucherUsageById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete voucher usage by ID (Admin only) - matches Node.js deleteVoucherUsageById exactly
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteVoucherUsageById(string id)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chỉ có Admin có quyền thực hiện chức năng này");
            }

            var result = await _voucherUsageService.DeleteVoucherUsageAsync(id);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy voucher usage để xóa" });
            }
            return Ok(new { message = "Đã xóa voucher usage thành công", voucherUsage = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteVoucherUsageById");
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
