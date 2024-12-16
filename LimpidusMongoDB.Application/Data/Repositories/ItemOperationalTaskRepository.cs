using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class ItemOperationalTaskRepository : BaseRepository<ItemOperationalTaskEntity>, IItemOperationalTaskRepository
    {
        public ItemOperationalTaskRepository(LimpidusContextDB contextDB) : base(contextDB) { }

        public async Task<IEnumerable<ItemOperationalTaskEntity>> FindByOperationalTaskIdAsync(string id)
        {
            var filterDefinition = Builders<ItemOperationalTaskEntity>.Filter.Where(x => x.OperationalTaskId == id);
            return await FindAsync(filterDefinition);
        }
    }
}