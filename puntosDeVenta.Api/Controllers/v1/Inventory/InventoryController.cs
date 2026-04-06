using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using puntosDeVenta.Application.Ports.Inbound;

namespace puntosDeVenta.Api.Controllers.v1.Inventory
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IGetCriticalInventoryUseCase _getCriticalInventoryUseCase;

        public InventoryController(IGetCriticalInventoryUseCase getCriticalInventoryUseCase)
        {
            _getCriticalInventoryUseCase = getCriticalInventoryUseCase;
        }

        [HttpGet("critical/{pos_id}")]
        public async Task<IActionResult> GetCritical(int pos_id)
        {
            if (pos_id <= 0)
                return BadRequest(new { message = "pos_id invilido" });

            var items = (await _getCriticalInventoryUseCase.HandleAsync(pos_id)).ToList();

            var criticalItems = items.Select(i => new
            {
                producto_id = i.Producto_Id,
                codigo_sku = i.Codigo_Sku,
                nombre = i.Nombre,
                stock_actual = Convert.ToDecimal(i.Stock_Actual),
                stock_minimo_permitido = Convert.ToDecimal(i.Stock_Minimo_Permitido)
            });

            var response = new
            {
                pos_id,
                status = "success",
                timestamp = DateTime.UtcNow.ToString("o"),
                critical_items = criticalItems
            };

            return Ok(response);
        }
    }
}