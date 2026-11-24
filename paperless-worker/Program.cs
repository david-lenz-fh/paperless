using Minio;
using paperless_worker;
using paperless_worker.services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace paperless_worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await RunAsync(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CRITICAL] Worker konnte nicht gestartet werden: {ex}");
                Environment.ExitCode = 1;
            }
        }

        private static async Task RunAsync(string[] args)
        {
            Console.WriteLine("=== [OCR WORKER] gestartet, verbinde mit RabbitMQ... ===");

            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://admin:admin@rabbitmq:5672/")
            };

            await Task.Delay(5000);

            using var connection = await factory.CreateConnectionAsync();

            using var documentUploadChannel = await connection.CreateChannelAsync();
            await documentUploadChannel.QueueDeclareAsync(
                queue: "documentUploaded",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );

            using var textExtractedPublishChannel = await connection.CreateChannelAsync();
            await textExtractedPublishChannel.QueueDeclareAsync(
                queue: "textExtracted",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );

            using var textExtractedReceiveChannel = await connection.CreateChannelAsync();
            await textExtractedReceiveChannel.QueueDeclareAsync(
                queue: "textExtracted",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );
            using var summaryGeneratedPublishChannel = await connection.CreateChannelAsync();
            await summaryGeneratedPublishChannel.QueueDeclareAsync(
                queue: "summaryGenerated",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
            );
            var documentUploadConsumer = new AsyncEventingBasicConsumer(documentUploadChannel);
            var textExtractedConsumer = new AsyncEventingBasicConsumer(textExtractedReceiveChannel);

            // MinIO initialisieren
            var minioClient = new MinioClient()
                .WithEndpoint("minio:9000")
                .WithCredentials("minioadmin", "minioadminpassword")
                .WithSSL(false)
                .Build();

            var ocrService = new OcrService(minioClient);
            var elasticService = new ElasticsearchService();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            documentUploadConsumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine("[INFO] Nachricht für OCR-Verarbeitung empfangen.");

                    var msg = JsonSerializer.Deserialize<DocumentUploadedMessage>(json, jsonOptions)
                              ?? throw new InvalidOperationException("Ungültige Nachricht");

                    Console.WriteLine($"[OCR] Start für {msg.FileName} (ID: {msg.DocumentId})");

                    var text = await ocrService.ExtractTextAsync(msg);
                    Console.WriteLine($"[OCR] Fertig, {text.Length} Zeichen erkannt!");

                    // Direkt in Elasticsearch indexieren
                    await elasticService.IndexDocumentAsync(msg.DocumentId, msg.FileName, msg.FileName, text);
                    Console.WriteLine($"[ES] Dokument {msg.DocumentId} erfolgreich verarbeitet.");

                    var extractedPayload = new
                    {
                        DocumentId = msg.DocumentId,
                        FileId = msg.FileId,
                        FileName = msg.FileName,
                        ExtractedText = text
                    };

                    var extractedJson = JsonSerializer.Serialize(extractedPayload);
                    var extractedBody = Encoding.UTF8.GetBytes(extractedJson);

                    await textExtractedPublishChannel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: "textExtracted",
                        body: extractedBody
                    );
                    Console.WriteLine($"[MQ] Text für Dokument {msg.DocumentId} an 'textExtracted' gesendet.");

                    await documentUploadChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Fehler bei der Verarbeitung: {ex.Message}");
                    await documentUploadChannel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                }
            };

            textExtractedConsumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<TextExtractedMessage>(json, jsonOptions)
                              ?? throw new InvalidOperationException("Ungültige TextExtracted-Nachricht");

                    Console.WriteLine($"[GEMINI] Starte Summary für {msg.FileName} (ID: {msg.DocumentId})");
                    var summary = await GenAIService.summarizeText(msg.ExtractedText);
                    var category = await GenAIService.categorizeText(msg.ExtractedText);
                    Console.WriteLine($"[GEMINI] Summary und Kategorie für Dokument {msg.DocumentId} erfolgreich erstellt.");
                    var summaryPayload = new
                    {
                        DocumentId = msg.DocumentId,
                        FileId = msg.FileId,
                        Summary = summary,
                        Category = category
                    };

                    var summaryJson = JsonSerializer.Serialize(summaryPayload);
                    var summaryBody = Encoding.UTF8.GetBytes(summaryJson);

                    await summaryGeneratedPublishChannel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: "summaryGenerated",
                        body: summaryBody
                    );
                    Console.WriteLine($"[MQ] Summary für Dokument {msg.DocumentId} an 'summaryGenerated' gesendet.");
                    await textExtractedReceiveChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Fehler bei Gemini-Summary: {ex.Message}");
                    await textExtractedReceiveChannel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                }
            };

            await documentUploadChannel.BasicConsumeAsync(queue: "documentUploaded", autoAck: false, consumer: documentUploadConsumer);
            await textExtractedReceiveChannel.BasicConsumeAsync(queue: "textExtracted", autoAck: false, consumer: textExtractedConsumer);

            Console.WriteLine(" [*] waiting for messages...");

            await Task.Delay(-1);
        }
    }

    public record TextExtractedMessage(int DocumentId, int FileId, string FileName, string ExtractedText);
}