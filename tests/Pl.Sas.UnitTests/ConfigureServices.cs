using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Loging;
using Pl.Sas.Infrastructure.RabbitmqMessageQueue;
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
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));

            services.AddHttpClient("ssidownloader", c =>
            {
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
                c.DefaultRequestHeaders.Add("Referer", "https://iboard.ssi.com.vn/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler()
                {
                    UseCookies = false
                };
            });
            services.AddHttpClient("vnddownloader", c =>
            {
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
                c.DefaultRequestHeaders.Add("Referer", "https://dchart.vndirect.com.vn/");
                c.DefaultRequestHeaders.Add("Origin", "https://dchart.vndirect.com.vn/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler()
                {
                    UseCookies = false
                };
            });
            services.AddHttpClient("fiindownloader", c =>
            {
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
                c.DefaultRequestHeaders.Add("Referer", "https://fiintrade.vn");
                c.DefaultRequestHeaders.Add("Origin", "https://fiintrade.vn");
                c.DefaultRequestHeaders.Add("Authorization", "Bearer");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler()
                {
                    UseCookies = false
                };
            });
            services.AddHttpClient("vpsdownloader", c =>
            {
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
                c.DefaultRequestHeaders.Add("Referer", "https://chart.vps.com.vn");
                c.DefaultRequestHeaders.Add("Origin", "https://chart.vps.com.vn");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler()
                {
                    UseCookies = false
                };
            });

            services.AddSingleton<IZipHelper, GZipHelper>();
            services.AddMemoryCacheService();
            services.AddRedisCacheService(option =>
            {
                option.InstanceName = "SASSTOCK";
                option.Configuration = configuration.GetConnectionString("CacheConnection");
            });

            services.AddSingleton<IFollowStockData, FollowStockData>();
            services.AddSingleton<IUserData, UserData>();
            services.AddSingleton<IKeyValueData, KeyValueData>();
            services.AddSingleton<IDownloadData, DownloadData>();
            services.AddSingleton<IStockData, StockData>();
            services.AddSingleton<IStockPriceData, StockPriceData>();
            services.AddSingleton<ICompanyData, CompanyData>();
            services.AddSingleton<IIndustryData, IndustryData>();
            services.AddSingleton<ICorporateActionData, CorporateActionData>();
            services.AddSingleton<IFinancialIndicatorData, FinancialIndicatorData>();
            services.AddSingleton<IFinancialGrowthData, FinancialGrowthData>();
            services.AddSingleton<ILeadershipData, LeadershipData>();
            services.AddSingleton<IStockTransactionData, StockTransactionData>();
            services.AddSingleton<IStockRecommendationData, StockRecommendationData>();
            services.AddSingleton<IVndStockScoreData, VndStockScoreData>();
            services.AddSingleton<IFiinEvaluatedData, FiinEvaluatedData>();
            services.AddSingleton<IScheduleData, ScheduleData>();
            services.AddSingleton<ITradingResultData, TradingResultData>();
            services.AddSingleton<IAnalyticsResultData, AnalyticsResultData>();
            services.AddSingleton<IChartPriceData, ChartPriceData>();

            services.AddSingleton<IWorkerQueueService, WorkerQueueService>();
            services.AddScoped<DownloadService>();
            services.AddScoped<AnalyticsService>();
            services.AddScoped<StockViewService>();
            services.AddScoped<RealtimeService>();
            return services;
        }
    }
}
