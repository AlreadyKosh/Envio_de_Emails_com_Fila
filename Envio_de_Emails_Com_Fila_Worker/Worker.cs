using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Envio_de_Emails_Com_Fila.Shared.Models;
using Envio_de_Emails_Com_Fila_Worker.Services;

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