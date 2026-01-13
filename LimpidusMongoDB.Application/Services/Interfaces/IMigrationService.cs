using LimpidusMongoDB.Application.Contracts;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IMigrationService
    {
        /// <summary>
        /// Migra dados do SQL Server (sistema WebForms) para o banco limpidus
        /// </summary>
        /// <param name="legacyProjectId">ID do projeto no sistema legado (WORK_HEADER_ID)</param>
        /// <param name="sqlServerConnectionString">String de conexão do SQL Server</param>
        /// <param name="cancellationToken"></param>
        Task<Result> MigrateFromSqlServerAsync(
            int legacyProjectId, 
            string sqlServerConnectionString,
            CancellationToken cancellationToken = default);
    }
}
