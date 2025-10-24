using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Voucher repository interface with specific voucher operations
/// </summary>
public interface IVoucherRepository : IRepository<Voucher>
{
    Task<Voucher?> GetByCodeAsync(string code);
    Task<IEnumerable<Voucher>> GetActiveVouchersAsync(int page, int limit);
    Task<int> GetActiveVouchersCountAsync();
}
