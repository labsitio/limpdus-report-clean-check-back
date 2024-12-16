using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<Result> SendReportAsync(ReportRequest request, CancellationToken cancellationToken = default);
    }
}
