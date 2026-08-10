using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// O Mongo pode ter vários <see cref="ProjectEntity"/> com o mesmo <c>LegacyId</c>
    /// (backups / versões N1 vs N2/N3). Preferimos o de maior Level (e mais recente).
    /// </summary>
    public static class ProjectLegacyResolver
    {
        public static ProjectEntity? PreferCanonical(IEnumerable<ProjectEntity>? projects)
        {
            if (projects == null)
                return null;

            return projects
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.UpdateDate ?? p.CreatedDate)
                .ThenBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        public static IEnumerable<ProjectEntity> DeduplicateByLegacyId(IEnumerable<ProjectEntity> projects) =>
            projects
                .GroupBy(p => p.LegacyId)
                .Select(g => PreferCanonical(g)!);
    }
}
