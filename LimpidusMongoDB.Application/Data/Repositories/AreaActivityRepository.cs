using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class AreaActivityRepository : BaseRepository<AreaActivityEntity>, IAreaActivityRepository
    {
        public AreaActivityRepository(LimpidusContextDB context) : base(context)
        {
        }
    }
}
