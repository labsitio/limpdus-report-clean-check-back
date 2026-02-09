using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    /// <summary>
    /// Abstração para acesso aos dados do SQL Server
    /// Permite testabilidade e segue o princípio de inversão de dependência (SOLID)
    /// </summary>
    public interface ISqlServerDataAccess
    {
        Task<List<SqlServerAreaEntity>> GetAreasAsync(int projectId, CancellationToken cancellationToken = default);
        Task<List<SqlServerTarefaEntity>> GetTarefasAsync(int projectId, CancellationToken cancellationToken = default);
        Task<SqlServerProjectEntity?> GetProjectAsync(int projectId, CancellationToken cancellationToken = default);
        Task<List<SqlServerEmployeeEntity>> GetEmployeesAsync(int projectId, CancellationToken cancellationToken = default);

        /// <summary>Project name (WORK_HEADER.NOMEPROJETO) for complaint.</summary>
        Task<string?> GetNomeProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

        /// <summary>Complaint email list (WORK_HEADER.MAIL), semicolon-separated.</summary>
        Task<string?> GetComplaintMailAsync(int projetoId, CancellationToken cancellationToken = default);

        /// <summary>Area name (WORK_AREA.AREA) by WORK_AREA_ID.</summary>
        Task<string?> GetNomeAreaAsync(int areaId, CancellationToken cancellationToken = default);
    }
}
