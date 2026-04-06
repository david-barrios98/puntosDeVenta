using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace puntosDeVenta.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher : IDisposable
    {
        Task PublishAsync(string queueName, object message);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(string amqpUrl)
        {
            var factory = new ConnectionFactory { Uri = new Uri(amqpUrl) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "sales_integration_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public Task PublishAsync(string queueName, object message)
        {
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            var props = _channel.CreateBasicProperties();
            props.DeliveryMode = 2;
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
        }
    }
}