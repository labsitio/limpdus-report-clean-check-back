using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class ItemHistoryRepository : BaseRepository<ItemHistoryEntity>, IItemHistoryRepository
    {
        public ItemHistoryRepository(LimpidusContextDB contextDB) : base(contextDB) { }

        public new async Task<IEnumerable<ItemHistoryEntity>> FindByHistoryIdAsync(string id)
        {
            var filterDefinition = Builders<ItemHistoryEntity>.Filter.Where(x => x.HistoryId == id);
            return await FindAsync(filterDefinition);
        }
    }
}