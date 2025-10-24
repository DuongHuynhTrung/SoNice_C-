using SoNice.Domain.Entities;

namespace SoNice.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions and repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    INotificationRepository Notifications { get; }
    IVoucherRepository Vouchers { get; }
    IVoucherUsageRepository VoucherUsages { get; }
    IBlogRepository Blogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
