using Pl.Sas.Logger.Data;


namespace Pl.Sas.Logger.Scheduler
{
    /// <summary>
    /// Scheduler auto delete log
    /// </summary>
    public class RecurrentDeleteLog : IHostedService, IDisposable
    {
        private readonly Timer _timer = null!;
        private readonly ILogger<RecurrentDeleteLog> _logger;
        private readonly LoggerData _loggerData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_logger">log file service :D</param>
        /// <param name="_schedulerService">Scheduler service</param>
        public RecurrentDeleteLog(
            LoggerData loggerData,
            ILogger<RecurrentDeleteLog> logger)
        {
            _logger = logger;
            _loggerData = loggerData;
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));
        }

        /// <summary>
        /// Start scheduler
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RecurrentDeleteLog scheduler is starting.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop scheduler
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RecurrentDeleteLog scheduler is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Service delete log
        /// </summary>
        /// <param name="state">Data object</param>
        private void DoWork(object? state)
        {
            var rowDeleted = _loggerData.RecurrentDelete(5000);
            _logger.LogInformation("RecurrentDeleteLog => Deleted {rowDeleted} row at {Now}.", rowDeleted, DateTime.Now);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
