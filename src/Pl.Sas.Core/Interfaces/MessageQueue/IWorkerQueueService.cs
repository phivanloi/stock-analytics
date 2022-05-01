using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IWorkerQueueService
    {
        /// <summary>
        /// Phát hành một sự kiện cần update memory task
        /// </summary>
        /// <param name="queueMessage">nội dung tin nhắn</param>
        void BroadcastUpdateMemoryTask(QueueMessage queueMessage);

        /// <summary>
        /// Lắng nghe sư kiện yêu cầu phân tích dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeAnalyticsTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Lắng nghe một sự kiện download dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeDownloadTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Thu hồi tài nguyên
        /// </summary>
        void Dispose();
    }
}
