using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAreaActivityRepository _areaActivityRepository;

        public ProjectService(
            IProjectRepository projectRepository,
            IEmployeeRepository employeeRepository,
            IAreaActivityRepository areaActivityRepository)
        {
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
            _areaActivityRepository = areaActivityRepository;
        }

        public async Task<Result> GetAllProjects()
        {
            try
            {
                var projects = await _projectRepository.FindAllAsync();

                if (!projects?.Any() ?? true)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var projectResponseList = new List<ProjectResponse>();

                foreach (var project in ProjectLegacyResolver.DeduplicateByLegacyId(projects))
                    projectResponseList.Add(await GetProjectDetail(project));

                return Result.Ok(data: projectResponseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetByLegacyIdAsync(int legacyId)
        {
            try
            {
                var projects = await _projectRepository.FindAsync(
                    Builders<ProjectEntity>.Filter.Eq(x => x.LegacyId, legacyId));
                var project = ProjectLegacyResolver.PreferCanonical(projects);
                if (project == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var response = await GetProjectDetail(project);

                return Result.Ok(data: response);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetByIdAsync(string id)
        {
            try
            {
                var project = await _projectRepository.FindByIdAsync(id);
                if (project == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var response = await GetProjectDetail(project);

                return Result.Ok(data: response);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> SaveAsync(ProjectRequest request)
        {
            try
            {
                var projectEntity = new ProjectEntity
                {
                    LegacyId = request.LegacyId,
                    Name = request.Name,
                    TotalM2 = request.TotalM2,
                    DaysYear = request.DaysYear,
                    Factor = request.Factor,
                    Address = request.Address,
                    Contact = request.Contact,
                    TelephoneNumber = request.TelephoneNumber,
                    CellphoneNumber = request.CellphoneNumber,
                    RegistrationDate = request.RegistrationDate,
                    Level = request.Level
                };

                if (string.IsNullOrWhiteSpace(request?.Id))
                {
                    await _projectRepository.InsertOneAsync(projectEntity);
                }
                else
                {
                    await _projectRepository.UpdateOneAsync(request.Id, projectEntity.GetUpdateDefinition());
                }

                if (request.Employees?.Any() == true)
                {
                    await SaveEmployeesAsync(request?.Id ?? projectEntity.Id.ToString(), request.Employees);
                }

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<int?> GetMaxHistoryRangeDaysAsync(int legacyId, CancellationToken cancellationToken = default)
        {
            var projects = await _projectRepository.FindAsync(
                Builders<ProjectEntity>.Filter.Eq(x => x.LegacyId, legacyId),
                cancellationToken);
            var project = ProjectLegacyResolver.PreferCanonical(projects);
            return project?.MaxHistoryRangeDays;
        }

        public async Task<int> GetEffectiveProjectViewerMaxDaysAsync(int legacyId, CancellationToken cancellationToken = default)
        {
            var overrideDays = await GetMaxHistoryRangeDaysAsync(legacyId, cancellationToken);
            return HistoryRangeLimits.EffectiveProjectViewerDays(overrideDays);
        }

        public async Task<Result> SetMaxHistoryRangeDaysAsync(
            int legacyId,
            int? maxHistoryRangeDays,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (maxHistoryRangeDays is <= 0)
                    return Result.Error("maxHistoryRangeDays deve ser um inteiro positivo, ou null para o default.");

                var projects = await _projectRepository.FindAsync(
                    Builders<ProjectEntity>.Filter.Eq(x => x.LegacyId, legacyId),
                    cancellationToken);
                var project = ProjectLegacyResolver.PreferCanonical(projects);
                if (project == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var update = BaseEntity.UpdateDateDefinition(
                    Builders<ProjectEntity>.Update.Set(x => x.MaxHistoryRangeDays, maxHistoryRangeDays));

                await _projectRepository.UpdateOneAsync(project.Id.ToString(), update, cancellationToken);
                return Result.Ok(data: new HistoryRangeResponse
                {
                    LegacyId = legacyId,
                    MaxHistoryRangeDays = maxHistoryRangeDays,
                    DefaultProjectViewerDays = HistoryRangeLimits.ProjectViewerDefaultDays,
                    EffectiveMaxDays = HistoryRangeLimits.EffectiveProjectViewerDays(maxHistoryRangeDays)
                });
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetClientAccessAsync(int legacyId, CancellationToken cancellationToken = default)
        {
            try
            {
                var projects = await _projectRepository.FindAsync(
                    Builders<ProjectEntity>.Filter.Eq(x => x.LegacyId, legacyId),
                    cancellationToken);
                var project = ProjectLegacyResolver.PreferCanonical(projects);
                if (project == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var available = await BuildAvailableActivitiesAsync(legacyId, cancellationToken);
                return Result.Ok(data: MapClientAccess(project, available));
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> SetClientAccessAsync(
            int legacyId,
            SetClientAccessRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return Result.Error("Body inválido.");

                if (request.MaxHistoryRangeDays is <= 0)
                    return Result.Error("maxHistoryRangeDays deve ser um inteiro positivo, ou null para o default.");

                var projects = await _projectRepository.FindAsync(
                    Builders<ProjectEntity>.Filter.Eq(x => x.LegacyId, legacyId),
                    cancellationToken);
                var project = ProjectLegacyResolver.PreferCanonical(projects);
                if (project == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var showActivities = request.ShowActivitiesToClient ?? project.ShowActivitiesToClient ?? true;
                List<string>? visibleIds = project.ClientVisibleActivityItemIds;
                if (request.UpdateVisibleActivities)
                {
                    visibleIds = request.ClientVisibleActivityItemIds?
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(id => id.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                var update = BaseEntity.UpdateDateDefinition(
                    Builders<ProjectEntity>.Update
                        .Set(x => x.MaxHistoryRangeDays, request.MaxHistoryRangeDays)
                        .Set(x => x.ShowActivitiesToClient, showActivities)
                        .Set(x => x.ClientVisibleActivityItemIds, visibleIds));

                await _projectRepository.UpdateOneAsync(project.Id.ToString(), update, cancellationToken);

                project.MaxHistoryRangeDays = request.MaxHistoryRangeDays;
                project.ShowActivitiesToClient = showActivities;
                project.ClientVisibleActivityItemIds = visibleIds;

                var available = await BuildAvailableActivitiesAsync(legacyId, cancellationToken);
                return Result.Ok(data: MapClientAccess(project, available));
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        #region Private methods

        private static ClientAccessResponse MapClientAccess(
            ProjectEntity project,
            List<ClientActivityOptionResponse> available)
        {
            return new ClientAccessResponse
            {
                LegacyId = project.LegacyId,
                ProjectName = project.Name ?? string.Empty,
                MaxHistoryRangeDays = project.MaxHistoryRangeDays,
                DefaultProjectViewerDays = HistoryRangeLimits.ProjectViewerDefaultDays,
                EffectiveMaxDays = HistoryRangeLimits.EffectiveProjectViewerDays(project.MaxHistoryRangeDays),
                ShowActivitiesToClient = project.ShowActivitiesToClient ?? true,
                ClientVisibleActivityItemIds = project.ClientVisibleActivityItemIds,
                AvailableActivities = available
            };
        }

        private async Task<List<ClientActivityOptionResponse>> BuildAvailableActivitiesAsync(
            int legacyId,
            CancellationToken cancellationToken)
        {
            var areas = await _areaActivityRepository.FindAsync(
                Builders<AreaActivityEntity>.Filter.Eq(x => x.ProjectId, legacyId),
                cancellationToken);

            return (areas ?? Enumerable.Empty<AreaActivityEntity>())
                .SelectMany(a => a.Items ?? Enumerable.Empty<AreaActivityItemEntity>())
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemId) || !string.IsNullOrWhiteSpace(i.Name))
                .GroupBy(
                    i => string.IsNullOrWhiteSpace(i.ItemId) ? i.Name.Trim() : i.ItemId.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    return new ClientActivityOptionResponse
                    {
                        ItemId = string.IsNullOrWhiteSpace(first.ItemId) ? first.Name.Trim() : first.ItemId.Trim(),
                        Name = first.Name?.Trim() ?? first.ItemId?.Trim() ?? string.Empty
                    };
                })
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


        private async Task<ProjectResponse> GetProjectDetail(ProjectEntity projectEntity)
        {
            var employeeList = await _employeeRepository.FindByProjectIdAsync(projectEntity.Id.ToString());

            return new ProjectResponse(projectEntity, employeeList ?? Enumerable.Empty<EmployeeEntity>());
        }

        private async Task SaveEmployeesAsync(string projectId, IEnumerable<EmployeeRequest> employees)
        {
            foreach (var employee in employees)
            {
                var entity = new EmployeeEntity
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Number = employee.Number,
                    Observation = employee.Observation,
                    ProjectId = projectId,
                };

                if (string.IsNullOrWhiteSpace(employee?.Id))
                {
                    await _employeeRepository.InsertOneAsync(entity);
                }
                else
                {
                    await _employeeRepository.UpdateOneAsync(employee.Id, entity.GetUpdateDefinition());
                }
            }
        }

        #endregion
    }
}