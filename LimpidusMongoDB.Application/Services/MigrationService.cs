using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace LimpidusMongoDB.Application.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IAreaActivityService _areaActivityService;

        private const string QueryAreas = @"
            SELECT AREA_ID, WORK_AREA_ID, AREA, METROS2, DENSIDADE, WORK_HEADER_ID 
            FROM WORK_AREA WITH(NOLOCK)
            WHERE WORK_HEADER_ID = @projectId 
            ORDER BY WORK_AREA_ID";

        private const string QueryTarefas = @"
            SELECT D.TAREFA_NUMERO AS Tarefa, 
                   D.NOME_TAREFA AS DESCRICAO, 
                   D.PERIODO, 
                   D.FREQUENCIA, 
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
            WHERE A.WORK_HEADER_ID = @projectId
            ORDER BY B.WORK_AREA_ID, D.TAREFA_NUMERO";

        public MigrationService(IAreaActivityService areaActivityService)
        {
            _areaActivityService = areaActivityService;
        }

        public async Task<Result> MigrateFromSqlServerAsync(
            int legacyProjectId,
            string sqlServerConnectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var areas = await GetAreasFromSqlServerAsync(sqlServerConnectionString, legacyProjectId, cancellationToken);
                if (!areas.Any())
                {
                    return Result.Error($"Nenhuma área encontrada para o projeto {legacyProjectId} no SQL Server.");
                }

                var tarefas = await GetTarefasFromSqlServerAsync(sqlServerConnectionString, legacyProjectId, cancellationToken);
                var tarefasPorWorkAreaId = GroupTarefasByWorkAreaId(tarefas);

                var areaActivityRequests = MapToAreaActivityRequests(areas, tarefasPorWorkAreaId, legacyProjectId);
                if (!areaActivityRequests.Any())
                {
                    return Result.Error("Nenhuma atividade para salvar.");
                }

                await UpdateExistingAreaIdsAsync(areaActivityRequests, legacyProjectId, cancellationToken);

                var saveResult = await _areaActivityService.SaveAsync(areaActivityRequests, cancellationToken);
                if (!saveResult.Success)
                {
                    return Result.Error($"Erro ao salvar atividades: {saveResult.Message}");
                }

                return Result.Ok(data: new
                {
                    areasMigrated = areaActivityRequests.Count,
                    totalItems = areaActivityRequests.Sum(a => a.Items?.Count() ?? 0),
                    projectId = legacyProjectId
                });
            }
            catch (SqlException ex)
            {
                return Result.Error($"Erro de conexão com SQL Server: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result.Error($"Erro durante a migração: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca áreas do projeto no SQL Server
        /// </summary>
        private async Task<List<SqlServerAreaEntity>> GetAreasFromSqlServerAsync(
            string connectionString,
            int projectId,
            CancellationToken cancellationToken)
        {
            var areas = new List<SqlServerAreaEntity>();

            await using var connection = new SqlConnection(connectionString);
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

        /// <summary>
        /// Busca tarefas do projeto no SQL Server
        /// </summary>
        private async Task<List<SqlServerTarefaEntity>> GetTarefasFromSqlServerAsync(
            string connectionString,
            int projectId,
            CancellationToken cancellationToken)
        {
            var tarefas = new List<SqlServerTarefaEntity>();

            await using var connection = new SqlConnection(connectionString);
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

        /// <summary>
        /// Mapeia uma linha do DataReader para SqlServerAreaEntity
        /// </summary>
        private static SqlServerAreaEntity MapAreaFromReader(SqlDataReader reader)
        {
            return new SqlServerAreaEntity
            {
                AreaId = reader.GetInt32(reader.GetOrdinal("AREA_ID")),
                WorkAreaId = reader.GetInt32(reader.GetOrdinal("WORK_AREA_ID")),
                Area = GetStringValue(reader, "AREA"),
                Metros2 = GetInt32Value(reader, "METROS2"),
                Densidade = GetStringValue(reader, "DENSIDADE"),
                WorkHeaderId = reader.GetInt32(reader.GetOrdinal("WORK_HEADER_ID"))
            };
        }

        /// <summary>
        /// Mapeia uma linha do DataReader para SqlServerTarefaEntity
        /// </summary>
        private static SqlServerTarefaEntity MapTarefaFromReader(SqlDataReader reader)
        {
            return new SqlServerTarefaEntity
            {
                Tarefa = GetInt32Value(reader, "Tarefa"),
                Descricao = GetStringValue(reader, "DESCRICAO"),
                Periodo = GetStringValue(reader, "PERIODO"),
                Frequencia = GetStringValue(reader, "FREQUENCIA"),
                Metros2 = GetInt32Value(reader, "METROS2"),
                WorkHeaderId = reader.GetInt32(reader.GetOrdinal("WORK_HEADER_ID")),
                WorkAreaId = reader.GetInt32(reader.GetOrdinal("WORK_AREA_ID"))
            };
        }

        /// <summary>
        /// Obtém valor string do reader, retornando string vazia se for DBNull
        /// </summary>
        private static string GetStringValue(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        /// <summary>
        /// Obtém valor int do reader, retornando 0 se for DBNull
        /// </summary>
        private static int GetInt32Value(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        /// <summary>
        /// Agrupa tarefas por WorkAreaId para facilitar o mapeamento
        /// </summary>
        private static Dictionary<int, List<SqlServerTarefaEntity>> GroupTarefasByWorkAreaId(List<SqlServerTarefaEntity> tarefas)
        {
            return tarefas
                .GroupBy(t => t.WorkAreaId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Converte áreas e tarefas do SQL Server para AreaActivityRequest
        /// </summary>
        private static List<AreaActivityRequest> MapToAreaActivityRequests(
            List<SqlServerAreaEntity> areas,
            Dictionary<int, List<SqlServerTarefaEntity>> tarefasPorWorkAreaId,
            int legacyProjectId)
        {
            var areaActivityRequests = new List<AreaActivityRequest>();
            short orderBy = 1;

            foreach (var area in areas.OrderBy(a => a.Area))
            {
                var tarefasDaArea = tarefasPorWorkAreaId.GetValueOrDefault(area.WorkAreaId, new List<SqlServerTarefaEntity>());
                var items = MapTarefasToItems(tarefasDaArea);

                var areaActivityRequest = new AreaActivityRequest
                {
                    Id = string.Empty,
                    Name = area.Area ?? string.Empty,
                    Description = string.Empty,
                    QuickTask = false,
                    TotalM2 = area.Metros2,
                    EmployeeId = null,
                    HeaderId = area.WorkAreaId.ToString(),
                    OrderBy = orderBy++,
                    Frequency = null,
                    Items = items,
                    ProjectId = legacyProjectId
                };

                areaActivityRequests.Add(areaActivityRequest);
            }

            return areaActivityRequests;
        }

        /// <summary>
        /// Converte tarefas do SQL Server para AreaActivityItemRequest
        /// </summary>
        private static List<AreaActivityItemRequest> MapTarefasToItems(List<SqlServerTarefaEntity> tarefas)
        {
            return tarefas
                .OrderBy(t => t.Tarefa)
                .Select((tarefa, index) => new AreaActivityItemRequest
                {
                    Id = tarefa.Tarefa.ToString(),
                    Name = tarefa.Descricao ?? string.Empty,
                    OrderBy = (short)(index + 1),
                    Frequency = new AreaActivityFrequency
                    {
                        Type = FrequencyConverter.ConvertFrequencyType(tarefa.Frequencia),
                        WeekDays = FrequencyConverter.ConvertPeriodoToWeekDays(tarefa.Periodo)
                    }
                })
                .ToList();
        }

        /// <summary>
        /// Busca áreas existentes no MongoDB e atualiza os IDs dos requests para evitar duplicados
        /// </summary>
        private async Task UpdateExistingAreaIdsAsync(
            List<AreaActivityRequest> areaActivityRequests,
            int legacyProjectId,
            CancellationToken cancellationToken)
        {
            var existingAreasResult = await _areaActivityService.GetByProjectIdAsync(legacyProjectId);
            if (!existingAreasResult.Success || existingAreasResult.Data is not IEnumerable<AreaActivityResponse> existingAreasList)
            {
                return;
            }

            var existingAreasMap = existingAreasList
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .ToDictionary(a => a.Name, a => a.Id);

            foreach (var request in areaActivityRequests)
            {
                if (string.IsNullOrWhiteSpace(request.Id) &&
                    !string.IsNullOrWhiteSpace(request.Name) &&
                    existingAreasMap.TryGetValue(request.Name, out var existingId))
                {
                    request.Id = existingId;
                }
            }
        }
    }
}
