using puntosDeVenta.Application.DTOs.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace puntosDeVenta.Application.Ports.Inbound
{
    public interface IGetCriticalInventoryUseCase
    {
        Task<IEnumerable<ProductoCriticalDTO>> HandleAsync(int salesPointId);
    }
}