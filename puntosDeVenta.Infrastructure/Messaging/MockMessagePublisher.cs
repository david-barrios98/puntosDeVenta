using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Outbound;

namespace puntosDeVenta.Infrastructure.Messaging
{
    /// <summary>
    /// Implementaci�n mock de IMessagePublisher para desarrollo/testing
    /// Simula la publicaci�n sin conectar a RabbitMQ real
    /// </summary>
    public class MockMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<MockMessagePublisher> _logger;

        public MockMessagePublisher(ILogger<MockMessagePublisher> logger)
        {
            _logger = logger;
        }

        public async Task PublishSaleRegisteredAsync(SaleRegisteredEventDTO @event)
        {
            try
            {
                _logger.LogInformation("?? [MOCK] Evento publicado: {EventType} - Sale ID: {SaleId}", 
                    @event.EventType, @event.InternalSaleId);

                _logger.LogInformation("?? Detalles del evento: PosId={PosId}, Amount={TotalAmount}, Time={ProcessedAt}",
                    @event.PosId, @event.TotalAmount, @event.ProcessedAt);

                // Simular un peque�o delay como si enviara a la cola
                await Task.Delay(100);

                _logger.LogInformation("? [MOCK] Evento procesado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error en mock publisher");
                throw;
            }
        }
    }
}