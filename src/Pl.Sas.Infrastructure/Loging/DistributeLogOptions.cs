using Microsoft.Extensions.Logging;

namespace Pl.Sas.Infrastructure.Loging
{
    public class DistributeLogOptions
    {
        /// <summary>
        /// base url string to logging
        /// </summary>
        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// finlter log by specify logleves
        /// </summary>
        public HashSet<LogLevel> FilterLogLevels { get; } = new HashSet<LogLevel>() { LogLevel.Information, LogLevel.Trace, LogLevel.Debug };

        /// <summary>
        /// finlter log by message contains
        /// </summary>
        public HashSet<string> FilterMessages { get; } = new HashSet<string>();

        /// <summary>
        /// logging secret
        /// </summary>
        public string Secret { get; set; } = null!;

        /// <summary>
        /// Set name of server
        /// </summary>
        public string? ServerName { get; set; }
    }
}