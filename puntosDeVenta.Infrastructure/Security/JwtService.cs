using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using puntosDeVenta.Application.DTOs.Auth;

namespace puntosDeVenta.Shared.Helper
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera JWT para autenticación de POS
        /// Incluye: role, pos_id, pos_name
        /// </summary>
        public TokenResponseDTO GeneratePosToken(GeneratePosTokenDTO request)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "pos_client"),
                new Claim("pos_id", request.pos_id),
                new Claim("pos_name", request.role ?? $"Punto de Venta {request.pos_id}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]);
            var expirationTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expirationTime,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenResponseDTO
            {
                Token = tokenString,
                PosId = request.pos_id,
                PosName = request.role,
                ExpiresAt = expirationTime,
                ExpiresInSeconds = expirationTime.ToString()
            };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public int GetAccessTokenExpirationSeconds()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryMinutes = double.Parse(jwtSettings["ExpiryInMinutes"] ?? "480");
            return (int)(expiryMinutes * 60);
        }

        public ClaimsPrincipal? ValidateTokenAndGetClaims(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public int GetUserIdFromJwt(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new SecurityTokenException("Invalid token");

            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
                throw new SecurityTokenException("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }

        public DateTime? GetExpirationTime(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

                if (expClaim != null)
                {
                    var expirationTimeUnix = long.Parse(expClaim.Value);
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnix).UtcDateTime;
                    return TimeZoneHelper.ConvertToColombiaTime(expirationTime);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Error al leer el token JWT: " + ex.Message);
            }
        }
    }
}
