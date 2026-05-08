using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Envio_de_Emails_Com_Fila.Shared.Models;
using Envio_de_Emails_Com_Fila_Worker.Services;
using Polly.Retry;
using Polly;

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
                HostName = "localhost"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

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
                    var policy = CreateRetryPolicy();

                    await policy.ExecuteAsync(async () =>
                    {
                        if (msg.To.Contains("fail"))
                            throw new Exception("Erro forçado");

                        await _emailService.SendEmail(msg);
                    });

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

        private AsyncRetryPolicy CreateRetryPolicy()
        {
            int[] delays = { 1, 3, 9 };

            return Policy
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
    }
}