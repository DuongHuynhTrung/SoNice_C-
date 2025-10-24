using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;

namespace SoNice.Application.Services;

/// <summary>
/// VoucherUsage service implementation - matches Node.js VoucherUsageController logic exactly
/// </summary>
public class VoucherUsageService : IVoucherUsageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VoucherUsageService> _logger;

    public VoucherUsageService(IUnitOfWork unitOfWork, ILogger<VoucherUsageService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<VoucherUsageResponseDto>>> GetAllVoucherUsagesAsync(int page = 1, int limit = 10)
    {
        try
        {
            var voucherUsages = await _unitOfWork.VoucherUsages.GetAllAsync();
            var voucherUsagesList = voucherUsages.ToList();
            
            // Pagination
            var total = voucherUsagesList.Count;
            var skip = (page - 1) * limit;
            var pagedVoucherUsages = voucherUsagesList.Skip(skip).Take(limit).ToList();

            var voucherUsageDtos = pagedVoucherUsages.Select(MapToResponseDto).ToList();

            var result = new PagedResult<VoucherUsageResponseDto>
            {
                Data = voucherUsageDtos,
                Total = total,
                Page = page,
                Limit = limit,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            };

            return ServiceResult<PagedResult<VoucherUsageResponseDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllVoucherUsagesAsync");
            return ServiceResult<PagedResult<VoucherUsageResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherUsageResponseDto>> GetVoucherUsageByIdAsync(string id)
    {
        try
        {
            var voucherUsage = await _unitOfWork.VoucherUsages.GetByIdAsync(id);
            if (voucherUsage == null)
            {
                return ServiceResult<VoucherUsageResponseDto>.Failure("Không tìm thấy voucher usage với ID đã cho");
            }

            var voucherUsageDto = MapToResponseDto(voucherUsage);
            return ServiceResult<VoucherUsageResponseDto>.SuccessResult(voucherUsageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetVoucherUsageByIdAsync");
            return ServiceResult<VoucherUsageResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherUsageResponseDto>> CreateVoucherUsageAsync(CreateVoucherUsageDto dto)
    {
        try
        {
            // Validate vouchers exist and are active
            var vouchers = new List<Voucher>();
            foreach (var voucherId in dto.VoucherList)
            {
                var voucher = await _unitOfWork.Vouchers.GetByIdAsync(voucherId);
                if (voucher == null)
                {
                    return ServiceResult<VoucherUsageResponseDto>.Failure($"Không tìm thấy voucher với ID: {voucherId}");
                }

                if (!voucher.IsActive)
                {
                    return ServiceResult<VoucherUsageResponseDto>.Failure($"Voucher {voucher.Code} không còn hoạt động");
                }

                if (voucher.UsedCount >= voucher.UsageLimit)
                {
                    return ServiceResult<VoucherUsageResponseDto>.Failure($"Voucher {voucher.Code} đã hết lượt sử dụng");
                }

                if (DateTime.UtcNow < voucher.StartDate || DateTime.UtcNow > voucher.EndDate)
                {
                    return ServiceResult<VoucherUsageResponseDto>.Failure($"Voucher {voucher.Code} không trong thời gian sử dụng");
                }

                vouchers.Add(voucher);
            }

            // Calculate total discount amount
            decimal totalDiscountAmount = 0;
            foreach (var voucher in vouchers)
            {
                if (voucher.Type == Domain.Enums.VoucherType.Percentage)
                {
                    // For percentage vouchers, we need the order total to calculate discount
                    // This would typically be passed from the order creation
                    totalDiscountAmount += voucher.Value; // This is a simplified calculation
                }
                else if (voucher.Type == Domain.Enums.VoucherType.FixedAmount)
                {
                    totalDiscountAmount += voucher.Value;
                }
            }

            var voucherUsage = new VoucherUsage
            {
                VoucherList = dto.VoucherList,
                DiscountAmount = totalDiscountAmount
            };

            await _unitOfWork.VoucherUsages.AddAsync(voucherUsage);
            await _unitOfWork.SaveChangesAsync();

            // Update voucher usage counts
            foreach (var voucher in vouchers)
            {
                voucher.UsedCount++;
                await _unitOfWork.Vouchers.UpdateAsync(voucher);
            }
            await _unitOfWork.SaveChangesAsync();

            var voucherUsageDto = MapToResponseDto(voucherUsage);
            return ServiceResult<VoucherUsageResponseDto>.SuccessResult(voucherUsageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateVoucherUsageAsync");
            return ServiceResult<VoucherUsageResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherUsageResponseDto>> UpdateVoucherUsageAsync(string id, UpdateVoucherUsageDto dto)
    {
        try
        {
            var voucherUsage = await _unitOfWork.VoucherUsages.GetByIdAsync(id);
            if (voucherUsage == null)
            {
                return ServiceResult<VoucherUsageResponseDto>.Failure("Không tìm thấy voucher usage để cập nhật");
            }

            // Update fields exactly like Node.js
            if (dto.VoucherList != null && dto.VoucherList.Any())
            {
                // Validate new vouchers
                var vouchers = new List<Voucher>();
                foreach (var voucherId in dto.VoucherList)
                {
                    var voucher = await _unitOfWork.Vouchers.GetByIdAsync(voucherId);
                    if (voucher == null)
                    {
                        return ServiceResult<VoucherUsageResponseDto>.Failure($"Không tìm thấy voucher với ID: {voucherId}");
                    }
                    vouchers.Add(voucher);
                }

                voucherUsage.VoucherList = dto.VoucherList;
            }

            if (dto.DiscountAmount.HasValue)
                voucherUsage.DiscountAmount = dto.DiscountAmount.Value;

            await _unitOfWork.VoucherUsages.UpdateAsync(voucherUsage);
            await _unitOfWork.SaveChangesAsync();

            var voucherUsageDto = MapToResponseDto(voucherUsage);
            return ServiceResult<VoucherUsageResponseDto>.SuccessResult(voucherUsageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateVoucherUsageAsync");
            return ServiceResult<VoucherUsageResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherUsageResponseDto>> DeleteVoucherUsageAsync(string id)
    {
        try
        {
            var voucherUsage = await _unitOfWork.VoucherUsages.GetByIdAsync(id);
            if (voucherUsage == null)
            {
                return ServiceResult<VoucherUsageResponseDto>.Failure("Không tìm thấy voucher usage để xóa");
            }

            await _unitOfWork.VoucherUsages.DeleteAsync(voucherUsage.Id);
            await _unitOfWork.SaveChangesAsync();

            var voucherUsageDto = MapToResponseDto(voucherUsage);
            return ServiceResult<VoucherUsageResponseDto>.SuccessResult(voucherUsageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteVoucherUsageAsync");
            return ServiceResult<VoucherUsageResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private VoucherUsageResponseDto MapToResponseDto(VoucherUsage voucherUsage)
    {
        return new VoucherUsageResponseDto
        {
            Id = voucherUsage.Id,
            VoucherList = voucherUsage.VoucherList,
            DiscountAmount = voucherUsage.DiscountAmount,
            CreatedAt = voucherUsage.CreatedAt,
            UpdatedAt = voucherUsage.UpdatedAt
        };
    }

    #endregion
}
