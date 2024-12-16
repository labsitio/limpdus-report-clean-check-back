using LimpidusMongoDB.Application.Contracts;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IOperationalTaskService
    {
        public Task<Result> GetAllOperationalTasks();
    }
}