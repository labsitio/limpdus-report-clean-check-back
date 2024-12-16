using LimpidusMongoDB.Application.CustomAttributes;
using MongoDB.Bson;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("user")]
    public class UserEntity : BaseEntity
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public string UserId { get; set; }
    }
}