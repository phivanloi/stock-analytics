using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
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

        private readonly IModel _subscribeDownloadChannel;
        private readonly IModel _subscribeAnalyticsChannel;
        private readonly IModel _subscribeBuildViewChannel;
        private readonly IModel _subscribeRealtimeChannel;
        private readonly IModel _broadcastUpdateMemoryChannel;
        private readonly IModel _broadcastViewUpdatedChannel;

        private readonly IModel _realtimeWorkerChannel;
        private readonly IBasicProperties _publishRealtimeProperties;

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

            _subscribeDownloadChannel = _connection.CreateModel();
            _subscribeDownloadChannel.QueueDeclare(queue: MessageQueueConstants.DownloadQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _subscribeRealtimeChannel = _connection.CreateModel();
            _subscribeRealtimeChannel.QueueDeclare(queue: MessageQueueConstants.RealtimeQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _subscribeAnalyticsChannel = _connection.CreateModel();
            _subscribeAnalyticsChannel.QueueDeclare(queue: MessageQueueConstants.AnalyticsQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _subscribeBuildViewChannel = _connection.CreateModel();
            _subscribeBuildViewChannel.QueueDeclare(queue: MessageQueueConstants.ViewWorkerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _broadcastUpdateMemoryChannel = _connection.CreateModel();
            _broadcastUpdateMemoryChannel.ExchangeDeclare(exchange: MessageQueueConstants.UpdateMemoryExchangeName, type: ExchangeType.Fanout);

            _broadcastViewUpdatedChannel = _connection.CreateModel();
            _broadcastViewUpdatedChannel.ExchangeDeclare(exchange: MessageQueueConstants.ViewUpdatedExchangeName, type: ExchangeType.Fanout);

            _realtimeWorkerChannel = _connection.CreateModel();
            _realtimeWorkerChannel.QueueDeclare(queue: MessageQueueConstants.RealtimeQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _publishRealtimeProperties = _realtimeWorkerChannel.CreateBasicProperties();
            _publishRealtimeProperties.Persistent = true;
        }

        public virtual void SubscribeDownloadTask(Func<QueueMessage, Task> func)
        {
            var consumer = new EventingBasicConsumer(_subscribeDownloadChannel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<QueueMessage>(_zipHelper.UnZipByte(ea.Body.ToArray()));
                    Guard.Against.Null(message, nameof(message));
                    await func(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Run SubscribeDownloadTask error");
                }
                finally
                {
                    _subscribeDownloadChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _subscribeDownloadChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _subscribeDownloadChannel.BasicConsume(queue: MessageQueueConstants.DownloadQueueName, autoAck: false, consumer: consumer);
        }

        public virtual void SubscribeAnalyticsTask(Func<QueueMessage, Task> func)
        {
            var consumer = new EventingBasicConsumer(_subscribeDownloadChannel);
            consumer.Received += async (model, ea) =>
            {
                var message = JsonSerializer.Deserialize<QueueMessage>(_zipHelper.UnZipByte(ea.Body.ToArray()));
                try
                {
                    Guard.Against.Null(message, nameof(message));
                    await func(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new Exception(JsonSerializer.Serialize(message)), "Message");
                    _logger.LogError(ex, "Run SubscribeAnalyticsTask error");
                }
                finally
                {
                    _subscribeDownloadChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _subscribeDownloadChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _subscribeDownloadChannel.BasicConsume(queue: MessageQueueConstants.AnalyticsQueueName, autoAck: false, consumer: consumer);
        }

        public virtual void SubscribeBuildViewTask(Func<QueueMessage, Task> func)
        {
            var consumer = new EventingBasicConsumer(_subscribeBuildViewChannel);
            consumer.Received += async (model, ea) =>
            {
                var message = JsonSerializer.Deserialize<QueueMessage>(_zipHelper.UnZipByte(ea.Body.ToArray()));
                Guard.Against.Null(message, nameof(message));
                try
                {
                    await func(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new Exception(JsonSerializer.Serialize(message) + ex.ToString()), "Run SubscribeBuildViewTask error");
                }
                finally
                {
                    _subscribeBuildViewChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _subscribeBuildViewChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _subscribeBuildViewChannel.BasicConsume(queue: MessageQueueConstants.ViewWorkerQueueName, autoAck: false, consumer: consumer);
        }

        public virtual void SubscribeRealtimeTask(Func<QueueMessage, Task> func)
        {
            var consumer = new EventingBasicConsumer(_subscribeRealtimeChannel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<QueueMessage>(_zipHelper.UnZipByte(ea.Body.ToArray()));
                    Guard.Against.Null(message, nameof(message));
                    await func(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Run SubscribeRealtimeTask error");
                }
                finally
                {
                    _subscribeRealtimeChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _subscribeRealtimeChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _subscribeRealtimeChannel.BasicConsume(queue: MessageQueueConstants.RealtimeQueueName, autoAck: false, consumer: consumer);
        }

        public virtual void BroadcastUpdateMemoryTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _broadcastUpdateMemoryChannel.BasicPublish(exchange: MessageQueueConstants.UpdateMemoryExchangeName, routingKey: "", basicProperties: null, body: body);
        }

        public virtual void BroadcastViewUpdatedTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _broadcastViewUpdatedChannel.BasicPublish(exchange: MessageQueueConstants.ViewUpdatedExchangeName, routingKey: "", basicProperties: null, body: body);
        }

        public virtual void PublishRealtimeTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _realtimeWorkerChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.RealtimeQueueName, basicProperties: _publishRealtimeProperties, body: body);
        }

        public virtual void Dispose()
        {
            _subscribeDownloadChannel.Dispose();
            _subscribeAnalyticsChannel.Dispose();
            _subscribeBuildViewChannel.Dispose();
            _broadcastUpdateMemoryChannel.Dispose();
            _broadcastViewUpdatedChannel.Dispose();
            _subscribeRealtimeChannel.Dispose();
            _realtimeWorkerChannel.Dispose();
            _connection.Dispose();
        }
    }
}
