using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MySql.Data.MySqlClient;
using Dapper;

namespace DedublicationOfDistributedSystem.Services

{
    public class EventConsumer
    {
        private readonly string _connString;
        private readonly string _queueName;
        private readonly string _hostName;
        private readonly IConfiguration _conf;
        public EventConsumer(IConfiguration _conf)
        {
       
            // Read from appsettings.json
            _connString = _conf.GetConnectionString("MySql")!;
            _queueName = _conf["RabbitMQ:Queue"]!;
            _hostName = _conf["RabbitMQ:Host"]!;
        }

        public async Task StartConsumingAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                
            };

            // Create async connection and channel
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<EventMessage>(message);

                await using var conn = new MySqlConnection(_connString);
                try
                {
                    // Check if event already processed
                    var existing = await conn.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(*) FROM Events WHERE EventId = @EventId AND IsCompleted = TRUE",
                        new { evt!.EventId });

                    if (existing > 0)
                    {
                        Console.WriteLine($"Duplicate event {evt.EventId}, skipping...");
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    // Insert event
                    await conn.ExecuteAsync(
                        "INSERT INTO Events (EventId, Payload, IsCompleted, ProcessedAt) VALUES (@EventId, @Payload, FALSE, NOW())",
                        new { evt.EventId, evt.Payload });

                    // Simulate processing
                    await Task.Delay(1000);

                    // Mark complete
                    await conn.ExecuteAsync(
                        "UPDATE Events SET IsCompleted = TRUE WHERE EventId = @EventId",
                        new { evt.EventId });

                    Console.WriteLine($"Processed event {evt.EventId}");
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (MySqlException ex) when (ex.Message.Contains("Duplicate"))
                {
                    Console.WriteLine($"DB duplicate for {evt!.EventId}");
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {evt!.EventId}: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine($"Consumer started — listening on queue '{_queueName}' at '{_hostName}'");
        }
    }

    public class EventMessage
    {
        public string EventId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
