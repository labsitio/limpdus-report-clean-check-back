using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IProjectService
    {
        public Task<Result> GetAllProjects();

        public Task<Result> GetByLegacyIdAsync(int legacyId);

        public Task<Result> GetByIdAsync(string id);

        public Task<Result> SaveAsync(ProjectRequest request);

        /// <summary>Override persistido (null se projeto inexistente ou sem override).</summary>
        Task<int?> GetMaxHistoryRangeDaysAsync(int legacyId, CancellationToken cancellationToken = default);

        /// <summary>Teto efetivo do ProjectViewer: override ?? 90.</summary>
        Task<int> GetEffectiveProjectViewerMaxDaysAsync(int legacyId, CancellationToken cancellationToken = default);

        Task<Result> SetMaxHistoryRangeDaysAsync(int legacyId, int? maxHistoryRangeDays, CancellationToken cancellationToken = default);

        Task<Result> GetClientAccessAsync(int legacyId, CancellationToken cancellationToken = default);

        Task<Result> SetClientAccessAsync(int legacyId, SetClientAccessRequest request, CancellationToken cancellationToken = default);
    }
}