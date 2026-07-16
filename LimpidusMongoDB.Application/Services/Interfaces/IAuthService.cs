using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
