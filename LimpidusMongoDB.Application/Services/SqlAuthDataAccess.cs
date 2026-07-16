using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LimpidusMongoDB.Application.Services
{
    public class SqlAuthDataAccess : ISqlAuthDataAccess
    {
        private readonly string? _connectionString;
        private readonly ILogger<SqlAuthDataAccess> _logger;

        private const string QueryFranqueado = @"
            SELECT TOP 1 ID, NOME, LOGIN
            FROM FRANQ_LOGIN WITH(NOLOCK)
            WHERE ATIVO = 1 AND LOGIN = @login AND SENHA = @senha";

        private const string QueryIsAdmin = @"
            SELECT TOP 1 1
            FROM GRUPOS_USER WITH(NOLOCK)
            WHERE ID_TBLGRUPOS = 1 AND ID_FRANQ = @franqId";

        private const string QueryFranqueadoProjects = @"
            SELECT WORK_HEADER_ID AS Id, NOMEPROJETO AS Name
            FROM WORK_HEADER WITH(NOLOCK)
            WHERE ID_DONO = @franqId
            UNION
            SELECT wh.WORK_HEADER_ID AS Id, wh.NOMEPROJETO AS Name
            FROM WORK_HEADER_SHARE sh WITH(NOLOCK)
            INNER JOIN WORK_HEADER wh WITH(NOLOCK) ON wh.WORK_HEADER_ID = sh.WORK_HEADER_ID
            WHERE sh.FRANQ_LOGIN = @franqId
            ORDER BY Name";

        /// <summary>
        /// Todos os projetos (Admin). Preferência de ordenação: N3 (NIVEL_PROJETO=3), depois nome.
        /// </summary>
        private const string QueryAllProjects = @"
            SELECT WORK_HEADER_ID AS Id, NOMEPROJETO AS Name
            FROM WORK_HEADER WITH(NOLOCK)
            ORDER BY
                CASE WHEN ISNULL(NIVEL_PROJETO, 0) = 3 THEN 0 ELSE 1 END,
                NOMEPROJETO";

        private const string QueryProjectLogin = @"
            SELECT TOP 1 WORK_HEADER_ID, NOMEPROJETO, LOGIN
            FROM WORK_HEADER WITH(NOLOCK)
            WHERE LOGIN = @login AND SENHA = @senha";

        private const string QueryListFranqueados = @"
            SELECT f.ID, f.NOME, f.LOGIN,
                   CASE WHEN g.ID_FRANQ IS NOT NULL THEN 1 ELSE 0 END AS IsAdmin
            FROM FRANQ_LOGIN f WITH(NOLOCK)
            LEFT JOIN GRUPOS_USER g WITH(NOLOCK)
                ON g.ID_FRANQ = f.ID AND g.ID_TBLGRUPOS = 1
            WHERE f.ATIVO = 1
            ORDER BY f.NOME";

        private const string QueryFranqueadoExists = @"
            SELECT TOP 1 1
            FROM FRANQ_LOGIN WITH(NOLOCK)
            WHERE ATIVO = 1 AND ID = @franqId";

        private const string QueryInsertAdmin = @"
            IF NOT EXISTS (
                SELECT 1 FROM GRUPOS_USER WITH(NOLOCK)
                WHERE ID_FRANQ = @franqId AND ID_TBLGRUPOS = 1
            )
            INSERT INTO GRUPOS_USER (ID_FRANQ, ID_TBLGRUPOS)
            VALUES (@franqId, 1)";

        private const string QueryDeleteAdmin = @"
            DELETE FROM GRUPOS_USER
            WHERE ID_FRANQ = @franqId AND ID_TBLGRUPOS = 1";

        public SqlAuthDataAccess(IConfiguration configuration, ILogger<SqlAuthDataAccess> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDB");
            _logger = logger;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

        public async Task<FranqueadoLoginEntity?> ValidateFranqueadoAsync(string login, string passwordMd5Hex, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryFranqueado, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@senha", passwordMd5Hex);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new FranqueadoLoginEntity
            {
                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                Nome = reader["NOME"]?.ToString() ?? string.Empty,
                Login = reader["LOGIN"]?.ToString() ?? string.Empty
            };
        }

        public async Task<bool> IsAdminAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryIsAdmin, connection);
            command.Parameters.AddWithValue("@franqId", franqId);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null && result != DBNull.Value;
        }

        public async Task<IReadOnlyList<AllowedProjectResponse>> GetFranqueadoProjectsAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();
            return await ReadProjectsAsync(QueryFranqueadoProjects, cmd =>
            {
                cmd.Parameters.AddWithValue("@franqId", franqId);
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<AllowedProjectResponse>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
        {
            EnsureConfigured();
            return await ReadProjectsAsync(QueryAllProjects, _ => { }, cancellationToken);
        }

        private async Task<IReadOnlyList<AllowedProjectResponse>> ReadProjectsAsync(
            string sql,
            Action<SqlCommand> configure,
            CancellationToken cancellationToken)
        {
            var projects = new List<AllowedProjectResponse>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            configure(command);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                projects.Add(new AllowedProjectResponse
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"]?.ToString() ?? string.Empty
                });
            }

            return projects;
        }

        public async Task<ProjectLoginEntity?> ValidateProjectLoginAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryProjectLogin, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@senha", password);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new ProjectLoginEntity
            {
                WorkHeaderId = reader.GetInt32(reader.GetOrdinal("WORK_HEADER_ID")),
                NomeProjeto = reader["NOMEPROJETO"]?.ToString() ?? string.Empty,
                Login = reader["LOGIN"]?.ToString() ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<FranqueadoUserEntity>> ListFranqueadosAsync(CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var users = new List<FranqueadoUserEntity>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryListFranqueados, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                users.Add(new FranqueadoUserEntity
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    Nome = reader["NOME"]?.ToString() ?? string.Empty,
                    Login = reader["LOGIN"]?.ToString() ?? string.Empty,
                    IsAdmin = Convert.ToInt32(reader["IsAdmin"]) == 1
                });
            }

            return users;
        }

        public async Task<bool> FranqueadoExistsAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryFranqueadoExists, connection);
            command.Parameters.AddWithValue("@franqId", franqId);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null && result != DBNull.Value;
        }

        public async Task SetAdminAsync(int franqId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(isAdmin ? QueryInsertAdmin : QueryDeleteAdmin, connection);
            command.Parameters.AddWithValue("@franqId", franqId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _logger.LogError("ConnectionStrings:SqlServerDB não configurada — login SQL indisponível.");
                throw new InvalidOperationException("ConnectionStrings:SqlServerDB não está configurada.");
            }
        }
    }
}
