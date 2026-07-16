using LimpidusMongoDB.Application.Contracts.Responses;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public class FranqueadoLoginEntity
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }

    public class ProjectLoginEntity
    {
        public int WorkHeaderId { get; set; }
        public string NomeProjeto { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
    }

    public class FranqueadoUserEntity
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    public interface ISqlAuthDataAccess
    {
        Task<FranqueadoLoginEntity?> ValidateFranqueadoAsync(string login, string passwordMd5Hex, CancellationToken cancellationToken = default);
        Task<bool> IsAdminAsync(int franqId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AllowedProjectResponse>> GetFranqueadoProjectsAsync(int franqId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AllowedProjectResponse>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
        Task<ProjectLoginEntity?> ValidateProjectLoginAsync(string login, string password, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FranqueadoUserEntity>> ListFranqueadosAsync(CancellationToken cancellationToken = default);
        Task<bool> FranqueadoExistsAsync(int franqId, CancellationToken cancellationToken = default);
        Task SetAdminAsync(int franqId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
