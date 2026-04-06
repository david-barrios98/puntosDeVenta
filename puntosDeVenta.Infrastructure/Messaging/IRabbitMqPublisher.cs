using RabbitMQ.Client;
using System;
using System.Text;

namespace puntosDeVenta.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishSaleRegisteredAsync(object message, string queueName = "sales_integration_queue");
    }

    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(string amqpUrl)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(amqpUrl) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "sales_integration_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public Task PublishSaleRegisteredAsync(object message, string queueName = "sales_integration_queue")
        {
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
        }
    }
}