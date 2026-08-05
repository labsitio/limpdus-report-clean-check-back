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
            SELECT WORK_HEADER_ID AS Id, NOMEPROJETO AS Name, ISNULL(NIVEL_PROJETO, 0) AS Level
            FROM WORK_HEADER WITH(NOLOCK)
            WHERE ID_DONO = @franqId
            UNION
            SELECT wh.WORK_HEADER_ID AS Id, wh.NOMEPROJETO AS Name, ISNULL(wh.NIVEL_PROJETO, 0) AS Level
            FROM WORK_HEADER_SHARE sh WITH(NOLOCK)
            INNER JOIN WORK_HEADER wh WITH(NOLOCK) ON wh.WORK_HEADER_ID = sh.WORK_HEADER_ID
            WHERE sh.FRANQ_LOGIN = @franqId
            ORDER BY Name";

        private const string QueryHierarchyContext = @"
            SELECT
                fl.ID AS FranqId,
                fl.TBL_NIVEIS_GRUPO_ID AS GrupoId,
                CAST(CASE WHEN ISNULL(fl.VER_NIVEL, 0) = 1 THEN 1 ELSE 0 END AS BIT) AS VerNivel,
                CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM TBL_NIVEIS_GRUPO child WITH(NOLOCK)
                    WHERE child.FATHER_ID = fl.TBL_NIVEIS_GRUPO_ID
                ) THEN 1 ELSE 0 END AS BIT) AS HasChildren
            FROM FRANQ_LOGIN fl WITH(NOLOCK)
            WHERE fl.ID = @franqId";

        /// <summary>
        /// Carteira consultor (LimpCalc Niveis.Children + Project.List):
        /// nós descendentes do TBL_NIVEIS_GRUPO do usuário; se VER_NIVEL, inclui o próprio nó;
        /// donos via VIEW_FRANQ_NIVEIS filtrados por regiões do consultor; + ID_DONO próprio + share.
        /// </summary>
        private const string QueryConsultorProjects = @"
            ;WITH UserCtx AS (
                SELECT
                    fl.ID AS FranqId,
                    fl.TBL_NIVEIS_GRUPO_ID AS GrupoId,
                    CAST(CASE WHEN ISNULL(fl.VER_NIVEL, 0) = 1 THEN 1 ELSE 0 END AS BIT) AS VerNivel
                FROM FRANQ_LOGIN fl WITH(NOLOCK)
                WHERE fl.ID = @franqId
            ),
            Descendants AS (
                SELECT g.TBL_NIVEIS_GRUPO_ID
                FROM TBL_NIVEIS_GRUPO g WITH(NOLOCK)
                INNER JOIN UserCtx u ON g.FATHER_ID = u.GrupoId
                WHERE u.GrupoId IS NOT NULL

                UNION ALL

                SELECT c.TBL_NIVEIS_GRUPO_ID
                FROM TBL_NIVEIS_GRUPO c WITH(NOLOCK)
                INNER JOIN Descendants d ON c.FATHER_ID = d.TBL_NIVEIS_GRUPO_ID
            ),
            Scope AS (
                SELECT TBL_NIVEIS_GRUPO_ID FROM Descendants
                UNION
                SELECT u.GrupoId
                FROM UserCtx u
                WHERE u.VerNivel = 1 AND u.GrupoId IS NOT NULL
            ),
            UserRegions AS (
                SELECT fr.TBL_REGIOES_ID
                FROM FRANQ_REGIOES fr WITH(NOLOCK)
                WHERE fr.FRANQ_ID = @franqId
            ),
            Owners AS (
                SELECT DISTINCT v.FRANQ_LOGIN_ID AS OwnerId
                FROM VIEW_FRANQ_NIVEIS v WITH(NOLOCK)
                INNER JOIN Scope s ON v.TBL_NIVEIS_GRUPO_ID = s.TBL_NIVEIS_GRUPO_ID
                WHERE ISNULL(v.ATIVO, 0) = 1
                  AND (
                      NOT EXISTS (SELECT 1 FROM UserRegions)
                      OR v.TBL_REGIOES_ID IN (SELECT TBL_REGIOES_ID FROM UserRegions)
                  )
                UNION
                SELECT @franqId
            )
            SELECT DISTINCT wh.WORK_HEADER_ID AS Id, wh.NOMEPROJETO AS Name, ISNULL(wh.NIVEL_PROJETO, 0) AS Level
            FROM WORK_HEADER wh WITH(NOLOCK)
            INNER JOIN Owners o ON wh.ID_DONO = o.OwnerId
            UNION
            SELECT wh.WORK_HEADER_ID AS Id, wh.NOMEPROJETO AS Name, ISNULL(wh.NIVEL_PROJETO, 0) AS Level
            FROM WORK_HEADER_SHARE sh WITH(NOLOCK)
            INNER JOIN WORK_HEADER wh WITH(NOLOCK) ON wh.WORK_HEADER_ID = sh.WORK_HEADER_ID
            WHERE sh.FRANQ_LOGIN = @franqId
            ORDER BY Name
            OPTION (MAXRECURSION 100)";

        /// <summary>
        /// Todos os projetos (Admin). Preferência de ordenação: N3 (NIVEL_PROJETO=3), depois nome.
        /// </summary>
        private const string QueryAllProjects = @"
            SELECT WORK_HEADER_ID AS Id, NOMEPROJETO AS Name, ISNULL(NIVEL_PROJETO, 0) AS Level
            FROM WORK_HEADER WITH(NOLOCK)
            ORDER BY
                CASE WHEN ISNULL(NIVEL_PROJETO, 0) = 3 THEN 0 ELSE 1 END,
                NOMEPROJETO";

        private const string QueryProjectLogin = @"
            SELECT TOP 1 WORK_HEADER_ID, NOMEPROJETO, LOGIN, ISNULL(NIVEL_PROJETO, 0) AS NIVEL_PROJETO
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

        public async Task<FranqueadoHierarchyContext> GetHierarchyContextAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryHierarchyContext, connection);
            command.Parameters.AddWithValue("@franqId", franqId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return new FranqueadoHierarchyContext { FranqId = franqId };
            }

            var grupoOrdinal = reader.GetOrdinal("GrupoId");
            return new FranqueadoHierarchyContext
            {
                FranqId = reader.GetInt32(reader.GetOrdinal("FranqId")),
                GrupoId = reader.IsDBNull(grupoOrdinal) ? null : Convert.ToInt32(reader.GetValue(grupoOrdinal)),
                VerNivel = Convert.ToBoolean(reader["VerNivel"]),
                HasChildren = Convert.ToBoolean(reader["HasChildren"])
            };
        }

        public async Task<IReadOnlyList<AllowedProjectResponse>> GetFranqueadoProjectsAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();
            return await ReadProjectsAsync(QueryFranqueadoProjects, cmd =>
            {
                cmd.Parameters.AddWithValue("@franqId", franqId);
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<AllowedProjectResponse>> GetConsultorProjectsAsync(int franqId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();
            return await ReadProjectsAsync(QueryConsultorProjects, cmd =>
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
                var levelOrdinal = reader.GetOrdinal("Level");
                projects.Add(new AllowedProjectResponse
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"]?.ToString() ?? string.Empty,
                    Level = reader.IsDBNull(levelOrdinal) ? 0 : Convert.ToInt32(reader.GetValue(levelOrdinal))
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
                Login = reader["LOGIN"]?.ToString() ?? string.Empty,
                NivelProjeto = Convert.ToInt32(reader["NIVEL_PROJETO"])
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
