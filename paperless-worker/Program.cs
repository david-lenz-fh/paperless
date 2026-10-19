using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    Console.WriteLine($"[OCR WORKER LOG] Nachricht empfangen für Dokument: '{message}'");
    
    await Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue: "test_q", autoAck: true, consumer: consumer);

Console.WriteLine(" [*] waiting for messages...");

// keeps it running
await Task.Delay(-1);