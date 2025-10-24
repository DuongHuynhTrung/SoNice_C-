using SoNice.Application.DTOs;
using SoNice.Application.Common;

namespace SoNice.Application.Interfaces;

/// <summary>
/// Voucher service interface - matches Node.js VoucherController logic
/// </summary>
public interface IVoucherService
{
    Task<ServiceResult<List<VoucherResponseDto>>> GetAllVouchersAsync();
    Task<ServiceResult<VoucherResponseDto>> GetVoucherByIdAsync(string id);
    Task<ServiceResult<VoucherResponseDto>> CreateVoucherAsync(CreateVoucherDto dto);
    Task<ServiceResult<VoucherResponseDto>> UpdateVoucherAsync(string id, UpdateVoucherDto dto);
    Task<ServiceResult<VoucherResponseDto>> DeleteVoucherAsync(string id);
}
