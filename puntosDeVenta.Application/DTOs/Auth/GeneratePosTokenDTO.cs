namespace puntosDeVenta.Application.DTOs.Auth
{
    /// <summary>
    /// DTO para generar token JWT para puntos de venta
    /// </summary>
    public class GeneratePosTokenDTO
    {
        /// <summary>
        /// ID del punto de venta (ej: POS-015)
        /// </summary>
        public string PosId { get; set; }

        /// <summary>
        /// Nombre del punto de venta
        /// </summary>
        public string PosName { get; set; }

        /// <summary>
        /// Duraci�n del token en minutos
        /// </summary>
        public int ExpirationMinutes { get; set; } = 480; // 8 horas
    }

    /// <summary>
    /// Respuesta con el token generado
    /// </summary>
    public class TokenResponseDTO
    {
        public string Token { get; set; }
        public string PosId { get; set; }
        public string PosName { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiresInSeconds { get; set; }
    }
}