using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Loging;
using Pl.Sas.Infrastructure.RabbitmqMessageQueue;
using Pl.Sas.Worker;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
}
builder.Services.AddDistributeLogService(option =>
{
    option.BaseUrl = builder.Configuration["LoggingSettings:BaseUrl"];
    option.Secret = builder.Configuration["LoggingSettings:Secret"];
    option.ServerName = builder.Configuration["LoggingSettings:ServerName"] ?? Utilities.IdentityServer();
    if (!builder.Environment.IsProduction())
    {
        option.FilterLogLevels = new HashSet<LogLevel>();
    }
});

builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddHttpClient("ssidownloader", c =>
{
    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
    c.DefaultRequestHeaders.Add("Referer", "https://iboard.ssi.com.vn/");
}).ConfigurePrimaryHttpMessageHandler(() => { return new SocketsHttpHandler() { UseCookies = false }; });
builder.Services.AddHttpClient("vnddownloader", c =>
{
    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
    c.DefaultRequestHeaders.Add("Referer", "https://dchart.vndirect.com.vn/");
    c.DefaultRequestHeaders.Add("Origin", "https://dchart.vndirect.com.vn/");
}).ConfigurePrimaryHttpMessageHandler(() => { return new SocketsHttpHandler() { UseCookies = false }; });
builder.Services.AddHttpClient("fiindownloader", c =>
{
    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
    c.DefaultRequestHeaders.Add("Referer", "https://fiintrade.vn");
    c.DefaultRequestHeaders.Add("Origin", "https://fiintrade.vn");
    c.DefaultRequestHeaders.Add("Authorization", "Bearer");
}).ConfigurePrimaryHttpMessageHandler(() => { return new SocketsHttpHandler() { UseCookies = false }; });
builder.Services.AddHttpClient("vpsdownloader", c =>
{
    c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.64 Safari/537.36 Edg/101.0.1210.53");
    c.DefaultRequestHeaders.Add("Referer", "https://chart.vps.com.vn");
    c.DefaultRequestHeaders.Add("Origin", "https://chart.vps.com.vn");
}).ConfigurePrimaryHttpMessageHandler(() => { return new SocketsHttpHandler() { UseCookies = false }; });

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(builder.Configuration.GetConnectionString("SystemConnection"), name: "system-database", tags: new string[] { "system_database", "31_db" })
    .AddSqlServer(builder.Configuration.GetConnectionString("AnalyticsConnection"), name: "analytics-database", tags: new string[] { "analytics_database", "31_db" })
    .AddSqlServer(builder.Configuration.GetConnectionString("MarketConnection"), name: "market-database", tags: new string[] { "market_database", "31_db" })
    .AddSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"), name: "identity-database", tags: new string[] { "identity_database", "31_db" })
    .AddRedis(builder.Configuration.GetConnectionString("CacheConnection"), name: "redis-cache", tags: new string[] { "redis_cache", "31_redis" })
    .AddRabbitMQ(builder.Configuration.GetConnectionString("EventBusConnection"), name: "rabbitmq-bus", tags: new string[] { "rabbitmq_bus", "31_rabbitmq" });

builder.Services.AddSingleton<IZipHelper, GZipHelper>();
builder.Services.AddMemoryCacheService();
builder.Services.AddRedisCacheService(option =>
{
    option.InstanceName = "SASSTOCK";
    option.Configuration = builder.Configuration.GetConnectionString("CacheConnection");
});

builder.Services.AddSingleton<IKeyValueData, KeyValueData>();
builder.Services.AddSingleton<IDownloadData, DownloadData>();
builder.Services.AddSingleton<IStockData, StockData>();
builder.Services.AddSingleton<IStockPriceData, StockPriceData>();
builder.Services.AddSingleton<ICompanyData, CompanyData>();
builder.Services.AddSingleton<IIndustryData, IndustryData>();
builder.Services.AddSingleton<ICorporateActionData, CorporateActionData>();
builder.Services.AddSingleton<IFinancialIndicatorData, FinancialIndicatorData>();
builder.Services.AddSingleton<IFinancialGrowthData, FinancialGrowthData>();
builder.Services.AddSingleton<ILeadershipData, LeadershipData>();
builder.Services.AddSingleton<IStockTransactionData, StockTransactionData>();
builder.Services.AddSingleton<IStockRecommendationData, StockRecommendationData>();
builder.Services.AddSingleton<IVndStockScoreData, VndStockScoreData>();
builder.Services.AddSingleton<IFiinEvaluatedData, FiinEvaluatedData>();
builder.Services.AddSingleton<IScheduleData, ScheduleData>();
builder.Services.AddSingleton<ITradingResultData, TradingResultData>();
builder.Services.AddSingleton<IAnalyticsResultData, AnalyticsResultData>();
builder.Services.AddSingleton<IChartPriceData, ChartPriceData>();

builder.Services.AddSingleton<IMemoryUpdateService, MemoryUpdateService>();
builder.Services.AddSingleton<IWorkerQueueService, WorkerQueueService>();
builder.Services.AddSingleton<DownloadService>();
builder.Services.AddSingleton<AnalyticsService>();
builder.Services.AddSingleton<StockViewService>();
builder.Services.AddSingleton<RealtimeService>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature?.Error, "");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Code = "-1",
                Message = "Error on process your request."
            }));
        });
    });
}
else
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", () =>
{
    return $"Worker version: {builder.Configuration["AppSettings:AppVersion"]}, published date: {builder.Configuration["AppSettings:AppPublishedDate"]}";
});

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});
app.Logger.LogInformation("Worker service {AppVersion} started at {Now}", app.Configuration["AppSettings:AppVersion"], DateTime.Now);
app.Run();