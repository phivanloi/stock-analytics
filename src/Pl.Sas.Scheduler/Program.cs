using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Loging;
using Pl.Sas.Infrastructure.RabbitmqMessageQueue;
using Pl.Sas.Scheduler;
using System.Net;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributeLogService(option =>
{
    option.BaseUrl = builder.Configuration["LoggingSettings:BaseUrl"];
    option.Secret = builder.Configuration["LoggingSettings:Secret"];
    option.ServerName = builder.Configuration["LoggingSettings:ServerName"] ?? Utilities.IdentityServer();
});

builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(builder.Configuration.GetConnectionString("MarketConnection"), name: "market-database", tags: new string[] { "market_database", "31_db" })
    .AddRedis(builder.Configuration.GetConnectionString("CacheConnection"), name: "redis-cache", tags: new string[] { "redis_cache", "31_redis" })
    .AddRabbitMQ(builder.Configuration.GetConnectionString("EventBusConnection"), name: "rabbitmq-bus", tags: new string[] { "rabbitmq_bus", "31_rabbitmq" });

builder.Services.AddSingleton<IZipHelper, GZipHelper>();
builder.Services.AddMemoryCacheService();
builder.Services.AddRedisCacheService(option =>
{
    option.InstanceName = "SASSTOCK";
    option.Configuration = builder.Configuration.GetConnectionString("CacheConnection");
});

builder.Services.AddSingleton<IWorkerQueueService, WorkerQueueService>();
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

app.MigrateDbContext<MarketDbContext>((context, services) =>
{
    services.MarketSeed(context);
    app.Logger.LogInformation("MarketDbContext migrations at {Now}", DateTime.Now);
});

app.MapGet("/", () =>
{
    return $"Scheduler version: {builder.Configuration["AppSettings:AppVersion"]}, published date: {builder.Configuration["AppSettings:AppPublishedDate"]}";
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
app.Logger.LogInformation("Dashboard scheduler application {AppVersion} started at {Now}", app.Configuration["AppSettings:AppVersion"], DateTime.Now);
app.Run();