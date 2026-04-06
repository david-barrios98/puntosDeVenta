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
        public string pos_id { get; set; }

        /// <summary>
        /// Nombre del punto de venta
        /// </summary>
        public string role { get; set; }
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
        public String ExpiresInSeconds { get; set; }
    }
}