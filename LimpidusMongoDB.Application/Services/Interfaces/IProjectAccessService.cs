using System.Security.Claims;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IProjectAccessService
    {
        bool CanAccessLegacyProject(ClaimsPrincipal user, int legacyProjectId);
        bool CanExport(ClaimsPrincipal user);
        bool CanSeeSensitiveHistory(ClaimsPrincipal user);
        /// <summary>Franqueado ou Consultor (relatório completo).</summary>
        bool IsFranqueado(ClaimsPrincipal user);
        bool IsConsultor(ClaimsPrincipal user);
        string? GetRole(ClaimsPrincipal user);
        int? GetLegacyProjectId(ClaimsPrincipal user);
        IReadOnlyList<int> GetAllowedProjectIds(ClaimsPrincipal user);
        bool IsAdmin(ClaimsPrincipal user);
        bool IsProjectViewer(ClaimsPrincipal user);
    }
}
