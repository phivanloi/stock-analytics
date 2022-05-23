namespace Pl.Sas.Infrastructure
{
    public class ConnectionStrings
    {
        /// <summary>
        /// Chuỗi kết nối đến identity connection
        /// </summary>
        public string IdentityConnection { get; set; } = null!;

        /// <summary>
        /// Chuỗi kết nối đến db market lưu dữ liệu gốc crawler về
        /// </summary>
        public string MarketConnection { get; set; } = null!;

        /// <summary>
        /// Chuỗi kết nối đến db market lưu dữ liệu phân tích
        /// </summary>
        public string AnalyticsConnection { get; set; } = null!;

        /// <summary>
        /// Chuỗi kết nối đến redis cache server
        /// </summary>
        public string CacheConnection { get; set; } = null!;

        /// <summary>
        /// Chuỗi kết nối đến event bus
        /// </summary>
        public string EventBusConnection { get; set; } = null!;
    }
}
