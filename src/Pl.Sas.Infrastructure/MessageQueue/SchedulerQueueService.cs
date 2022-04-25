using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using RabbitMQ.Client;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.RabbitmqMessageQueue
{
    public class SchedulerQueueService : ISchedulerQueueService
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly IZipHelper _zipHelper;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;

        private readonly IModel _workerChannel;
        private readonly IBasicProperties _publishWorkerProperties;

        public SchedulerQueueService(
            IZipHelper zipHelper,
            IOptionsMonitor<ConnectionStrings> optionsMonitor)
        {
            _connectionStrings = optionsMonitor.CurrentValue;
            _zipHelper = zipHelper;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_connectionStrings.EventBusConnection)
            };
            _connection = _connectionFactory.CreateConnection();

            _workerChannel = _connection.CreateModel();
            _workerChannel.QueueDeclare(queue: MessageQueueConstants.WorkerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _publishWorkerProperties = _workerChannel.CreateBasicProperties();
            _publishWorkerProperties.Persistent = true;
        }

        public void PublishWorkerTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _workerChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.WorkerQueueName, basicProperties: _publishWorkerProperties, body: body);
        }

        public virtual void Dispose()
        {
            _workerChannel.Dispose();
            _connection.Dispose();
        }
    }
}
