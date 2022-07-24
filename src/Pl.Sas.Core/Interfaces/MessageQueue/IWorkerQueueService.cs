using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IWorkerQueueService
    {
        /// <summary>
        /// Lắng nghe xự kiện update memory, thay đổi dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeUpdateMemoryTask(Action<QueueMessage> func);

        /// <summary>
        /// Phát hành một sự kiện cần update memory
        /// </summary>
        /// <param name="queueMessage">nội dung tin nhắn</param>
        void BroadcastUpdateMemoryTask(QueueMessage queueMessage);

        /// <summary>
        /// Lắng nghe sư kiện yêu cầu phân tích dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeAnalyticsTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Lắng nghe một sự kiện download và lưu trữ dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeDownloadTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Xây dựng dữ liệu cho view
        /// </summary>
        /// <param name="func">Hàm cần phân tích</param>
        void SubscribeBuildViewTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Phát tiếng hiệu update view
        /// </summary>
        /// <param name="queueMessage">Nội dung tin nhắn</param>
        void BroadcastViewUpdatedTask(QueueMessage queueMessage);

        /// <summary>
        /// Gửi một yêu cầu phân tích dữ liệu
        /// </summary>
        /// <param name="queueMessage">Thông tin cần phân tích</param>
        void PublishRealtimeTask(QueueMessage queueMessage);

        /// <summary>
        /// Xử lý yêu cầu reatime
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeRealtimeTask(Func<QueueMessage, Task> func);

        /// <summary>
        /// Thu hồi tài nguyên
        /// </summary>
        void Dispose();
    }
}
