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

            services.AddSingleton<IZipHelper, GZipHelper>();
            services.AddMemoryCacheService();
            services.AddRedisCacheService(option =>
            {
                option.InstanceName = "SASSTOCK";
                option.Configuration = configuration.GetConnectionString("CacheConnection");
            });


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
            return services;
        }
    }
}
