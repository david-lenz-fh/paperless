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

// mini initialisieren
var minioClient = new MinioClient()
    .WithEndpoint("minio:9000") // minio container in docker
    .WithCredentials("admin", "admin")
    .WithSSL(false)
    .Build();

// client an ocr übergebn
var ocrService = new OcrService(minioClient);

consumer.ReceivedAsync += async (_, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var msg = JsonSerializer.Deserialize<DocumentUploadedMessage>(json)
                  ?? throw new InvalidOperationException("Ungültige Nachricht");

        Console.WriteLine($"[OCR] Start für {msg.FileName} ({msg.DocumentId})");
        // ocr ergebnis wird in "text" variable gespeichert
        var text = await ocrService.ExtractTextAsync(msg);
        Console.WriteLine($"[OCR] Fertig, {text.Length} Zeichen erkannt! Extrahierter text: {text}");

        // Ergebnis an die nächste Queue (Indexing-Worker) weitergeben
        var result = JsonSerializer.SerializeToUtf8Bytes(new { msg.DocumentId, msg.FileName, Text = text });
        await channel.BasicPublishAsync("", "ocr_result_q", result);

        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[OCR][ERROR] {ex.Message}");
        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
    }
};

await channel.BasicConsumeAsync(queue: "test_q", autoAck: false, consumer: consumer);

Console.WriteLine(" [*] waiting for messages...");

// keeps it running
await Task.Delay(-1);