using Envio_de_Emails_Com_Fila_PersistenceWorker.Services;
using Envio_de_Emails_Com_Fila_Shared.Messaging;
using Envio_de_Emails_Com_Fila_Shared.Models;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Envio_de_Emails_Com_Fila_PersistenceWorker
{
    public class Worker : BackgroundService
    {
        private readonly EmailPersistenceService _emailPersistenceService;
        private readonly ILogger<Worker> _logger;

        private IConnection _connection = null!;
        private IChannel _channel = null!;

        public Worker(EmailPersistenceService emailPersistenceService, ILogger<Worker> logger)
        {
            _emailPersistenceService = emailPersistenceService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest"
            };

            await ConnectRabbitAsync(factory);

            await _channel.ExchangeDeclareAsync(
                exchange: RabbitMqTopology.EmailExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            );

            await _channel.QueueDeclareAsync(
                queue: RabbitMqTopology.EmailPersistenceQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await _channel.QueueBindAsync(
                queue: RabbitMqTopology.EmailPersistenceQueueName,
                exchange: RabbitMqTopology.EmailExchangeName,
                routingKey: RabbitMqTopology.EmailReceivedRoutingKey
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var msg = JsonSerializer.Deserialize<EmailMessage>(
                        Encoding.UTF8.GetString(ea.Body.ToArray())
                    );

                    if (msg == null)
                    {
                        throw new JsonException("Mensagem de email invalida.");
                    }

                    await _emailPersistenceService.SaveAsync(msg, stoppingToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao persistir mensagem de email");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: RabbitMqTopology.EmailPersistenceQueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        private async Task ConnectRabbitAsync(ConnectionFactory factory)
        {
            var retryConnection = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .Or<IOException>()
                .WaitAndRetryAsync(
                    retryCount: 10,
                    sleepDurationProvider: attempt =>
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 300));
                        return baseDelay + jitter;
                    },
                    onRetry: (ex, delay, retry, ctx) =>
                    {
                        _logger.LogWarning(ex,
                            "Tentativa {Retry} de conexao com RabbitMQ falhou. Aguardando {Delay}s. Erro: {Message}",
                            retry,
                            delay.TotalSeconds,
                            ex.Message
                        );
                    });

            await retryConnection.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Tentando conectar no RabbitMQ para persistencia...");

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                _logger.LogInformation("Conectado no RabbitMQ para persistencia!");
            });
        }
    }
}
