using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.InvestmentPrinciplesTests;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions();
        services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
        services.Configure<ConnectionStrings>(hostContext.Configuration.GetSection("ConnectionStrings"));

        services.AddSingleton<IZipHelper, GZipHelper>();
        services.AddMemoryCacheService();
        services.AddRedisCacheService(option =>
        {
            option.InstanceName = "SASSTOCK";
            option.Configuration = hostContext.Configuration.GetConnectionString("CacheConnection");
        });


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
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
