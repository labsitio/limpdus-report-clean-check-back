using LimpidusMongoDB.Application.Contracts.Responses;
using System.Security.Claims;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public class AuthTokenPayload
    {
        public string Role { get; set; } = string.Empty;
        public bool IsFranqueado { get; set; }
        public int? FranqId { get; set; }
        public int? LegacyProjectId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public IReadOnlyList<int> AllowedProjectIds { get; set; } = Array.Empty<int>();
    }

    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAtUtc) CreateToken(AuthTokenPayload payload);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
