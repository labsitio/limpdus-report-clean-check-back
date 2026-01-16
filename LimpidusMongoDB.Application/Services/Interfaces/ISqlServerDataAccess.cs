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
    }
}
