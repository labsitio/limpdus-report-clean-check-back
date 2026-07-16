using System.Security.Claims;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Responses;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<Result> ListFranqueadosAsync(CancellationToken cancellationToken = default);
        Task<Result> SetAdminAsync(ClaimsPrincipal actor, int franqId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
