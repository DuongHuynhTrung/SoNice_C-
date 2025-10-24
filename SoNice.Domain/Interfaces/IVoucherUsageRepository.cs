using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// VoucherUsage repository interface with specific voucher usage operations
/// </summary>
public interface IVoucherUsageRepository : IRepository<VoucherUsage>
{
    Task<IEnumerable<VoucherUsage>> GetVoucherUsagesByVoucherIdAsync(string voucherId);
}
