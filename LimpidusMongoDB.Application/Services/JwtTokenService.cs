using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Services.Interfaces;
using LimpidusMongoDB.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LimpidusMongoDB.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public (string Token, DateTime ExpiresAtUtc) CreateToken(AuthTokenPayload payload)
        {
            if (string.IsNullOrWhiteSpace(_settings.Key) || _settings.Key.Length < 32)
                throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 caracteres.");

            var expires = DateTime.UtcNow.AddHours(_settings.ExpirationHours > 0 ? _settings.ExpirationHours : 12);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, payload.Role),
                new(AuthClaims.IsFranqueado, payload.IsFranqueado ? "true" : "false"),
                new(AuthClaims.DisplayName, payload.DisplayName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            if (payload.FranqId.HasValue)
            {
                claims.Add(new Claim(AuthClaims.FranqId, payload.FranqId.Value.ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, $"franq:{payload.FranqId.Value}"));
            }

            if (payload.LegacyProjectId.HasValue)
            {
                claims.Add(new Claim(AuthClaims.LegacyProjectId, payload.LegacyProjectId.Value.ToString()));
                if (!payload.FranqId.HasValue)
                    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, $"project:{payload.LegacyProjectId.Value}"));
            }

            if (payload.AllowedProjectIds.Count > 0)
                claims.Add(new Claim(AuthClaims.AllowedProjectIds, string.Join(",", payload.AllowedProjectIds)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_settings.Key))
                return null;

            var handler = new JwtSecurityTokenHandler();
            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = _settings.Issuer,
                    ValidAudience = _settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
