using Envio_de_Emails_Com_Fila_Shared.Messaging;
using Envio_de_Emails_Com_Fila_Shared.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Envio_de_Emails_Com_Fila_API.Services
{
    public class RabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQService()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            DeclareTopology();
        }

        public async Task Publish(EmailMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.MessageId))
            {
                msg.MessageId = Guid.NewGuid().ToString("N");
            }

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            var props = new BasicProperties
            {
                MessageId = msg.MessageId,
                Persistent = true
            };

            await _channel.BasicPublishAsync(
                exchange: RabbitMqTopology.EmailExchangeName,
                routingKey: RabbitMqTopology.EmailReceivedRoutingKey,
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        private void DeclareTopology()
        {
            _channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.EmailExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            ).GetAwaiter().GetResult();

            DeclareAndBindQueue(RabbitMqTopology.EmailQueueName);
            DeclareAndBindQueue(RabbitMqTopology.EmailPersistenceQueueName);
        }

        private void DeclareAndBindQueue(string queueName)
        {
            _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            ).GetAwaiter().GetResult();

            _channel.QueueBindAsync(
                queue: queueName,
                exchange: RabbitMqTopology.EmailExchangeName,
                routingKey: RabbitMqTopology.EmailReceivedRoutingKey
            ).GetAwaiter().GetResult();
        }
    }
}
