using puntosDeVenta.Application.DTOs.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace puntosDeVenta.Application.Ports.Outbound
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<ProductoCriticalDTO>> GetCriticalProductsAsync(int salesPointId);
    }
}