using puntosDeVenta.Application.DTOs.Sales;
using System.Threading.Tasks;

namespace puntosDeVenta.Application.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida: Contrato para persistencia de ventas en BD
    /// Implementado por: SalesRepositoryAdapter (Infrastructure.Persistence.Adapters)
    /// </summary>
    public interface ISalesRepositoryAdapter
    {
        /// <summary>
        /// Registra una venta completa (cabecera + detalles) de forma transaccional
        /// </summary>
        /// <param name="sale">Datos de la venta a registrar</param>
        /// <returns>ID �nico de la cabecera de venta generado por BD</returns>
        Task<int> RegisterSaleAsync(RegisterSaleDTO sale);
    }
}