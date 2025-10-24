using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SoNice.Api.Middleware;
using SoNice.Application.Validators;
using SoNice.Infrastructure;
using SoNice.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add logging to track startup process
var startupLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
startupLogger.LogInformation("=== SoNice API Startup Beginning ===");
startupLogger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);

// Add services to the container.
startupLogger.LogInformation("Adding controllers...");
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
startupLogger.LogInformation("Adding Swagger...");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "SoNice API", 
        Version = "v1",
        Description = "API documentation for SoNice e-commerce platform"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();

// Add Infrastructure services
startupLogger.LogInformation("Adding Infrastructure services...");
builder.Services.AddInfrastructure();

// Add Application services
startupLogger.LogInformation("Adding Application services...");
builder.Services.AddScoped<SoNice.Application.Interfaces.IProductService, SoNice.Application.Services.ProductService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IOrderService, SoNice.Application.Services.OrderService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.ICartService, SoNice.Application.Services.CartService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.ICategoryService, SoNice.Application.Services.CategoryService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IBlogService, SoNice.Application.Services.BlogService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.INotificationService, SoNice.Application.Services.NotificationService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IVoucherService, SoNice.Application.Services.VoucherService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IVoucherUsageService, SoNice.Application.Services.VoucherUsageService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IOrderItemService, SoNice.Application.Services.OrderItemService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IStatisticService, SoNice.Application.Services.StatisticService>();
builder.Services.AddScoped<SoNice.Application.Interfaces.IPayOsService, SoNice.Application.Services.PayOsService>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

startupLogger.LogInformation("Building application...");
var app = builder.Build();
startupLogger.LogInformation("Application built successfully");

// Configure the HTTP request pipeline.
startupLogger.LogInformation("Configuring HTTP request pipeline...");
// Always enable Swagger UI for easy API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SoNice API v1");
    c.RoutePrefix = "api-docs";
});

// Add middleware
app.UseErrorHandling();
app.UseMongoDbHealthCheck();
app.UseRateLimiting();
app.UseJwtMiddleware();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");
app.MapGet("/health-simple", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheckSimple")
    .WithOpenApi();

// Server info endpoint
app.MapGet("/server-info", (HttpContext context) => 
{
    var urls = app.Urls;
    var serverInfo = new
    {
        ApplicationName = "SoNice API",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Urls = urls.ToList(),
        MachineName = Environment.MachineName,
        ProcessId = Environment.ProcessId,
        WorkingDirectory = Environment.CurrentDirectory,
        FrameworkVersion = Environment.Version.ToString(),
        SwaggerUrl = "/api-docs",
        HealthCheckUrl = "/health"
    };
    
    return Results.Ok(serverInfo);
})
.WithName("ServerInfo")
.WithOpenApi();

// Log application startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var urls = app.Urls;

logger.LogInformation("=== SoNice API Starting ===");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Application URLs:");
foreach (var url in urls)
{
    logger.LogInformation("  - {Url}", url);
}

logger.LogInformation("Swagger UI available at: {SwaggerUrl}", "/api-docs");
logger.LogInformation("Health check available at: {HealthUrl}", "/health");
logger.LogInformation("Server info available at: {ServerInfoUrl}", "/server-info");

logger.LogInformation("=== SoNice API Started Successfully ===");

app.Run();
