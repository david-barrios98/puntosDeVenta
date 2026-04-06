using Microsoft.AspNetCore.Mvc;
using puntosDeVenta.Application.DTOs.Auth;
using puntosDeVenta.Application.Ports.Outbound;
using puntosDeVenta.Shared.Helper;
using System;
using System.Threading.Tasks;

namespace puntosDeVenta.Api.Controllers.v1.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IInventoryRepository _inventoryRepository;

        public AuthController(JwtService jwtService, IInventoryRepository inventoryRepository)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        }

        /// <summary>
        /// Genera un token JWT para un punto de venta (Testing/Development)
        /// ⚠️ En producción, esto debe estar protegido y requiere autenticación previa
        /// </summary>
        /// <param name="request">Datos del POS (ej: POS-015)</param>
        /// <returns>Token JWT válido para autenticarse en endpoints protegidos</returns>
        /// <response code="200">Token generado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("token")]
        [ProducesResponseType(typeof(TokenResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GeneratePosToken([FromBody] GeneratePosTokenDTO request)
        {
            try
            {
                // Validación básica
                if (string.IsNullOrWhiteSpace(request?.pos_id))
                    return BadRequest(new { error = "Pos_id es requerido" });

                // Intentar obtener un id numérico. Permite formatos como "POS-015" extrayendo dígitos.
                var digits = new string((request.pos_id ?? string.Empty).Where(char.IsDigit).ToArray());
                if (string.IsNullOrWhiteSpace(digits) || !int.TryParse(digits, out var posId))
                    return BadRequest(new { error = "Pos_id inválido. Debe contener un identificador numérico." });

                // Verificar existencia del POS
                var exists = await _inventoryRepository.PosExistsAsync(posId);
                if (!exists)
                    return BadRequest(new { error = $"Pos_id '{request.pos_id}' no existe" });

                // Generar token
                var response = _jwtService.GeneratePosToken(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO para validar un token
    /// </summary>
    public class ValidateTokenRequestDTO
    {
        public string Token { get; set; }
    }
}