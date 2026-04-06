using puntosDeVenta.Application.DTOs.Sales;
using System.Threading.Tasks;

namespace puntosDeVenta.Application.Ports.Inbound
{
    /// <summary>
    /// Puerto de entrada: Caso de uso para registrar una venta
    /// Implementado por: RegisterSaleUseCase (Application.UseCases)
    /// </summary>
    public interface IRegisterSaleUseCase
    {
        /// <summary>
        /// Ejecuta el proceso completo de registro de venta:
        /// 1. Valida datos y c�lculos
        /// 2. Persiste en BD transaccionalmente
        /// 3. Publica evento asincronamente
        /// </summary>
        /// <param name="sale">DTO con datos de la venta</param>
        /// <returns>Evento generado con ID de venta</returns>
        Task<SaleRegisteredEventDTO> ExecuteAsync(RegisterSaleDTO sale);
    }
}