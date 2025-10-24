using Microsoft.Extensions.Logging;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Application.Common;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Domain.Enums;

namespace SoNice.Application.Services;

/// <summary>
/// Voucher service implementation - matches Node.js VoucherController logic exactly
/// </summary>
public class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VoucherService> _logger;

    public VoucherService(IUnitOfWork unitOfWork, ILogger<VoucherService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ServiceResult<List<VoucherResponseDto>>> GetAllVouchersAsync()
    {
        try
        {
            var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
            var voucherDtos = vouchers.Select(MapToResponseDto).ToList();

            return ServiceResult<List<VoucherResponseDto>>.SuccessResult(voucherDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllVouchersAsync");
            return ServiceResult<List<VoucherResponseDto>>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherResponseDto>> GetVoucherByIdAsync(string id)
    {
        try
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Không tìm thấy voucher với ID đã cho");
            }

            var voucherDto = MapToResponseDto(voucher);
            return ServiceResult<VoucherResponseDto>.SuccessResult(voucherDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetVoucherByIdAsync");
            return ServiceResult<VoucherResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherResponseDto>> CreateVoucherAsync(CreateVoucherDto dto)
    {
        try
        {
            // Check if voucher code already exists
            var existingVoucher = await _unitOfWork.Vouchers.GetByCodeAsync(dto.Code);
            if (existingVoucher != null)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Mã voucher đã tồn tại");
            }

            // Voucher type is already validated by the DTO

            // Validate dates
            if (dto.StartDate >= dto.EndDate)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
            }

            // Validate value based on type
            if (dto.Type == VoucherType.Percentage && (dto.Value <= 0 || dto.Value > 100))
            {
                return ServiceResult<VoucherResponseDto>.Failure("Giá trị voucher phần trăm phải từ 1 đến 100");
            }

            if (dto.Type == VoucherType.FixedAmount && dto.Value <= 0)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Giá trị voucher cố định phải lớn hơn 0");
            }

            var voucher = new Voucher
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Value = dto.Value,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                CanStack = dto.CanStack,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Vouchers.AddAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            var voucherDto = MapToResponseDto(voucher);
            return ServiceResult<VoucherResponseDto>.SuccessResult(voucherDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateVoucherAsync");
            return ServiceResult<VoucherResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherResponseDto>> UpdateVoucherAsync(string id, UpdateVoucherDto dto)
    {
        try
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Không tìm thấy voucher để cập nhật");
            }

            // Check if new code already exists (excluding current voucher)
            if (!string.IsNullOrEmpty(dto.Code) && dto.Code != voucher.Code)
            {
                var existingVoucher = await _unitOfWork.Vouchers.GetByCodeAsync(dto.Code);
                if (existingVoucher != null && existingVoucher.Id != id)
                {
                    return ServiceResult<VoucherResponseDto>.Failure("Mã voucher đã tồn tại");
                }
            }

            // Validate voucher type if provided
            if (dto.Type.HasValue)
            {
                voucher.Type = dto.Type.Value;
            }

            // Validate dates if provided
            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
            {
                if (dto.StartDate.Value >= dto.EndDate.Value)
                {
                    return ServiceResult<VoucherResponseDto>.Failure("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
                }
                voucher.StartDate = dto.StartDate.Value;
                voucher.EndDate = dto.EndDate.Value;
            }

            // Validate value based on type
            if (dto.Value.HasValue)
            {
                if (voucher.Type == VoucherType.Percentage && (dto.Value.Value <= 0 || dto.Value.Value > 100))
                {
                    return ServiceResult<VoucherResponseDto>.Failure("Giá trị voucher phần trăm phải từ 1 đến 100");
                }

                if (voucher.Type == VoucherType.FixedAmount && dto.Value.Value <= 0)
                {
                    return ServiceResult<VoucherResponseDto>.Failure("Giá trị voucher cố định phải lớn hơn 0");
                }

                voucher.Value = dto.Value.Value;
            }

            // Update fields exactly like Node.js
            if (!string.IsNullOrEmpty(dto.Code))
                voucher.Code = dto.Code;
            if (!string.IsNullOrEmpty(dto.Name))
                voucher.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description))
                voucher.Description = dto.Description;
            if (dto.UsageLimit.HasValue)
                voucher.UsageLimit = dto.UsageLimit.Value;
            if (dto.CanStack.HasValue)
                voucher.CanStack = dto.CanStack.Value;
            if (dto.IsActive.HasValue)
                voucher.IsActive = dto.IsActive.Value;

            await _unitOfWork.Vouchers.UpdateAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            var voucherDto = MapToResponseDto(voucher);
            return ServiceResult<VoucherResponseDto>.SuccessResult(voucherDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateVoucherAsync");
            return ServiceResult<VoucherResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    public async Task<ServiceResult<VoucherResponseDto>> DeleteVoucherAsync(string id)
    {
        try
        {
            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
            if (voucher == null)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Không tìm thấy voucher để xóa");
            }

            // Check if voucher has been used
            if (voucher.UsedCount > 0)
            {
                return ServiceResult<VoucherResponseDto>.Failure("Không thể xóa voucher đã được sử dụng");
            }

            await _unitOfWork.Vouchers.DeleteAsync(voucher.Id);
            await _unitOfWork.SaveChangesAsync();

            var voucherDto = MapToResponseDto(voucher);
            return ServiceResult<VoucherResponseDto>.SuccessResult(voucherDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteVoucherAsync");
            return ServiceResult<VoucherResponseDto>.Failure("Lỗi máy chủ nội bộ");
        }
    }

    #region Helper Methods

    private VoucherResponseDto MapToResponseDto(Voucher voucher)
    {
        return new VoucherResponseDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            Type = voucher.Type,
            Value = voucher.Value,
            UsageLimit = voucher.UsageLimit,
            UsedCount = voucher.UsedCount,
            CanStack = voucher.CanStack,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            IsActive = voucher.IsActive,
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt
        };
    }

    #endregion
}
