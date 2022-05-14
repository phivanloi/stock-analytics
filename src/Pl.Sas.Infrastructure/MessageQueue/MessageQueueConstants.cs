namespace Pl.Sas.Infrastructure.RabbitmqMessageQueue
{
    public static class MessageQueueConstants
    {
        /// <summary>
        /// Queue name cho phần download dữ liệu
        /// </summary>
        public const string DownloadQueueName = "DownloadQueueName";

        /// <summary>
        /// Queue name cho phần phân tích dữ liệu
        /// </summary>
        public const string AnalyticsQueueName = "AnalyticsQueueName";

        /// <summary>
        /// Exchange cho phần update memory
        /// </summary>
        public const string UpdateMemoryExchangeName = "UpdateMemoryExchangeName";

        /// <summary>
        /// Thực hiện tạo dữ liệu view
        /// </summary>
        public static readonly string ViewWorkerQueueName = "ViewWorkerQueueName";

        /// <summary>
        /// Kênh nhận dự kiện update view
        /// </summary>
        public static readonly string ViewUpdatedExchangeName = "ViewUpdatedExchange";
    }
}
