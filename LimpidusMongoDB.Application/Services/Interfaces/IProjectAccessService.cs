using System.Security.Claims;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IProjectAccessService
    {
        bool CanAccessLegacyProject(ClaimsPrincipal user, int legacyProjectId);
        bool CanExport(ClaimsPrincipal user);
        bool CanSeeSensitiveHistory(ClaimsPrincipal user);
        bool IsFranqueado(ClaimsPrincipal user);
        string? GetRole(ClaimsPrincipal user);
        int? GetLegacyProjectId(ClaimsPrincipal user);
        IReadOnlyList<int> GetAllowedProjectIds(ClaimsPrincipal user);
        bool IsAdmin(ClaimsPrincipal user);
        bool IsProjectViewer(ClaimsPrincipal user);
    }
}
