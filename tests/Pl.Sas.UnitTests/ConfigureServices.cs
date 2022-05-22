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
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Identity;
using Pl.Sas.Infrastructure.Loging;
using Pl.Sas.Infrastructure.Market;
using Pl.Sas.Infrastructure.RabbitmqMessageQueue;
using Pl.Sas.Infrastructure.System;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

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

            services.AddHttpClient("downloader", c =>
            {
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36 Edg/86.0.622.51");
                c.DefaultRequestHeaders.Add("Referer", "https://iboard.ssi.com.vn/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler()
                {
                    UseCookies = false
                };
            });

            services.AddDbContext<SystemDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SystemConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ConfigureServices).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                }));
            services.AddDbContext<MarketDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MarketConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ConfigureServices).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                }));
            services.AddDbContext<AnalyticsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AnalyticsConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ConfigureServices).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                }));
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ConfigureServices).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                }));

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

            services.AddSingleton<IWorkerQueueService, WorkerQueueService>();
            services.AddSingleton<IDownloadData, DownloadData>();
            services.AddScoped<IMarketData, MarketData>();
            services.AddScoped<IAnalyticsData, AnalyticsData>();
            services.AddScoped<ISystemData, SystemData>();
            services.AddScoped<IIdentityData, IdentityData>();
            services.AddScoped<DownloadService>();
            services.AddScoped<AnalyticsService>();
            services.AddScoped<StockViewService>();
            return services;
        }
    }
}
