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

        public ProjectService(IProjectRepository projectRepository, IEmployeeRepository employeeRepository)
        {
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result> GetAllProjects()
        {
            try
            {
                var projects = await _projectRepository.FindAllAsync();

                if (!projects?.Any() ?? true)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var projectResponseList = new List<ProjectResponse>();

                foreach (var project in projects)
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
                var project = await _projectRepository.FindOneAsync(Builders<ProjectEntity>.Filter.Eq("legacyId", legacyId));
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

        #region Private methods

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