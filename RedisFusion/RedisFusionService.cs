
namespace RedisFusion
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using RedisFusion.Services;
    using RedisFusion.Utilities;

    public static class RedisFusionService
    {

        public static IServiceCollection AddRedisFusionService(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure RedisConfigurations
            services.Configure<RedisFusionConfigurations>(configuration.GetSection(nameof(RedisFusionConfigurations)));

            // Build the service provider to access the configurations
            var serviceProvider = services.BuildServiceProvider();
            var cacheRedisConfigurations = serviceProvider.GetRequiredService<IOptions<RedisFusionConfigurations>>().Value;

            // Register StackExchange Redis Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = cacheRedisConfigurations.InstanceName;
                options.Configuration = cacheRedisConfigurations.ConnectionString;
            });

            // Register RedisService
            services.AddSingleton<IRedisFusionService, RedisFusion.Services.RedisFusionService>();

            return services;
        }

    }

}
