using System.Collections.Generic;
using System.Threading.Tasks;
using puntosDeVenta.Application.DTOs.Inventory;
using puntosDeVenta.Application.Ports.Inbound;
using puntosDeVenta.Application.Ports.Outbound;

namespace puntosDeVenta.Application.UseCases
{
    public class GetCriticalInventoryUseCase : IGetCriticalInventoryUseCase
    {
        private readonly IInventoryRepository _inventoryRepository;

        public GetCriticalInventoryUseCase(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<ProductoCriticalDTO>> HandleAsync(int salesPointId)
        {
            return await _inventoryRepository.GetCriticalProductsAsync(salesPointId);
        }
    }
}