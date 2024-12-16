using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        public Task<Result> SaveAsync(EmployeeRequest request);
        Task<Result> GetEmployeeByIdAsync(string id);
        Task<Result> GetEmployeeByNameAsync(string name);
    }
}
