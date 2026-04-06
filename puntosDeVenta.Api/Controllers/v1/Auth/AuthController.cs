using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using puntosDeVenta.Application.DTOs.Auth;
using puntosDeVenta.Shared.Helper;

namespace puntosDeVenta.Api.Controllers.v1.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
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
        public IActionResult GeneratePosToken([FromBody] GeneratePosTokenDTO request)
        {
            try
            {
                // Validación básica
                if (string.IsNullOrWhiteSpace(request?.pos_id))
                    return BadRequest(new { error = "PosId es requerido" });

                // Generar token
                var response = _jwtService.GeneratePosToken(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Valida un token JWT y retorna sus claims
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>Claims del token si es válido</returns>
        [HttpPost("validate-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Token))
                    return BadRequest(new { error = "Token es requerido" });

                var principal = _jwtService.ValidateTokenAndGetClaims(request.Token);

                if (principal == null)
                    return Unauthorized(new { error = "Token inválido o expirado" });

                var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();

                return Ok(new
                {
                    valid = true,
                    claims,
                    expiresAt = _jwtService.GetExpirationTime(request.Token)
                });
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