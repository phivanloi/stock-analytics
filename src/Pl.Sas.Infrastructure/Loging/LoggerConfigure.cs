using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pl.Sas.Infrastructure.Loging
{
    public static class LoggerConfigure
    {
        /// <summary>
        /// add log service provider to service container
        /// </summary>
        /// <param name="services">Service container</param>
        /// <param name="setupAction">Setup log action</param>
        public static void AddDistributeLogService(this IServiceCollection services, Action<DistributeLogOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddSingleton<ILoggerProvider, DistributeLoggerProvider>();
            services.AddSingleton<ILoggingBackgroundTaskQueue, LoggingBackgroundTaskQueue>();
            services.AddHostedService<LoggingQueuedHostedService>();
        }
    }
}