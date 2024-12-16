using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IAreaActivityService
    {
        Task<Result> GetByProjectIdAsync(int legacyProjectId);

        Task<Result> GetByProjectIdAndEmployeeIdAsync(int legacyProjectId, string employeeId, CancellationToken cancellationToken = default);

        Task<Result> GetItemsByAreaIsAsync(string areaId, CancellationToken cancellationToken = default);

        Task<Result> SaveAsync(IEnumerable<AreaActivityRequest> request, CancellationToken cancellationToken = default);
    }
}
