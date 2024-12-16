using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class ProjectRepository : BaseRepository<ProjectEntity>, IProjectRepository
    {
        public ProjectRepository(LimpidusContextDB contextDB) : base(contextDB) { }
    }
}