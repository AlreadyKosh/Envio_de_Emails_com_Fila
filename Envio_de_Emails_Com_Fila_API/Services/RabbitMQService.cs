using Envio_de_Emails_Com_Fila.Shared.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Envio_de_Emails_Com_Fila_API.Services
{
    public class RabbitMQService
    {
        private readonly IConnection _connection; //Conexão com Rabbit
        private readonly IChannel _channel; //Canal de Comunicação
        private const string QueueName = "email-queue";

        public RabbitMQService()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync(
                 queue: QueueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false
             ).GetAwaiter().GetResult();
        }

        public async Task Publish(EmailMessage msg)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            var props = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(
               exchange: "",
               routingKey: QueueName,
               mandatory: false,
               basicProperties: props,
               body: body
           );
        }
    }
}
