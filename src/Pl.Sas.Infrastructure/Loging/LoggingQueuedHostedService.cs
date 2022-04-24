using Microsoft.Extensions.Hosting;

namespace Pl.Sas.Infrastructure.Loging
{
    public class LoggingQueuedHostedService : BackgroundService
    {
        public LoggingQueuedHostedService(ILoggingBackgroundTaskQueue logTaskQueue)
        {
            LogTaskQueue = logTaskQueue;
        }

        public ILoggingBackgroundTaskQueue LogTaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var logWorkItem = await LogTaskQueue.DequeueAsync(stoppingToken);
                try
                {
                    await logWorkItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred executing {nameof(logWorkItem)}: {ex}");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}