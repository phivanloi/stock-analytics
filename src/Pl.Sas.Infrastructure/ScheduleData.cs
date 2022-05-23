using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Pl.Sas.Infrastructure
{
    public class BaseData
    {
        protected readonly ConnectionStrings _connectionStrings;
        protected AsyncRetryPolicy _dbAsyncRetry = null!;

        public BaseData(IOptionsMonitor<ConnectionStrings> options)
        {
            _connectionStrings = options.CurrentValue;
            _dbAsyncRetry = Policy.Handle<Exception>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}