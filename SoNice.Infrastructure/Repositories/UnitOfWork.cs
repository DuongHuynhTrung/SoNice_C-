using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;

namespace SoNice.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IClientSessionHandle? _session;
    private bool _disposed = false;

    public UnitOfWork(MongoDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        
        // Initialize repositories
        Users = new UserRepository(context, loggerFactory.CreateLogger<UserRepository>());
        Products = new ProductRepository(context, loggerFactory.CreateLogger<ProductRepository>());
        Categories = new CategoryRepository(context, loggerFactory.CreateLogger<CategoryRepository>());
        Carts = new CartRepository(context, loggerFactory.CreateLogger<CartRepository>());
        Orders = new OrderRepository(context, loggerFactory.CreateLogger<OrderRepository>());
        OrderItems = new OrderItemRepository(context, loggerFactory.CreateLogger<OrderItemRepository>());
        Notifications = new NotificationRepository(context, loggerFactory.CreateLogger<NotificationRepository>());
        Vouchers = new VoucherRepository(context, loggerFactory.CreateLogger<VoucherRepository>());
        VoucherUsages = new VoucherUsageRepository(context, loggerFactory.CreateLogger<VoucherUsageRepository>());
        Blogs = new BlogRepository(context, loggerFactory.CreateLogger<BlogRepository>());
    }

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ICartRepository Carts { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public INotificationRepository Notifications { get; }
    public IVoucherRepository Vouchers { get; }
    public IVoucherUsageRepository VoucherUsages { get; }
    public IBlogRepository Blogs { get; }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            // In MongoDB, changes are typically saved immediately
            // This method is here for consistency with the interface
            // and to handle any potential transaction logic
            if (_session != null)
            {
                await _session.CommitTransactionAsync();
            }
            
            _logger.LogDebug("Changes saved successfully");
            return 1; // Return 1 to indicate success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            if (_session != null)
            {
                await _session.AbortTransactionAsync();
            }
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        try
        {
            if (_session == null)
            {
                _session = await _context._database.Client.StartSessionAsync();
                _session.StartTransaction();
                _logger.LogDebug("Transaction started");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting transaction");
            throw;
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_session != null)
            {
                await _session.CommitTransactionAsync();
                _session.Dispose();
                _session = null;
                _logger.LogDebug("Transaction committed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_session != null)
            {
                await _session.AbortTransactionAsync();
                _session.Dispose();
                _session = null;
                _logger.LogDebug("Transaction rolled back");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _session?.Dispose();
            _disposed = true;
        }
    }
}
