using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Data.Repositories.Interfaces
{
    public interface IItemOperationalTaskRepository : IBaseRepository<ItemOperationalTaskEntity>
    {
        Task<IEnumerable<ItemOperationalTaskEntity>> FindByOperationalTaskIdAsync(string id);
    }
}