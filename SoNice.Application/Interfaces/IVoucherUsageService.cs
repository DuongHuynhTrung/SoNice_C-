using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// VoucherUsage service interface - matches Node.js VoucherUsageController logic
/// </summary>
public interface IVoucherUsageService
{
    Task<ServiceResult<PagedResult<VoucherUsageResponseDto>>> GetAllVoucherUsagesAsync(int page = 1, int limit = 10);
    Task<ServiceResult<VoucherUsageResponseDto>> GetVoucherUsageByIdAsync(string id);
    Task<ServiceResult<VoucherUsageResponseDto>> CreateVoucherUsageAsync(CreateVoucherUsageDto dto);
    Task<ServiceResult<VoucherUsageResponseDto>> UpdateVoucherUsageAsync(string id, UpdateVoucherUsageDto dto);
    Task<ServiceResult<VoucherUsageResponseDto>> DeleteVoucherUsageAsync(string id);
}
