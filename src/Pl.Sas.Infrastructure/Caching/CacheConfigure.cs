using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Caching
{
    public static class CacheConfigure
    {
        /// <summary>
        /// Add memory cache service to service container
        /// </summary>
        /// <param name="services">service container</param>
        /// <param name="setupAction">memory cache options</param>
        public static void AddMemoryCacheService(this IServiceCollection services, Action<MemoryCacheOptions>? setupAction = null)
        {
            if (setupAction != null)
            {
                services.AddMemoryCache(setupAction);
            }
            else
            {
                services.AddMemoryCache();
            }
            services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
        }

        /// <summary>
        /// Add redis cache service to service container
        /// </summary>
        /// <param name="services">service container</param>
        /// <param name="setupAction">redis cache options</param>
        public static void AddRedisCacheService(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            Guard.Against.Null(setupAction, nameof(setupAction));
            services.AddStackExchangeRedisCache(setupAction);
            services.AddSingleton<IAsyncCacheService, AsyncDistributedCacheService>();
        }
    }
}