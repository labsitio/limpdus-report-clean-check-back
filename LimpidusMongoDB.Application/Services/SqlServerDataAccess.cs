using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace LimpidusMongoDB.Application.Services
{
    /// <summary>
    /// Implementação concreta do acesso aos dados do SQL Server
    /// </summary>
    public class SqlServerDataAccess : ISqlServerDataAccess
    {
        private readonly string _connectionString;

        private const string QueryAreas = @"
            SELECT AREA_ID, WORK_AREA_ID, AREA, METROS2, DENSIDADE, WORK_HEADER_ID 
            FROM WORK_AREA WITH(NOLOCK)
            WHERE WORK_HEADER_ID = @projectId 
            ORDER BY WORK_AREA_ID";

        private const string QueryTarefas = @"
            SELECT D.TAREFA_NUMERO AS Tarefa, 
                   D.NOME_TAREFA AS DESCRICAO, 
                   D.PERIODO, 
                   C.FREQUENCIA AS FREQUENCIA_DIAS,
                   FREQ.FREQUENCIA AS FREQUENCIA_NOME,
                   C.ORDEM,
                   B.METROS2, 
                   A.WORK_HEADER_ID, 
                   B.WORK_AREA_ID
            FROM WORK_HEADER A WITH(NOLOCK)
            INNER JOIN WORK_AREA B WITH(NOLOCK)
                ON B.WORK_HEADER_ID = A.WORK_HEADER_ID
            INNER JOIN WORK_TAREFAS C WITH(NOLOCK)
                ON C.WORK_AREA_ID = B.WORK_AREA_ID
            INNER JOIN WORK_TBL_TAREFAS D WITH(NOLOCK)
                ON D.WORK_TBL_TAREFAS_ID = C.WORK_TBL_TAREFAS_ID
            LEFT JOIN WORK_TBL_FREQ FREQ WITH(NOLOCK)
                ON C.FREQUENCIA = FREQ.DIAS
            WHERE A.WORK_HEADER_ID = @projectId
            ORDER BY B.WORK_AREA_ID, C.ORDEM, D.TAREFA_NUMERO";

        private const string QueryProject = @"
            SELECT WORK_HEADER_ID, NOMEPROJETO, TOTALM2, DIASANO, OPCOESCALCULO, FATOR,
                   END1, END2, END3, CONTATO, TELEFONE, CELULAR, DATACAD, NIVEL_PROJETO
            FROM WORK_HEADER WITH(NOLOCK)
            WHERE WORK_HEADER_ID = @projectId";

        private const string QueryEmployees = @"
            SELECT WORK_HEADER_ID, FUNCIONARIO, HORAENTRA, HORASAI, OBS
            FROM WORK_FUNCIONARIO WITH(NOLOCK)
            WHERE WORK_HEADER_ID = @projectId
            ORDER BY FUNCIONARIO";

        private const string QueryNomeProjeto = "SELECT NOMEPROJETO FROM WORK_HEADER WITH(NOLOCK) WHERE WORK_HEADER_ID = @projetoId";
        private const string QueryComplaintMail = "SELECT MAIL FROM WORK_HEADER WITH(NOLOCK) WHERE WORK_HEADER_ID = @projetoId";
        private const string QueryNomeArea = "SELECT AREA FROM WORK_AREA WITH(NOLOCK) WHERE WORK_AREA_ID = @areaId";

        public SqlServerDataAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<List<SqlServerAreaEntity>> GetAreasAsync(int projectId, CancellationToken cancellationToken = default)
        {
            var areas = new List<SqlServerAreaEntity>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryAreas, connection);
            command.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                areas.Add(MapAreaFromReader(reader));
            }

            return areas;
        }

        public async Task<List<SqlServerTarefaEntity>> GetTarefasAsync(int projectId, CancellationToken cancellationToken = default)
        {
            var tarefas = new List<SqlServerTarefaEntity>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryTarefas, connection);
            command.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                tarefas.Add(MapTarefaFromReader(reader));
            }

            return tarefas;
        }

        public async Task<SqlServerProjectEntity?> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryProject, connection);
            command.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return MapProjectFromReader(reader);
            }

            return null;
        }

        public async Task<List<SqlServerEmployeeEntity>> GetEmployeesAsync(int projectId, CancellationToken cancellationToken = default)
        {
            var employees = new List<SqlServerEmployeeEntity>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(QueryEmployees, connection);
            command.Parameters.AddWithValue("@projectId", projectId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                employees.Add(MapEmployeeFromReader(reader));
            }

            return employees;
        }

        public async Task<string?> GetNomeProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(QueryNomeProjeto, connection);
            command.Parameters.AddWithValue("@projetoId", projetoId);
            var value = await command.ExecuteScalarAsync(cancellationToken);
            if (value == null || value == DBNull.Value) return null;
            var s = value.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public async Task<string?> GetComplaintMailAsync(int projetoId, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(QueryComplaintMail, connection);
            command.Parameters.AddWithValue("@projetoId", projetoId);
            var value = await command.ExecuteScalarAsync(cancellationToken);
            if (value == null || value == DBNull.Value) return null;
            var s = value.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public async Task<string?> GetNomeAreaAsync(int areaId, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(QueryNomeArea, connection);
            command.Parameters.AddWithValue("@areaId", areaId);
            var value = await command.ExecuteScalarAsync(cancellationToken);
            if (value == null || value == DBNull.Value) return null;
            var s = value.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static SqlServerAreaEntity MapAreaFromReader(SqlDataReader reader)
        {
            return new SqlServerAreaEntity
            {
                AreaId = GetInt32Value(reader, "AREA_ID"),
                WorkAreaId = GetInt32Value(reader, "WORK_AREA_ID"),
                Area = GetStringValue(reader, "AREA"),
                Metros2 = GetInt32Value(reader, "METROS2"),
                Densidade = GetStringValue(reader, "DENSIDADE"),
                WorkHeaderId = GetInt32Value(reader, "WORK_HEADER_ID")
            };
        }

        private static SqlServerTarefaEntity MapTarefaFromReader(SqlDataReader reader)
        {
            return new SqlServerTarefaEntity
            {
                Tarefa = GetInt32Value(reader, "Tarefa"),
                Descricao = GetStringValue(reader, "DESCRICAO"),
                Periodo = GetStringValue(reader, "PERIODO"),
                FrequenciaDias = GetInt32Value(reader, "FREQUENCIA_DIAS"),
                FrequenciaNome = GetStringValue(reader, "FREQUENCIA_NOME"),
                Ordem = GetInt32Value(reader, "ORDEM"),
                Metros2 = GetInt32Value(reader, "METROS2"),
                WorkHeaderId = GetInt32Value(reader, "WORK_HEADER_ID"),
                WorkAreaId = GetInt32Value(reader, "WORK_AREA_ID")
            };
        }

        private static string GetStringValue(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            // Tenta ler como String primeiro, se falhar tenta converter de outros tipos
            try
            {
                return reader.GetString(ordinal);
            }
            catch (InvalidCastException)
            {
                // Se o valor vier como Int32, Double, etc., converte para String
                var value = reader.GetValue(ordinal);
                return value?.ToString() ?? string.Empty;
            }
        }

        private static int GetInt32Value(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return 0;
            }

            // Tenta ler como Int32 primeiro, se falhar tenta como Double e converte
            try
            {
                return reader.GetInt32(ordinal);
            }
            catch (InvalidCastException)
            {
                // Se o valor vier como Double/Numeric, converte para Int32
                var doubleValue = reader.GetDouble(ordinal);
                return Convert.ToInt32(doubleValue);
            }
        }

        private static double GetFloatValue(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return 0.0;
            }

            // Tenta ler como Double primeiro
            try
            {
                return reader.GetDouble(ordinal);
            }
            catch (InvalidCastException)
            {
                // Se falhar, tenta ler como decimal e converte
                var decimalValue = reader.GetDecimal(ordinal);
                return (double)decimalValue;
            }
        }

        private static SqlServerProjectEntity MapProjectFromReader(SqlDataReader reader)
        {
            return new SqlServerProjectEntity
            {
                WorkHeaderId = GetInt32Value(reader, "WORK_HEADER_ID"),
                NomeProjeto = GetStringValue(reader, "NOMEPROJETO"),
                TotalM2 = GetFloatValue(reader, "TOTALM2"),
                DiasAno = GetInt32Value(reader, "DIASANO"),
                OpcoesCalculo = GetInt32Value(reader, "OPCOESCALCULO"),
                Fator = GetFloatValue(reader, "FATOR"),
                End1 = GetStringValue(reader, "END1"),
                End2 = GetStringValue(reader, "END2"),
                End3 = GetStringValue(reader, "END3"),
                Contato = GetStringValue(reader, "CONTATO"),
                Telefone = GetStringValue(reader, "TELEFONE"),
                Celular = GetStringValue(reader, "CELULAR"),
                DataCad = GetDateTimeValue(reader, "DATACAD"),
                NivelProjeto = GetInt32Value(reader, "NIVEL_PROJETO")
            };
        }

        private static SqlServerEmployeeEntity MapEmployeeFromReader(SqlDataReader reader)
        {
            return new SqlServerEmployeeEntity
            {
                WorkHeaderId = GetInt32Value(reader, "WORK_HEADER_ID"),
                Funcionario = GetInt32Value(reader, "FUNCIONARIO"),
                HoraEntra = GetTimeSpanValue(reader, "HORAENTRA"),
                HoraSai = GetTimeSpanValue(reader, "HORASAI"),
                Obs = GetStringValue(reader, "OBS")
            };
        }

        private static DateTime GetDateTimeValue(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return DateTime.MinValue;
            }

            try
            {
                return reader.GetDateTime(ordinal);
            }
            catch (InvalidCastException)
            {
                // Se falhar, tenta converter de string
                var value = reader.GetValue(ordinal);
                if (value is DateTime dateTime)
                {
                    return dateTime;
                }
                if (DateTime.TryParse(value?.ToString(), out var parsedDate))
                {
                    return parsedDate;
                }
                return DateTime.MinValue;
            }
        }

        private static TimeSpan? GetTimeSpanValue(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                // SQL Server time type pode ser lido como TimeSpan diretamente
                return reader.GetTimeSpan(ordinal);
            }
            catch (InvalidCastException)
            {
                // Se falhar, tenta ler como DateTime e extrair o TimeSpan
                try
                {
                    var dateTime = reader.GetDateTime(ordinal);
                    return dateTime.TimeOfDay;
                }
                catch
                {
                    // Se ainda falhar, tenta converter de string
                    var value = reader.GetValue(ordinal);
                    if (value is TimeSpan timeSpan)
                    {
                        return timeSpan;
                    }
                    if (TimeSpan.TryParse(value?.ToString(), out var parsedTimeSpan))
                    {
                        return parsedTimeSpan;
                    }
                    return null;
                }
            }
        }
    }
}
