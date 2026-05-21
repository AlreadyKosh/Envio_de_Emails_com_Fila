using Envio_de_Emails_Com_Fila_Shared.Models;
using Envio_de_Emails_Com_Fila_Worker.Services;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Envio_de_Emails_Com_Fila_Worker
{
    public class Worker : BackgroundService
    {
        private readonly EmailService _emailService;
        private readonly ILogger<Worker> _logger;

        private IConnection _connection;
        private IChannel _channel;

        private const string QueueName = "email-queue";

        public Worker(EmailService emailService, ILogger<Worker> logger)
        {
            _emailService = emailService;
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

            await ConnectRabbitAsync(factory, stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var msg = JsonSerializer.Deserialize<EmailMessage>(
                    Encoding.UTF8.GetString(ea.Body.ToArray())
                );

                try
                {
                    await ProcessWithRetry(msg, stoppingToken);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem");

                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        private async Task ConnectRabbitAsync(ConnectionFactory factory, CancellationToken ct)
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
                            "Tentativa {Retry} de conexão com RabbitMQ falhou. Aguardando {Delay}s. Erro: {Message}",
                            retry,
                            delay.TotalSeconds,
                            ex.Message
                        );
                    });

            await retryConnection.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Tentando conectar no RabbitMQ...");

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                _logger.LogInformation("Conectado no RabbitMQ!");
            });
        }

        private async Task ProcessWithRetry(EmailMessage msg, CancellationToken ct)
        {
            int[] delays = { 1000, 3000, 9000 };

            for (int i = 0; i < delays.Length; i++)
            {
                try
                {
                    if (msg.To.Contains("fail"))
                    {
                        throw new Exception("Erro");
                    }

                    await _emailService.SendEmail(msg);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Tentativa {i + 1} falhou: {ex.Message}");

                    if (i == delays.Length - 1)
                        throw;

                    await Task.Delay(delays[i], ct);
                }
            }
        }
    }
}