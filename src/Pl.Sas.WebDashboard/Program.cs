using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.WebEncoders;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Core.Services;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Loging;
using Pl.Sas.Infrastructure.RabbitmqMessageQueue;
using Pl.Sas.WebDashboard;
using Pl.Sas.WebDashboard.RealtimeHub;
using StackExchange.Redis;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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

builder.Services.AddDataProtection(opts =>
{
    opts.ApplicationDiscriminator = "PLSASIDENTITY";
}).PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("CacheConnection")), "dashboard-dataprotection");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "pliuddashboard";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
    options.LoginPath = "/dang-nhap";
    options.LogoutPath = "/dang-xuat";
    options.AccessDeniedPath = "/accessdenied.html";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(86400);
});

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

builder.Services.AddResponseCompression();
builder.Services.Configure<WebEncoderOptions>(webEncoderOptions => webEncoderOptions.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddSingleton<IFollowStockData, FollowStockData>();
builder.Services.AddSingleton<IUserData, UserData>();
builder.Services.AddSingleton<IKeyValueData, KeyValueData>();
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
builder.Services.AddSingleton<IWebDashboardQueueService, WebDashboardQueueService>();
builder.Services.AddSingleton<StockViewService>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();


var app = builder.Build();
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature?.Error, "Error url: {url}", url);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("Internal server error.");
        });
    });

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Frame-Options", "sameorigin");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src *; font-src * data:; frame-src *; img-src * data:; media-src *; object-src *; script-src * 'unsafe-inline' 'unsafe-eval'; style-src * 'unsafe-inline';");
        context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "same-origin");
        context.Response.Headers.Add("Feature-Policy", "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("Server");
        await next();
    });

    app.UseForwardedHeaders();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<StockRealtimeHub>("/stockrealtime");

app.Logger.LogDebug("Sas dashboard {AppVersion} started at {Now}", app.Configuration["AppSettings:AppVersion"], DateTime.Now);
app.Run();
