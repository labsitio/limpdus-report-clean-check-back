using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class UserRepository : BaseRepository<UserEntity>, IUserRepository
    {
        public UserRepository(LimpidusContextDB contextDB) : base(contextDB) { }

    }
}