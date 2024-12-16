using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IProjectService
    {
        public Task<Result> GetAllProjects();

        public Task<Result> GetByLegacyIdAsync(int legacyId);

        public Task<Result> GetByIdAsync(string id);

        public Task<Result> SaveAsync(ProjectRequest request);
    }
}