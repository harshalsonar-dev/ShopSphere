using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Auth.Services;
using ShopSphere.Domain.Interfaces;
using ShopSphere.Infrastructure.Data;
using ShopSphere.Infrastructure.Repositories;
using ShopSphere.Infrastructure.Services;
using StackExchange.Redis;

namespace ShopSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=ShopSphereDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        // 1. EF Core Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // 2. Repositories and Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. Redis Distributed Caching (with graceful memory cache fallback)
        var redisConn = configuration.GetConnectionString("Redis");
        var redisConnected = false;

        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            try
            {
                var redisOptions = ConfigurationOptions.Parse(redisConn);
                redisOptions.ConnectTimeout = 1000;
                redisOptions.SyncTimeout = 1000;
                redisOptions.AbortOnConnectFail = false;

                var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
                if (multiplexer.IsConnected)
                {
                    services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConn;
                        options.InstanceName = "ShopSphere_";
                    });
                    redisConnected = true;
                }
            }
            catch
            {
                redisConnected = false;
            }
        }

        if (!redisConnected)
        {
            // Fallback to in-memory distributed cache when Redis instance is unreachable
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, CacheService>();

        // 4. Security, Identity & Authentication Services
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
