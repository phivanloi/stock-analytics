using Ardalis.GuardClauses;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.RabbitmqMessageQueue
{
    public class WorkerQueueService : IWorkerQueueService
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly IZipHelper _zipHelper;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly ILogger<WorkerQueueService> _logger;

        private readonly IModel _workerChannel;
        private readonly IModel _broadcastUpdateMemoryChannel;

        public WorkerQueueService(
            ILogger<WorkerQueueService> logger,
            IZipHelper zipHelper,
            IOptionsMonitor<ConnectionStrings> optionsMonitor)
        {
            _logger = logger;
            _connectionStrings = optionsMonitor.CurrentValue;
            _zipHelper = zipHelper;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_connectionStrings.EventBusConnection)
            };
            _connection = _connectionFactory.CreateConnection();

            _workerChannel = _connection.CreateModel();
            _workerChannel.QueueDeclare(queue: MessageQueueConstants.WorkerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _broadcastUpdateMemoryChannel = _connection.CreateModel();
            _broadcastUpdateMemoryChannel.ExchangeDeclare(exchange: MessageQueueConstants.UpdateMemoryExchangeName, type: ExchangeType.Fanout);
        }

        public virtual void SubscribeWorkerTask(Action<QueueMessage> func)
        {
            var consumer = new EventingBasicConsumer(_workerChannel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<QueueMessage>(_zipHelper.UnZipByte(ea.Body.ToArray()));
                    Guard.Against.Null(message, nameof(message));
                    func(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Run SubscribeWorkerTask error");
                }
                finally
                {
                    _workerChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _workerChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _workerChannel.BasicConsume(queue: MessageQueueConstants.WorkerQueueName, autoAck: false, consumer: consumer);
        }

        public virtual void BroadcastUpdateMemoryTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _broadcastUpdateMemoryChannel.BasicPublish(exchange: MessageQueueConstants.UpdateMemoryExchangeName, routingKey: "", basicProperties: null, body: body);
        }

        public virtual void Dispose()
        {
            _workerChannel.Dispose();
            _broadcastUpdateMemoryChannel.Dispose();
            _connection.Dispose();
        }
    }
}
