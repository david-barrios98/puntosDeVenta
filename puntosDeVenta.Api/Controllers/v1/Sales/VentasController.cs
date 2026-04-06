using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Inbound;

namespace puntosDeVenta.Api.Controllers.v1.Sales
{
    [ApiController]
    [Route("api/v1/ventas")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VentasController : ControllerBase
    {
        private readonly IRegisterSaleUseCase _registerSaleUseCase;

        public VentasController(IRegisterSaleUseCase registerSaleUseCase)
        {
            _registerSaleUseCase = registerSaleUseCase ?? throw new ArgumentNullException(nameof(registerSaleUseCase));
        }

        /// <summary>
        /// Registra una venta desde un punto de venta
        /// Requiere: JWT con claims pos_id y role=pos_client
        /// </summary>
        /// <param name="request">Datos de la venta</param>
        /// <returns>Evento de venta registrada</returns>
        /// <response code="200">Venta registrada exitosamente</response>
        /// <response code="400">Error de validaci�n</response>
        /// <response code="401">Token inv�lido o expirado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<SaleRegisteredEventDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterSale([FromBody] RegisterSaleDTO request)
        {
            try
            {
                // ============== VALIDACI�N JWT ==============
                var posIdClaim = User.FindFirst("pos_id")?.Value;
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

                // Validar que el JWT tenga los claims requeridos
                if (string.IsNullOrWhiteSpace(posIdClaim))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token inv�lido: falta el claim 'pos_id'",
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (roleClaim != "pos_client")
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Acceso denegado: el token debe tener rol 'pos_client'",
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                // ============== VALIDACI�N DE PAYLOAD ==============
                // Verificar que el PosId del JWT coincida con el del payload
                if (!posIdClaim.Equals(request.PosId, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Error: el PosId del token ({posIdClaim}) no coincide con el del payload ({request.PosId})",
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                // ============== REGISTRAR VENTA ==============
                var result = await _registerSaleUseCase.ExecuteAsync(request);

                return Ok(new ApiResponse<SaleRegisteredEventDTO>
                {
                    Success = true,
                    Message = "Venta registrada exitosamente",
                    Data = result,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error de validaci�n",
                    Data = ex.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => (object)g.Select(e => e.ErrorMessage).ToList()),
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al registrar la venta",
                    Data = ex.Message,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; }
    }
}