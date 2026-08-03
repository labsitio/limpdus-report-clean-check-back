using System.Security.Claims;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class ProjectAccessService : IProjectAccessService
    {
        public bool CanAccessLegacyProject(ClaimsPrincipal user, int legacyProjectId)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            if (IsAdmin(user))
                return true;

            if (IsProjectViewer(user))
            {
                var projectId = GetLegacyProjectId(user);
                return projectId.HasValue && projectId.Value == legacyProjectId;
            }

            // Franqueado e Consultor: apenas allowedProjectIds (sem bypass Admin).
            var allowed = GetAllowedProjectIds(user);
            return allowed.Contains(legacyProjectId);
        }

        public bool CanExport(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return IsAdmin(user) || IsFranqueado(user) || IsConsultor(user);
        }

        public bool CanSeeSensitiveHistory(ClaimsPrincipal user) => CanExport(user);

        /// <summary>
        /// Franqueado ou Consultor (login FRANQ_LOGIN com relatório completo: todos os status).
        /// </summary>
        public bool IsFranqueado(ClaimsPrincipal user)
        {
            var role = GetRole(user);
            return string.Equals(role, AuthRoles.Franqueado, StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, AuthRoles.Consultor, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsConsultor(ClaimsPrincipal user) =>
            string.Equals(GetRole(user), AuthRoles.Consultor, StringComparison.OrdinalIgnoreCase);

        public string? GetRole(ClaimsPrincipal user) =>
            user?.FindFirst(ClaimTypes.Role)?.Value
            ?? user?.FindFirst("role")?.Value;

        public int? GetLegacyProjectId(ClaimsPrincipal user)
        {
            var raw = user?.FindFirst(AuthClaims.LegacyProjectId)?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }

        public IReadOnlyList<int> GetAllowedProjectIds(ClaimsPrincipal user)
        {
            var raw = user?.FindFirst(AuthClaims.AllowedProjectIds)?.Value;
            if (string.IsNullOrWhiteSpace(raw))
                return Array.Empty<int>();

            return raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToArray();
        }

        public bool IsAdmin(ClaimsPrincipal user) =>
            string.Equals(GetRole(user), AuthRoles.Admin, StringComparison.OrdinalIgnoreCase);

        public bool IsProjectViewer(ClaimsPrincipal user) =>
            string.Equals(GetRole(user), AuthRoles.ProjectViewer, StringComparison.OrdinalIgnoreCase);
    }
}
