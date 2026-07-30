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

    /// <summary>
    /// Contexto de hierarquia (TBL_NIVEIS_GRUPO), alinhado a Niveis.Children + Work.Project.List.
    /// </summary>
    public class FranqueadoHierarchyContext
    {
        public int FranqId { get; set; }
        public int? GrupoId { get; set; }
        public bool VerNivel { get; set; }
        /// <summary>True se o nó tem filhos diretos na árvore (carteira de consultor).</summary>
        public bool HasChildren { get; set; }
        public bool IsConsultor => HasChildren;
    }

    public interface ISqlAuthDataAccess
    {
        Task<FranqueadoLoginEntity?> ValidateFranqueadoAsync(string login, string passwordMd5Hex, CancellationToken cancellationToken = default);
        Task<bool> IsAdminAsync(int franqId, CancellationToken cancellationToken = default);
        Task<FranqueadoHierarchyContext> GetHierarchyContextAsync(int franqId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AllowedProjectResponse>> GetFranqueadoProjectsAsync(int franqId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Carteira do consultor: ID_DONO de franqueados nos nós descendentes
        /// (+ próprio nó se VER_NIVEL), filtrado por FRANQ_REGIOES, mais share do usuário.
        /// </summary>
        Task<IReadOnlyList<AllowedProjectResponse>> GetConsultorProjectsAsync(int franqId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AllowedProjectResponse>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
        Task<ProjectLoginEntity?> ValidateProjectLoginAsync(string login, string password, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FranqueadoUserEntity>> ListFranqueadosAsync(CancellationToken cancellationToken = default);
        Task<bool> FranqueadoExistsAsync(int franqId, CancellationToken cancellationToken = default);
        Task SetAdminAsync(int franqId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
