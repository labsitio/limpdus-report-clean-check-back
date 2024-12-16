using LimpidusMongoDB.Application.CustomAttributes;
using LimpidusMongoDB.Application.Helpers;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("employee")]
    public class EmployeeEntity : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
        public string Observation { get; set; }
        public string ProjectId { get; set; }

        public UpdateDefinition<EmployeeEntity> GetUpdateDefinition() =>
            Builders<EmployeeEntity>.Update
                .Set(nameof(FirstName).FirstCharToLowerCase(), FirstName)
                .Set(nameof(LastName).FirstCharToLowerCase(), LastName)
                .Set(nameof(Number).FirstCharToLowerCase(), Number)
                .Set(nameof(Observation).FirstCharToLowerCase(), Observation);
    }
}