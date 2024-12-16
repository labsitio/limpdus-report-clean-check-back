using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Data.Repositories.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<EmployeeEntity>
    {
        Task<IEnumerable<EmployeeEntity>> FindByProjectIdAsync(string id);
    }
}