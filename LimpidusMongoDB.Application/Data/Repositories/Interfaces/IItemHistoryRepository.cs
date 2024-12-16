using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Data.Repositories.Interfaces
{
    public interface IItemHistoryRepository : IBaseRepository<ItemHistoryEntity>
    {
        new Task<IEnumerable<ItemHistoryEntity>> FindByHistoryIdAsync(string id);
    }
}