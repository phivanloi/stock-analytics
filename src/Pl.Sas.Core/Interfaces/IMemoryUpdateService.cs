using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IMemoryUpdateService
    {
        /// <summary>
        /// Xử lý cập nhập bộ nhớ với queue message
        /// </summary>
        /// <param name="queueMessage"></param>
        void HandleUpdateByQueueMessage(QueueMessage queueMessage);
    }
}