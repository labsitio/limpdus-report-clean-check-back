using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintDataResponse?> GetDataAsync(int legacyProjectId, int legacyAreaId, CancellationToken cancellationToken = default);

        Task<ComplaintSendResponse> SendAsync(ComplaintSendRequest request, CancellationToken cancellationToken = default);
    }
}
