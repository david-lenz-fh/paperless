using business_layer.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using business_layer.Dtos;

namespace api
{
    public record SummaryGeneratedMessage(int DocumentId, int FileId, string Category, string Summary);

    public class SummaryConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SummaryConsumerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://admin:admin@rabbitmq:5672/")
            };

            await Task.Delay(5000, stoppingToken);

            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "summaryGenerated",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } },
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<SummaryGeneratedMessage>(json, jsonOptions)
                              ?? throw new InvalidOperationException("Ungültige Summary-Nachricht");

                    using var scope = _scopeFactory.CreateScope();
                    var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
                    await documentService.UpdateFileSummaryAndDocumentCategory(new FileSummaryAndDocumentCategoryDto(msg.DocumentId, msg.FileId, msg.Summary, msg.Category));
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API ERROR] Fehler beim Speichern der Summary: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: "summaryGenerated",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
