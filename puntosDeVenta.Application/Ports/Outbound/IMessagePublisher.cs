using puntosDeVenta.Application.DTOs.Sales;
using System.Threading.Tasks;

namespace puntosDeVenta.Application.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida: Contrato para publicar eventos de venta
    /// Implementado por: RabbitMqPublisher (Infrastructure.Messaging)
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publica un evento de venta registrada en la cola de mensajer�a
        /// </summary>
        /// <param name="event">Evento con los datos de la venta registrada</param>
        /// <returns>Tarea asincr�nica completada</returns>
        Task PublishSaleRegisteredAsync(SaleRegisteredEventDTO @event);
    }
}