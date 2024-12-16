using LimpidusMongoDB.Application.Data.Entities;
using MongoDB.Bson;

namespace LimpidusMongoDB.Application.Data.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<UserEntity>
    {
    }
}