using Envio_de_Emails_Com_Fila.Shared.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Retry;

namespace Envio_de_Emails_Com_Fila_API.Services
{
    public class RabbitMQService
    {
        private readonly IConnection _connection; //Conexão com Rabbit
        private readonly IChannel _channel; //Canal de Comunicação
        private const string QueueName = "email-queue";
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(ILogger<RabbitMQService> logger)
        {
            _logger = logger;

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

            int[] delays = { 1, 3, 9 };
            _retryPolicy = Policy
            .Handle<Exception>()
             .WaitAndRetryAsync(
                    delays.Length,
                    retryAttempt => TimeSpan.FromSeconds(delays[retryAttempt - 1]),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            $"Tentativa {retryCount} falhou. Próxima em {timeSpan.TotalSeconds}s"
                        );
                    });
        }

        public async Task Publish(EmailMessage msg)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            var props = new BasicProperties
            {
                Persistent = true
            };

            await _retryPolicy.ExecuteAsync(async () =>
            {
                // Simulação de erro (opcional pra teste)
                if (msg.To.Contains("fail"))
                {
                    throw new Exception("Erro forçado");
                }

                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: QueueName,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );
            });
        }
    }
}
