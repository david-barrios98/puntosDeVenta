using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Outbound;

namespace puntosDeVenta.Infrastructure.Messaging
{
    /// <summary>
    /// Implementaci�n del puerto IMessagePublisher usando RabbitMQ
    /// Publica eventos de venta a la cola de integraci�n
    /// </summary>
    public class RabbitMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly string _exchangeName;
        private readonly string _queueName = "sales_integration_queue";

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _exchangeName = configuration.GetSection("RabbitMq")["ExchangeName"] ?? "sales_exchange";

            var rabbitMqConfig = configuration.GetSection("RabbitMq");
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig["HostName"] ?? "localhost",
                Port = int.TryParse(rabbitMqConfig["Port"], out var port) ? port : 5672,
                UserName = rabbitMqConfig["UserName"] ?? "guest",
                Password = rabbitMqConfig["Password"] ?? "guest",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo conectar a RabbitMQ. Verifica que el servidor est� disponible.", ex);
            }
        }

        /// <summary>
        /// Publica evento de venta registrada en la cola
        /// </summary>
        public async Task PublishSaleRegisteredAsync(SaleRegisteredEventDTO @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            try
            {
                using var channel = _connection.CreateModel();

                // Declarar exchange y cola (idempotente)
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
                channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(_queueName, _exchangeName, "sale.registered");

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.ContentType = "application/json";
                properties.Persistent = true;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                channel.BasicPublish(_exchangeName, "sale.registered", properties, body);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al publicar evento en RabbitMQ", ex);
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}