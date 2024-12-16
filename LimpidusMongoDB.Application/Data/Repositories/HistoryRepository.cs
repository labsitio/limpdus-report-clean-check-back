using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class HistoryRepository : BaseRepository<HistoryEntity>, IHistoryRepository
    {
        public HistoryRepository(LimpidusContextDB contextDB) : base(contextDB) { }
    }
}