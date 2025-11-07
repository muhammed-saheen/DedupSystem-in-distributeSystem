using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DedublicationOfDistributedSystem.Services
{
    public class WebSocketPublisher
    {
        private readonly IChannel _channel;
        private readonly string _queueName;

        public WebSocketPublisher(IConfiguration _conf)
        {
            var factory = new ConnectionFactory { HostName = _conf["RabbitMQ:Host"] };
            // New async API
            var connection = factory.CreateConnectionAsync().Result;
            _channel = connection.CreateChannelAsync().Result;

            _channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false);
            _queueName = _conf["RabbitMQ:Queue"] ?? "event_queue";
        }

        public void PublishEvent(EventMessage evt)
        {
            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublishAsync("", _queueName, body: body);
            Console.WriteLine($"Published event: {evt.EventId}");
        }

      
    }
}
