namespace Pl.Sas.Infrastructure.RabbitmqMessageQueue
{
    public static class MessageQueueConstants
    {
        /// <summary>
        /// Queue cho phần worker
        /// </summary>
        public const string WorkerQueueName = "WorkerQueueName";

        /// <summary>
        /// Exchange cho phần update memory
        /// </summary>
        public const string UpdateMemoryExchangeName = "UpdateMemoryExchangeName";
    }
}
