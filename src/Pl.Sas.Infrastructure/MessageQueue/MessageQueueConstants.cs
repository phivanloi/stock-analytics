namespace Pl.Sas.Infrastructure.RabbitmqMessageQueue
{
    public static class MessageQueueConstants
    {
        /// <summary>
        /// Queue name cho phần download dữ liệu
        /// </summary>
        public const string DownloadQueueName = "DownloadQueueName";

        /// <summary>
        /// Queue name cho phần xử lý realtime
        /// </summary>
        public const string RealtimeQueueName = "RealtimeQueueName";

        /// <summary>
        /// Queue name cho phần phân tích dữ liệu
        /// </summary>
        public const string AnalyticsQueueName = "AnalyticsQueueName";

        /// <summary>
        /// Thực hiện tạo, xử lý dữ liệu cho phần hiển thị view
        /// </summary>
        public static readonly string ViewWorkerQueueName = "ViewWorkerQueueName";

        /// <summary>
        /// Kênh nhận sự kiên update view
        /// </summary>
        public static readonly string ViewUpdatedExchangeName = "ViewUpdatedExchange";

        /// <summary>
        /// Exchange cho phần update memory
        /// </summary>
        public const string UpdateMemoryExchangeName = "UpdateMemoryExchangeName";
    }
}
