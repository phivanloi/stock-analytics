using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ISchedulerQueueService
    {
        /// <summary>
        /// Gửi một nhiệm vụ cho worker task
        /// </summary>
        /// <param name="queueMessage">Thông tin nhiệm vụ</param>
        void PublishDownloadTask(QueueMessage queueMessage);

        /// <summary>
        /// Gửi một nhiệm vụ phân tích chứng khoán vào message queue
        /// </summary>
        /// <param name="queueMessage">Nội dung message</param>
        void PublishAnalyticsWorkerTask(QueueMessage queueMessage);

        /// <summary>
        /// Xứ lý phần phân tích hiển thị và phần thông báo
        /// </summary>
        /// <param name="queueMessage">Nội dung message</param>
        void PublishViewWorkerTask(QueueMessage queueMessage);

        /// <summary>
        /// Thu hồi tài nguyên
        /// </summary>
        void Dispose();
    }
}
