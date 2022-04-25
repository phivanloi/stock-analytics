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
    public class WebDashboardQueueService : IWebDashboardQueueService
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly IZipHelper _zipHelper;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly ILogger<WebDashboardQueueService> _logger;

        private readonly IModel _subscribeUpdateMemoryChannel;

        public WebDashboardQueueService(
            ILogger<WebDashboardQueueService> logger,
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

            _subscribeUpdateMemoryChannel = _connection.CreateModel();
            _subscribeUpdateMemoryChannel.ExchangeDeclare(exchange: MessageQueueConstants.UpdateMemoryExchangeName, type: ExchangeType.Fanout);
        }

        public virtual void SubscribeUpdateMemoryTask(Action<QueueMessage> func)
        {
            var queueName = _subscribeUpdateMemoryChannel.QueueDeclare().QueueName;
            _subscribeUpdateMemoryChannel.QueueBind(queue: queueName, exchange: MessageQueueConstants.UpdateMemoryExchangeName, routingKey: "");
            var consumer = new EventingBasicConsumer(_subscribeUpdateMemoryChannel);
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
                    _logger.LogError(ex, "Run SubscribeUpdateMemoryTask error");
                }
                finally
                {
                    _subscribeUpdateMemoryChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _subscribeUpdateMemoryChannel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public virtual void Dispose()
        {
            _subscribeUpdateMemoryChannel.Dispose();
            _connection.Dispose();
        }
    }
}
