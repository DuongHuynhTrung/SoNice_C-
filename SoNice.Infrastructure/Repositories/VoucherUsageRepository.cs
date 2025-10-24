using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// VoucherUsage repository implementation with specific voucher usage operations
/// </summary>
public class VoucherUsageRepository : Repository<VoucherUsage>, IVoucherUsageRepository
{
    public VoucherUsageRepository(MongoDbContext context, ILogger<VoucherUsageRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<VoucherUsage>> GetVoucherUsagesByVoucherIdAsync(string voucherId)
    {
        try
        {
            var filter = Builders<VoucherUsage>.Filter.AnyIn(x => x.VoucherList, new[] { voucherId });
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher usages by voucher ID: {VoucherId}", voucherId);
            throw;
        }
    }
}
