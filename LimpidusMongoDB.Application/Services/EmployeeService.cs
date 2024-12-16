using System.Text.RegularExpressions;
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
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result> GetEmployeeByNameAsync(string name)
        {
            try
            {
                FilterDefinition<EmployeeEntity> filterDefinition = Builders<EmployeeEntity>.Filter.Regex(x => x.FirstName, new MongoDB.Bson.BsonRegularExpression(new Regex(name, RegexOptions.IgnoreCase)));
                var employee = await _employeeRepository.FindAsync(filterDefinition);

                var result = employee.Select(x => new EmployeeResponse(x)).ToList();

                return Result.Ok(data: result);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetEmployeeByIdAsync(string id)
        {
            try
            {
                var employee = await _employeeRepository.FindByIdAsync(id);

                var result = new EmployeeResponse(employee);

                return Result.Ok(data: result);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> SaveAsync(EmployeeRequest request)
        {
            try
            {
                var entity = new EmployeeEntity
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Number = request.Number,
                    Observation = request.Observation,
                    ProjectId = request.ProjectId,
                };

                if (string.IsNullOrWhiteSpace(request?.Id))
                {
                    await _employeeRepository.InsertOneAsync(entity);
                }
                else
                {
                    await _employeeRepository.UpdateOneAsync(request.Id, entity.GetUpdateDefinition());
                }

                return Result.Ok(data: request?.Id ?? entity.Id.ToString());
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }
    }
}
