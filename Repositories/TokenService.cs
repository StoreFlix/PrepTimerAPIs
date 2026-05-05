using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServiceFabricAPIsOld.Repositories
{
    public class TokenService
    {
        public const string PasswordResetPurpose = "password_reset";

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;

        public TokenService(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        public string GenerateAccessToken(string ClientName)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("clientName", ClientName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(1440),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Generates a short-lived JWT used as a one-shot password-reset link token.
        /// The "stamp" claim is bound to the user's current password hash, so once the
        /// password is changed any outstanding reset tokens become invalid (no DB tracking required).
        /// </summary>
        public string GeneratePasswordResetToken(int userId, string email, string passwordHash, int validForMinutes = 30)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
                new Claim("purpose", PasswordResetPurpose),
                new Claim("stamp", BuildStamp(passwordHash))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(validForMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates a password-reset JWT. Returns the principal on success; null on any failure
        /// (signature, expiry, wrong purpose). Caller is still responsible for verifying the
        /// "stamp" claim against the user's current password hash.
        /// </summary>
        public ClaimsPrincipal? ValidatePasswordResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var parameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out _);
                var purpose = principal.FindFirst("purpose")?.Value;
                if (!string.Equals(purpose, PasswordResetPurpose, StringComparison.Ordinal))
                    return null;
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public static string BuildStamp(string? passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash)) return string.Empty;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(passwordHash));
            return Convert.ToBase64String(bytes).Substring(0, 12);
        }
    }
}
