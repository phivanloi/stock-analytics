using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Analytics;
using Pl.Sas.Infrastructure.Caching;
using Pl.Sas.Infrastructure.Helper;
using Pl.Sas.Infrastructure.Market;
using Pl.Sas.Infrastructure.System;
using Pl.Sas.InvestmentPrinciplesTests;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<SystemDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("SystemConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }));
        services.AddDbContext<MarketDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("MarketConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }));
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("AnalyticsConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }));
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("IdentityConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }));

        services.AddSingleton<IZipHelper, GZipHelper>();
        services.AddMemoryCacheService();
        services.AddRedisCacheService(option =>
        {
            option.InstanceName = "SASSTOCK";
            option.Configuration = hostContext.Configuration.GetConnectionString("CacheConnection");
        });

        services.AddScoped<IMarketData, MarketData>();
        services.AddScoped<IAnalyticsData, AnalyticsData>();
        services.AddScoped<ISystemData, SystemData>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
