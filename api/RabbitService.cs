using RabbitMQ.Client;
using System.Text;

namespace api
{
    public class RabbitService
    {
        public async static void SendToQueue(string message)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://admin:admin@rabbitmq:5672/");

            Console.WriteLine($" [x] Sending {message} ...");

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {message}");
        }
    }
}
