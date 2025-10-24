using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SoNice.Domain.Entities;

namespace SoNice.Infrastructure.Data;

/// <summary>
/// MongoDB database context
/// </summary>
public class MongoDbContext
{
    public readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbContext> _logger;

    public MongoDbContext(IConfiguration configuration, ILogger<MongoDbContext> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetConnectionString("MongoDB") 
            ?? throw new ArgumentNullException("MongoDB connection string is required");
        
        // Configure MongoDB client with timeout settings
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
        clientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        clientSettings.SocketTimeout = TimeSpan.FromSeconds(10);
        
        var client = new MongoClient(clientSettings);
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "SoNice";
        _database = client.GetDatabase(databaseName);
        
        _logger.LogInformation("MongoDB context initialized with database: {DatabaseName}", databaseName);
        
        // Note: Connection test is now handled by health check to avoid blocking startup
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Cart> Carts => _database.GetCollection<Cart>("cart");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<OrderItem> OrderItems => _database.GetCollection<OrderItem>("orderitems");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
    public IMongoCollection<Voucher> Vouchers => _database.GetCollection<Voucher>("vouchers");
    public IMongoCollection<VoucherUsage> VoucherUsages => _database.GetCollection<VoucherUsage>("voucherusages");
    public IMongoCollection<Blog> Blogs => _database.GetCollection<Blog>("blogs");
}
