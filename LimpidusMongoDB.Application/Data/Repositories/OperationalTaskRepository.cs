using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class OperationalTaskRepository : BaseRepository<OperationalTaskEntity>, IOperationalTaskRepository
    {
        public OperationalTaskRepository(LimpidusContextDB contextDB) : base(contextDB) { }
    }
}