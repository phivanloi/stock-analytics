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

        private readonly IModel _analyticsWorkerChannel;
        private readonly IBasicProperties _publishAnalyticsWorkerProperties;

        private readonly IModel _viewWorkerChannel;
        private readonly IBasicProperties _publishViewWorkerProperties;

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

            _analyticsWorkerChannel = _connection.CreateModel();
            _analyticsWorkerChannel.QueueDeclare(queue: MessageQueueConstants.AnalyticsQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _publishAnalyticsWorkerProperties = _analyticsWorkerChannel.CreateBasicProperties();
            _publishAnalyticsWorkerProperties.Persistent = true;

            _viewWorkerChannel = _connection.CreateModel();
            _viewWorkerChannel.QueueDeclare(queue: MessageQueueConstants.ViewWorkerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _publishViewWorkerProperties = _viewWorkerChannel.CreateBasicProperties();
            _publishViewWorkerProperties.Persistent = true;
        }

        public virtual void PublishDownloadTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _downloadChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.DownloadQueueName, basicProperties: _publishDownloadProperties, body: body);
        }

        public virtual void PublishViewWorkerTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _viewWorkerChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.ViewWorkerQueueName, basicProperties: _publishViewWorkerProperties, body: body);
        }

        public virtual void PublishAnalyticsWorkerTask(QueueMessage queueMessage)
        {
            Guard.Against.Null(queueMessage, nameof(queueMessage));
            var body = _zipHelper.ZipByte(JsonSerializer.SerializeToUtf8Bytes(queueMessage));
            _analyticsWorkerChannel.BasicPublish(exchange: "", routingKey: MessageQueueConstants.AnalyticsQueueName, basicProperties: _publishAnalyticsWorkerProperties, body: body);
        }

        public virtual void Dispose()
        {
            _analyticsWorkerChannel.Dispose();
            _viewWorkerChannel.Dispose();
            _downloadChannel.Dispose();
            _connection.Dispose();
        }
    }
}
