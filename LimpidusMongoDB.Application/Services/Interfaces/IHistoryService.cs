using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IHistoryService
    {
        Task<Result> GetByProjectIdAndEmployeeIdAsync(int legacyProjectId, string employeeId, CancellationToken cancellationToken = default);
        Task<Result> GetByProjectIdAsync(int legacyProjectId, HistoryQueryRequest query,CancellationToken cancellationToken = default);
        Task<Result> GetHistoriesInSpreadsheet(int legacyProjectId, HistoryQueryRequest query,CancellationToken cancellationToken = default);
        Task<Result> SaveAsync(IEnumerable<HistoryRequest> requests, CancellationToken cancellationToken = default);
    }
}