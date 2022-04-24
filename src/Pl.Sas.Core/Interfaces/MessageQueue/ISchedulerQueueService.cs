using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ISchedulerQueueService
    {
        /// <summary>
        /// Gửi một nhiệm vụ cho worker task
        /// </summary>
        /// <param name="queueMessage">Thông tin nhiệm vụ</param>
        void PublishWorkerTask(QueueMessage queueMessage);

        /// <summary>
        /// Thu hồi tài nguyên
        /// </summary>
        void Dispose();
    }
}
