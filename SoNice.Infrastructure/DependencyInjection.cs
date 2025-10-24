using Microsoft.Extensions.DependencyInjection;
using SoNice.Application.Interfaces;
using SoNice.Application.Services;
using SoNice.Domain.Entities;
using SoNice.Domain.Interfaces;
using SoNice.Infrastructure.Data;
using SoNice.Infrastructure.Repositories;

namespace SoNice.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Database context
        services.AddScoped<MongoDbContext>();
        
        // Register IMongoDatabase for health checks and middleware
        services.AddScoped<MongoDB.Driver.IMongoDatabase>(provider => 
        {
            var context = provider.GetRequiredService<MongoDbContext>();
            return context._database;
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IVoucherUsageRepository, VoucherUsageRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
