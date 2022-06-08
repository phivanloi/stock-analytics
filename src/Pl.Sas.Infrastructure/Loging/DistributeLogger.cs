using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Loging
{
    public class DistributeLogger : ILogger
    {
        private readonly DistributeLogOptions _distributeLogOptions;
        private readonly ILoggingBackgroundTaskQueue _loggingBackgroundTaskQueue;
        private readonly string _name = null!;
        private static HttpClient? _httpClient;

        public DistributeLogger(
            string name,
            DistributeLogOptions dbLogOptions,
            ILoggingBackgroundTaskQueue loggingTaskQueue)
        {
            _distributeLogOptions = dbLogOptions;
            _name = name;
            _loggingBackgroundTaskQueue = loggingTaskQueue;
        }

        public virtual IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return !_distributeLogOptions.FilterLogLevels.Contains(logLevel);
        }

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var logBuilder = new StringBuilder();
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception != null)
            {
                message = exception.Message;
            }

            if (_distributeLogOptions.FilterMessages.Count > 0 && _distributeLogOptions.FilterMessages.Any(q => message.Contains(q)))
            {
                return;
            }

            message = $"{logLevel}: {message}";
            var fullMessage = CreateFullMessage(logLevel, eventId, message, exception);

            if (null == _httpClient)
            {
                _httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(_distributeLogOptions.BaseUrl),
                };
                _httpClient.DefaultRequestHeaders.Add("SecurityKey", _distributeLogOptions.Secret);
            }

            _loggingBackgroundTaskQueue.QueueBackgroundWorkItem(async token =>
            {
                var postObject = new
                {
                    Host = _distributeLogOptions.ServerName,
                    Message = message,
                    FullMessage = fullMessage,
                    Type = (byte)logLevel
                };
                await _httpClient.PostAsync($"write", new StringContent(JsonSerializer.Serialize(postObject), Encoding.UTF8, "application/json"), CancellationToken.None);
                fullMessage = null;
                message = null;
                postObject = null;
            });

            string CreateFullMessage(LogLevel logLevel, EventId eventId, string message, Exception? exception)
            {
                var logBuilder = new StringBuilder();

                var logLevelString = logLevel.ToString();
                logBuilder.Append($"{DateTime.Now.ToString()} - {logLevelString} - {_name}");

                if (eventId.Id > 0)
                {
                    logBuilder.Append($" [ {eventId.Id} - {eventId.Name ?? "null"} ] ");
                }

                AppendAndReplaceNewLine(logBuilder, message);

                if (exception != null)
                {
                    logBuilder.Append(' ');
                    AppendAndReplaceNewLine(logBuilder, exception.ToString());
                }

                return logBuilder.ToString();

                static void AppendAndReplaceNewLine(StringBuilder sb, string message)
                {
                    var len = sb.Length;
                    sb.Append(message);
                    sb.Replace(Environment.NewLine, " ", len, message.Length);
                }
            }
        }
    }

    internal sealed class NullScope : IDisposable
    {
        public static NullScope Instance
        {
            get;
        } = new NullScope();


        private NullScope()
        {
        }

        public void Dispose()
        {
        }
    }
}