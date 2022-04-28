using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Analytics;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Crawl;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Loging;
using System;
using System.IO;
using System.Net.Http;

namespace Pl.Sas.UnitTests
{
    public class ConfigureServices
    {
        public static IServiceCollection GetConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Tests.json", false)
                .AddEnvironmentVariables()
                .Build();

            services.AddDistributeLogService(option =>
            {
                option.BaseUrl = configuration["LoggingSettings:BaseUrl"];
                option.Secret = configuration["LoggingSettings:Secret"];
                option.ServerName = configuration["LoggingSettings:ServerName"] ?? Utilities.IdentityServer();
            });

            services.AddLogging(builder => builder.AddDebug().AddConsole());
            services.AddOptions();
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            services.AddHttpClient("downloader");

            services.AddSingleton<IZipHelper, GZipHelper>();
            services.AddMemoryCacheService();
            services.AddRedisCacheService(option =>
            {
                option.InstanceName = "SASSTOCK";
                option.Configuration = configuration.GetConnectionString("CacheConnection");
            });

            services.AddDbContext<MarketDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MarketConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                })
            );

            services.AddDbContext<AnalyticsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AnalyticsConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                })
            );

            services.AddSingleton<ICrawlData, CrawlData>();
            services.AddScoped<IMarketData, MarketData>();
            services.AddScoped<IAnalyticsData, AnalyticsData>();
            services.AddScoped<WorkerService>();
            return services;
        }
    }
}
