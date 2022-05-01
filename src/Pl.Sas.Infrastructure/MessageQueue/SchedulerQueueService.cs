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

        private readonly IModel _downloadChannel;
        private readonly IBasicProperties _publishDownloadProperties;

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

            _downloadChannel = _connection.CreateModel();
            _downloadChannel.QueueDeclare(queue: MessageQueueConstants.DownloadQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _publishDownloadProperties = _downloadChannel.CreateBasicProperties();
            _publishDownloadProperties.Persistent = true;
        }

        public void PublishDownloadTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _downloadChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.DownloadQueueName, basicProperties: _publishDownloadProperties, body: body);
        }

        public virtual void Dispose()
        {
            _downloadChannel.Dispose();
            _connection.Dispose();
        }
    }
}
