using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IAreaActivityService _areaActivityService;
        private readonly ISqlServerDataAccessFactory _sqlServerDataAccessFactory;
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;

        public MigrationService(
            IAreaActivityService areaActivityService,
            ISqlServerDataAccessFactory sqlServerDataAccessFactory,
            IProjectService projectService,
            IEmployeeService employeeService)
        {
            _areaActivityService = areaActivityService ?? throw new ArgumentNullException(nameof(areaActivityService));
            _sqlServerDataAccessFactory = sqlServerDataAccessFactory ?? throw new ArgumentNullException(nameof(sqlServerDataAccessFactory));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        }

        public async Task<Result> MigrateFromSqlServerAsync(
            int legacyProjectId,
            string sqlServerConnectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sqlServerDataAccess = _sqlServerDataAccessFactory.Create(sqlServerConnectionString);

                // 1. Buscar projeto do SQL Server
                var sqlProject = await sqlServerDataAccess.GetProjectAsync(legacyProjectId, cancellationToken);
                if (sqlProject == null)
                {
                    return Result.Error($"Projeto {legacyProjectId} não encontrado no SQL Server.");
                }

                // 2. Verificar se projeto existe no MongoDB e criar/atualizar
                var projectId = await CreateOrUpdateProjectAsync(sqlProject, cancellationToken);
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return Result.Error($"Erro ao criar/atualizar projeto {legacyProjectId} no MongoDB.");
                }

                // 3. Buscar funcionários do SQL Server e criar/atualizar no MongoDB
                var sqlEmployees = await sqlServerDataAccess.GetEmployeesAsync(legacyProjectId, cancellationToken);
                await CreateOrUpdateEmployeesAsync(sqlEmployees, projectId, cancellationToken);

                // 4. Buscar áreas e tarefas do SQL Server
                var areas = await sqlServerDataAccess.GetAreasAsync(legacyProjectId, cancellationToken);
                if (!areas.Any())
                {
                    return Result.Error($"Nenhuma área encontrada para o projeto {legacyProjectId} no SQL Server.");
                }

                var tarefas = await sqlServerDataAccess.GetTarefasAsync(legacyProjectId, cancellationToken);
                var tarefasPorWorkAreaId = GroupTarefasByWorkAreaId(tarefas);

                // 5. Buscar o primeiro funcionário do projeto para atribuir ao employeeId
                // Agora garantido que existe, pois acabamos de criar/atualizar
                var firstEmployeeId = await GetFirstEmployeeIdFromProjectAsync(legacyProjectId, cancellationToken);

                // 6. Migrar áreas
                var areaActivityRequests = MapToAreaActivityRequests(areas, tarefasPorWorkAreaId, legacyProjectId, firstEmployeeId);
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

                var resultData = new
                {
                    areasMigrated = areaActivityRequests.Count,
                    totalItems = areaActivityRequests.Sum(a => a.Items?.Count() ?? 0),
                    projectId = legacyProjectId,
                    employeesMigrated = sqlEmployees.Count,
                    employeeIdAssigned = !string.IsNullOrWhiteSpace(firstEmployeeId),
                    employeeId = firstEmployeeId
                };

                var message = string.IsNullOrWhiteSpace(firstEmployeeId)
                    ? $"Migração concluída. ⚠️ Atenção: EmployeeId não foi atribuído (projeto {legacyProjectId} não tem funcionários)."
                    : $"Migração concluída. ✅ Projeto, {sqlEmployees.Count} funcionário(s) e áreas migrados. EmployeeId atribuído: {firstEmployeeId}";

                return Result.Ok(message, resultData);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erro durante a migração: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner: {ex.InnerException.Message}";
                }
                return Result.Error(errorMessage);
            }
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
        /// Busca o primeiro funcionário do projeto pelo legacyId
        /// </summary>
        /// <remarks>
        /// Retorna o ID do primeiro funcionário encontrado no projeto.
        /// Estamos usando FirstOrDefault pois ainda não há uma regra de negócio clara
        /// sobre qual funcionário deve ser atribuído a cada área durante a migração.
        /// </remarks>
        private async Task<string?> GetFirstEmployeeIdFromProjectAsync(int legacyProjectId, CancellationToken cancellationToken)
        {
            try
            {
                var projectResult = await _projectService.GetByLegacyIdAsync(legacyProjectId);

                if (!projectResult.Success)
                {
                    // Projeto não encontrado ou erro ao buscar
                    // Nota: Isso é esperado se o projeto ainda não foi criado no MongoDB
                    // O employeeId ficará null e pode ser preenchido posteriormente
                    System.Diagnostics.Debug.WriteLine($"⚠️ Projeto {legacyProjectId} não encontrado no MongoDB. Mensagem: {projectResult.Message}");
                    return null;
                }

                if (projectResult.Data is not ProjectResponse projectResponse)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Dados do projeto {legacyProjectId} não são do tipo ProjectResponse");
                    return null;
                }

                // Pega o primeiro funcionário do array de funcionários do projeto
                // Nota: Usando FirstOrDefault pois ainda não há regra de negócio clara
                // sobre qual funcionário atribuir a cada área na migração
                var employees = projectResponse.Employees?.ToList() ?? new List<EmployeeResponse>();

                System.Diagnostics.Debug.WriteLine($"📊 Projeto {legacyProjectId} encontrado. Total de funcionários: {employees.Count}");

                if (!employees.Any())
                {
                    // Projeto não tem funcionários cadastrados
                    // O employeeId ficará null
                    System.Diagnostics.Debug.WriteLine($"⚠️ Projeto {legacyProjectId} não tem funcionários cadastrados");
                    return null;
                }

                var firstEmployee = employees.FirstOrDefault();
                var employeeId = firstEmployee?.Id;

                System.Diagnostics.Debug.WriteLine($"✅ Primeiro funcionário do projeto {legacyProjectId}: {employeeId} (Nome: {firstEmployee?.FullName})");

                return employeeId;
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna null (employeeId ficará null)
                // Log do erro para debug (pode ser removido em produção)
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao buscar funcionário do projeto {legacyProjectId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Converte áreas e tarefas do SQL Server para AreaActivityRequest
        /// </summary>
        private static List<AreaActivityRequest> MapToAreaActivityRequests(
            List<SqlServerAreaEntity> areas,
            Dictionary<int, List<SqlServerTarefaEntity>> tarefasPorWorkAreaId,
            int legacyProjectId,
            string? employeeId)
        {
            var areaActivityRequests = new List<AreaActivityRequest>();
            
            // Ordena áreas por WORK_AREA_ID e usa DENSE_RANK para orderBy (como na query SQL original)
            var areasOrdenadas = areas.OrderBy(a => a.WorkAreaId).ToList();
            short orderBy = 1;

            foreach (var area in areasOrdenadas)
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
                    // Atribui o primeiro funcionário do projeto a todas as áreas
                    // Nota: Usando o primeiro funcionário pois ainda não há regra de negócio clara
                    // sobre qual funcionário atribuir a cada área durante a migração
                    EmployeeId = employeeId,
                    // Usa WORK_AREA_ID diretamente como headerId (a query SQL usa DENSE_RANK, mas o valor real é mais útil)
                    HeaderId = area.WorkAreaId.ToString(),
                    OrderBy = orderBy++, // DENSE_RANK equivalente (incremental baseado em WORK_AREA_ID)
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
                .OrderBy(t => t.Ordem) // Usa ORDEM da tabela WORK_TAREFAS, como na query SQL original
                .ThenBy(t => t.Tarefa) // Ordenação secundária por TAREFA_NUMERO
                .Select(tarefa => new AreaActivityItemRequest
                {
                    Id = tarefa.Tarefa.ToString(),
                    Name = tarefa.Descricao ?? string.Empty,
                    OrderBy = (short)(tarefa.Ordem > 0 ? tarefa.Ordem : tarefa.Tarefa), // Usa ORDEM se disponível, senão usa Tarefa
                    Frequency = new AreaActivityFrequency
                    {
                        // Usa FrequenciaDias (FREQ.DIAS) para conversão, como na query SQL original
                        Type = ConvertFrequencyTypeFromDias(tarefa.FrequenciaDias, tarefa.FrequenciaNome),
                        WeekDays = FrequencyConverter.ConvertPeriodoToWeekDays(tarefa.Periodo)
                    }
                })
                .ToList();
        }

        /// <summary>
        /// Converte frequência baseada em DIAS (como na query SQL original)
        /// </summary>
        private static string ConvertFrequencyTypeFromDias(int dias, string frequenciaNome)
        {
            return dias switch
            {
                1 => "yearly",
                2 => "semi-annual",
                4 => "quarterly",
                6 => "bimonthly",
                12 => "monthly",
                26 => "biweekly",
                52 => "weekly",
                260 => "weekly", // 5 dias por semana * 52 semanas
                365 => "everyday",
                _ => !string.IsNullOrWhiteSpace(frequenciaNome) ? frequenciaNome.ToLower() : "weekly" // Fallback para nome da frequência ou padrão
            };
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

            // Agrupa por nome e pega o primeiro ID caso haja duplicatas
            // Nota: Se houver áreas com o mesmo nome, usa a primeira encontrada
            var existingAreasMap = existingAreasList
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

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

        /// <summary>
        /// Cria ou atualiza o projeto no MongoDB a partir dos dados do SQL Server
        /// </summary>
        private async Task<string?> CreateOrUpdateProjectAsync(SqlServerProjectEntity sqlProject, CancellationToken cancellationToken)
        {
            try
            {
                // Verifica se projeto já existe no MongoDB
                var existingProjectResult = await _projectService.GetByLegacyIdAsync(sqlProject.WorkHeaderId);
                string? existingProjectId = null;

                if (existingProjectResult.Success && existingProjectResult.Data is ProjectResponse existingProject)
                {
                    existingProjectId = existingProject.Id;
                }

                // Concatena endereço
                var address = $"{sqlProject.End1} {sqlProject.End2} {sqlProject.End3}".Trim();

                // Cria ProjectRequest
                var projectRequest = new ProjectRequest
                {
                    Id = existingProjectId ?? string.Empty,
                    LegacyId = sqlProject.WorkHeaderId,
                    Name = sqlProject.NomeProjeto ?? string.Empty,
                    TotalM2 = Convert.ToInt32(sqlProject.TotalM2),
                    DaysYear = sqlProject.DiasAno,
                    Factor = Convert.ToInt32(sqlProject.Fator),
                    Address = address,
                    Contact = sqlProject.Contato ?? string.Empty,
                    TelephoneNumber = sqlProject.Telefone ?? string.Empty,
                    CellphoneNumber = sqlProject.Celular ?? string.Empty,
                    RegistrationDate = sqlProject.DataCad != DateTime.MinValue ? sqlProject.DataCad : DateTime.Now,
                    Level = sqlProject.NivelProjeto,
                    Employees = new List<EmployeeRequest>() // Funcionários serão criados separadamente
                };

                // Salva projeto
                var saveResult = await _projectService.SaveAsync(projectRequest);
                if (!saveResult.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erro ao salvar projeto {sqlProject.WorkHeaderId}: {saveResult.Message}");
                    return null;
                }

                // Busca o projeto salvo para obter o ID
                var savedProjectResult = await _projectService.GetByLegacyIdAsync(sqlProject.WorkHeaderId);
                if (savedProjectResult.Success && savedProjectResult.Data is ProjectResponse savedProject)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Projeto {sqlProject.WorkHeaderId} criado/atualizado. ID: {savedProject.Id}");
                    return savedProject.Id;
                }

                return existingProjectId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar/atualizar projeto {sqlProject.WorkHeaderId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Cria ou atualiza funcionários no MongoDB a partir dos dados do SQL Server
        /// </summary>
        private async Task CreateOrUpdateEmployeesAsync(List<SqlServerEmployeeEntity> sqlEmployees, string projectId, CancellationToken cancellationToken)
        {
            // Busca funcionários existentes do projeto para verificar duplicatas
            var existingProjectResult = await _projectService.GetByIdAsync(projectId);
            var existingEmployees = new List<EmployeeResponse>();

            if (existingProjectResult.Success && existingProjectResult.Data is ProjectResponse existingProject)
            {
                existingEmployees = existingProject.Employees?.ToList() ?? new List<EmployeeResponse>();
            }

            foreach (var sqlEmployee in sqlEmployees)
            {
                try
                {
                    // Verifica se funcionário já existe (por Number e ProjectId)
                    var existingEmployee = existingEmployees.FirstOrDefault(e => e.Number == sqlEmployee.Funcionario);

                    var employeeRequest = new EmployeeRequest
                    {
                        Id = existingEmployee?.Id ?? string.Empty,
                        LegacyId = 0, // Não temos legacyId para funcionário
                        FirstName = $"Funcionario {sqlEmployee.Funcionario}",
                        LastName = string.Empty,
                        Number = sqlEmployee.Funcionario,
                        Observation = sqlEmployee.Obs ?? string.Empty,
                        ProjectId = projectId
                    };

                    var saveResult = await _employeeService.SaveAsync(employeeRequest);
                    if (!saveResult.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao salvar funcionário {sqlEmployee.Funcionario} do projeto {projectId}: {saveResult.Message}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Funcionário {sqlEmployee.Funcionario} criado/atualizado no projeto {projectId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar/atualizar funcionário {sqlEmployee.Funcionario}: {ex.Message}");
                    // Continua com próximo funcionário mesmo se este falhar
                }
            }
        }
    }
}
