using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class UserResponse
    {
        public UserResponse(string name, string lastName)
        {
            Name = name;
            LastName = lastName;
        }

        public static implicit operator UserResponse(HistoryUserEntity userEntity)
        {
            return new UserResponse(userEntity.Name, userEntity.LastName);
        }

        public string Name { get; set; }
        public string LastName { get; set; }
    }
}