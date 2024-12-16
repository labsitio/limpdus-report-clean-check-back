using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class EmployeeRepository : BaseRepository<EmployeeEntity>, IEmployeeRepository
    {
        public EmployeeRepository(LimpidusContextDB contextDB) : base(contextDB) { }

        public async Task<IEnumerable<EmployeeEntity>> FindByProjectIdAsync(string id)
        {
            var filterDefinition = Builders<EmployeeEntity>.Filter.Where(x => x.ProjectId == id);
            return await FindAsync(filterDefinition);
        }
    }
}