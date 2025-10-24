using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;
using System.Security.Claims;

namespace SoNice.Api.Controllers;

/// <summary>
/// Voucher controller - matches Node.js VoucherController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly ILogger<VoucherController> _logger;

    public VoucherController(IVoucherService voucherService, ILogger<VoucherController> logger)
    {
        _voucherService = voucherService;
        _logger = logger;
    }

    /// <summary>
    /// Get all vouchers with pagination - matches Node.js getAllVouchers exactly
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVouchers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _voucherService.GetAllVouchersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllVouchers");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Get voucher by ID - matches Node.js getVoucherById exactly
    /// </summary>
    [HttpGet("{voucherId}")]
    public async Task<IActionResult> GetVoucherById(string voucherId)
    {
        try
        {
            var result = await _voucherService.GetVoucherByIdAsync(voucherId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy voucher với ID đã cho" });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetVoucherById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Create new voucher (Admin only) - matches Node.js createVoucher exactly
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            if (string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.Name) || dto.Value == 0)
            {
                return BadRequest(new { message = "Vui lòng cung cấp code, name, type và value" });
            }

            var result = await _voucherService.CreateVoucherAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateVoucher");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Update voucher by ID (Admin only) - matches Node.js updateVoucherById exactly
    /// </summary>
    [HttpPut("{voucherId}")]
    [Authorize]
    public async Task<IActionResult> UpdateVoucherById(string voucherId, [FromBody] UpdateVoucherDto dto)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _voucherService.UpdateVoucherAsync(voucherId, dto);
            if (!result.Success)
            {
                if (result.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = "Không tìm thấy voucher để cập nhật" });
                return BadRequest(new { message = result.Message });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateVoucherById");
            return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
        }
    }

    /// <summary>
    /// Delete voucher by ID (Admin only) - matches Node.js deleteVoucherById exactly
    /// </summary>
    [HttpDelete("{voucherId}")]
    [Authorize]
    public async Task<IActionResult> DeleteVoucherById(string voucherId)
    {
        try
        {
            var userRole = GetUserRole();
            if (userRole != UserRole.Admin)
            {
                return Forbid("Chi có Admin có quyền thực hiện chức năng này");
            }

            var result = await _voucherService.DeleteVoucherAsync(voucherId);
            if (!result.Success)
            {
                return NotFound(new { message = "Không tìm thấy voucher để xóa" });
            }
            return Ok(new { message = "Đã xóa voucher thành công", voucher = result.Data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteVoucherById");
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
