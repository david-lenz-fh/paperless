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

// short break to boot up
await Task.Delay(5000);

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// declare a queue - in this case the test q
await channel.QueueDeclareAsync(
    queue: "test_q",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }
);

var consumer = new AsyncEventingBasicConsumer(channel);

// triggered by BasicConcumeAsync, event (output text, later further actions with ocr)
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    // print message
    Console.WriteLine($"[OCR WORKER LOG] Nachricht empfangen für Dokument: '{message}'");
    
    await Task.CompletedTask;
};

// Listens: for messages for the test q and triggers the specified consumer
// there would be more channels if there are more queues, multiple queues could trigger the same or different consumers
await channel.BasicConsumeAsync(queue: "test_q", autoAck: true, consumer: consumer);

Console.WriteLine(" [*] waiting for messages...");

// keeps it running
await Task.Delay(-1);