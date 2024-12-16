using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class JustificationRepository : BaseRepository<JustificationEntity>, IJustificationRepository
    {
        public JustificationRepository(LimpidusContextDB contextDB) : base(contextDB) { }
    }
}