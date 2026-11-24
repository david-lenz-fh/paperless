using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace api;

public class RabbitService
{
    private readonly ILogger<RabbitService> _logger;

    public RabbitService(ILogger<RabbitService> logger)
    {
        _logger = logger;
    }

    public async Task SendDocumentUploadedMessageAsync(int documentId, int fileId, string objectKey, string fileName)
    {
        try
        {
            _logger.LogInformation("Publishing document {DocumentId} to RabbitMQ.", documentId);

            var factory = new ConnectionFactory { Uri = new Uri("amqp://admin:admin@rabbitmq:5672/") };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "documentUploaded",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );

            // Eobjekt zusammenbauen
            var messagePayload = new
            {
                DocumentId = documentId,
                FileId = fileId,
                ObjectKey = objectKey,
                FileName = fileName
            };

            // als json serialsieren
            var jsonMessage = JsonSerializer.Serialize(messagePayload);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            // Nachricht in queue senden
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "documentUploaded", body: body);

            _logger.LogInformation("Document {DocumentId} was published to RabbitMQ.", documentId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to publish document {DocumentId} to RabbitMQ.", documentId);
            throw;
        }
    }
}
