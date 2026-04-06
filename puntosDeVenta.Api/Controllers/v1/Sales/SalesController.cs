using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Inbound;

namespace puntosDeVenta.Api.Controllers.v1.Sales
{
    [ApiController]
    [Route("api/v1/sales")]
    public class SalesController : ControllerBase
    {
        private readonly IRegisterSaleUseCase _registerSaleUseCase;
        private readonly ILogger<SalesController> _logger;

        public SalesController(IRegisterSaleUseCase registerSaleUseCase, ILogger<SalesController> logger)
        {
            _registerSaleUseCase = registerSaleUseCase ?? throw new ArgumentNullException(nameof(registerSaleUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        [Authorize(Roles = "pos_client")]
        public async Task<IActionResult> RegisterSale([FromBody] RegisterSaleRequestDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { error = "Payload inválido" });

                if (string.IsNullOrWhiteSpace(request.pos_id) || string.IsNullOrWhiteSpace(request.cashier_id))
                    return BadRequest(new { error = "pos_id and cashier_id are required" });

                // Mapear DTO externo (request) -> DTO de dominio (RegisterSaleDTO)
                var registerDto = new RegisterSaleDTO
                {
                    PosId = request.pos_id,
                    CashierId = request.cashier_id,
                    SaleDate = request.sale_date,
                    TotalAmount = request.total_amount,
                    Items = request.items?.Select(i => new SaleItemDTO
                    {
                        producto_id = i.producto_id,
                        quantity = i.quantity,
                        unit_price = i.unit_price
                    }).ToList() ?? new()
                };

                // Ejecutar caso de uso
                var @event = await _registerSaleUseCase.ExecuteAsync(registerDto).ConfigureAwait(false);

                // Construir respuesta consistente con Fase 2
                var response = new RegisterSaleResponseDTO
                {
                    internal_sale_id = @event.InternalSaleId,
                    processed_at = @event.ProcessedAt
                };

                return Created(string.Empty, response);
            }
            catch (FluentValidation.ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation error registering sale");
                return BadRequest(new { error = "Validation failed", details = vex.Errors, code = "VALIDATION_ERROR" });
            }
            catch (InvalidOperationException ex) when (ex.Message == "TOTAL_MISMATCH")
            {
                return BadRequest(new { error = "Total no coincide con suma de items", code = "TOTAL_MISMATCH" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message, code = "VALIDATION_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering sale");
                return StatusCode(500, new { error = "INTERNAL_ERROR", message = ex.Message });
            }
        }
    }
}