using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IWebDashboardQueueService
    {
        /// <summary>
        /// Lắng nghe xự kiện update memory, thay đổi dữ liệu
        /// </summary>
        /// <param name="func">Hàm xử lý</param>
        void SubscribeUpdateMemoryTask(Action<QueueMessage> func);

        /// <summary>
        /// Thu hồi tài nguyên
        /// </summary>
        void Dispose();
    }
}
