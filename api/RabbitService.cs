using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace api
{
    public class RabbitService
    {
        public static async Task SendDocumentUploadedMessageAsync(int documentId, string objectKey, string fileName)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://admin:admin@rabbitmq:5672/");

            Console.WriteLine($" [x] Sending Document Uploaded Message for ID: {documentId}, Path: {objectKey} ...");

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "test_q",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );

            // Eobjekt zusammenbauen
            var messagePayload = new
            {
                DocumentId = documentId,
                ObjectKey = objectKey,
                FileName = fileName
            };

            // als json serialsieren
            var jsonMessage = JsonSerializer.Serialize(messagePayload);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            // Nachricht in queue senden
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "test_q", body: body);

            Console.WriteLine($" [x] Sent structured message for Document ID {documentId}");
        }
    }
}
