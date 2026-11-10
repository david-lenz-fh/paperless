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

Console.WriteLine("=== [OCR WORKER] gestartet, verbinde mit RabbitMQ... ===");

var factory = new ConnectionFactory
{
    Uri = new Uri("amqp://admin:admin@rabbitmq:5672/")
};

await Task.Delay(5000);

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "test_q",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
);

var consumer = new AsyncEventingBasicConsumer(channel);

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

consumer.ReceivedAsync += async (_, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        Console.WriteLine($"[DEBUG] Nachricht empfangen: {json}");

        var msg = JsonSerializer.Deserialize<DocumentUploadedMessage>(json, jsonOptions)
                  ?? throw new InvalidOperationException("Ungültige Nachricht");

        Console.WriteLine($"[OCR] Start für {msg.FileName} (ID: {msg.DocumentId})");

        var text = await ocrService.ExtractTextAsync(msg);
        Console.WriteLine($"[OCR] Fertig, {text.Length} Zeichen erkannt!");

        // Direkt in Elasticsearch indexieren
        await elasticService.IndexDocumentAsync(msg.DocumentId, msg.FileName, msg.FileName, text);
        Console.WriteLine($"[ES] Dokument {msg.DocumentId} erfolgreich verarbeitet.");

        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Fehler bei der Verarbeitung: {ex.Message}");
        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
    }
};

await channel.BasicConsumeAsync(queue: "test_q", autoAck: false, consumer: consumer);
Console.WriteLine(" [*] waiting for messages...");

await Task.Delay(-1);