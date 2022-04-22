namespace Pl.Sas.Logger
{
    public class Connections
    {
        /// <summary>
        /// kết nối đến db log
        /// </summary>
        public string LoggingConnection { get; set; } = null!;
    }

    public class AppSettings
    {
        /// <summary>
        /// Application vertion
        /// </summary>
        public string AppVersion { get; set; } = null!;

        /// <summary>
        /// Application publish time
        /// </summary>
        public string AppPublishedDate { get; set; } = null!;

        /// <summary>
        /// Khóa bảo vệ khi ghi log
        /// </summary>
        public string SecurityKey { get; set; } = null!;
    }
}