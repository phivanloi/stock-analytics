using System.Collections.Concurrent;

namespace Pl.Sas.Infrastructure.Loging
{
    public class LoggingBackgroundTaskQueue : ILoggingBackgroundTaskQueue, IDisposable
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem ?? throw new Exception("workItem is null");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _signal.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}