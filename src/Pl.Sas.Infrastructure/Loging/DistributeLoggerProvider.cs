using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pl.Sas.Infrastructure.Loging
{
    public class DistributeLoggerProvider : ILoggerProvider
    {
        private readonly DistributeLogOptions _dbLogOptions;
        private readonly ILoggingBackgroundTaskQueue _logTaskQueue;

        public DistributeLoggerProvider(
            IOptions<DistributeLogOptions> options,
            ILoggingBackgroundTaskQueue logTaskQueue)
        {
            _dbLogOptions = options.Value;
            _logTaskQueue = logTaskQueue;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DistributeLogger(categoryName, _dbLogOptions, _logTaskQueue);
        }

        public void Dispose()
        {            
            GC.SuppressFinalize(this);
        }
    }
}