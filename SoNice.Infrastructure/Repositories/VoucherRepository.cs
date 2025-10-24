using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Voucher repository implementation with specific voucher operations
/// </summary>
public class VoucherRepository : Repository<Voucher>, IVoucherRepository
{
    public VoucherRepository(MongoDbContext context, ILogger<VoucherRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Voucher?> GetByCodeAsync(string code)
    {
        try
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.Code, code);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher by code: {Code}", code);
            throw;
        }
    }

    public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync(int page, int limit)
    {
        try
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.IsActive, true);
            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vouchers");
            throw;
        }
    }

    public async Task<int> GetActiveVouchersCountAsync()
    {
        try
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.IsActive, true);
            return (int)await _collection.CountDocumentsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vouchers count");
            throw;
        }
    }
}
