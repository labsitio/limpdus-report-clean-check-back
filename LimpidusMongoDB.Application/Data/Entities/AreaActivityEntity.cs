using LimpidusMongoDB.Application.CustomAttributes;
using LimpidusMongoDB.Application.Helpers;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("areaActivity")]
    public class AreaActivityEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int TotalM2 { get; set; }
        public string EmployeeId { get; set; }
        public string HeaderId { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequencyEntity Frequency { get; set; }
        public IEnumerable<AreaActivityItemEntity> Items { get; set; }
        public int ProjectId { get; set; }

        public UpdateDefinition<AreaActivityEntity> GetUpdateDefinition() =>
            Builders<AreaActivityEntity>.Update
                .Set(nameof(Name).FirstCharToLowerCase(), Name)
                .Set(nameof(Description).FirstCharToLowerCase(), Description)
                .Set(nameof(QuickTask).FirstCharToLowerCase(), QuickTask)
                .Set(nameof(TotalM2).FirstCharToLowerCase(), TotalM2)
                .Set(nameof(EmployeeId).FirstCharToLowerCase(), EmployeeId)
                .Set(nameof(HeaderId).FirstCharToLowerCase(), HeaderId)
                .Set(nameof(OrderBy).FirstCharToLowerCase(), OrderBy)
                .Set(nameof(Frequency).FirstCharToLowerCase(), Frequency)
                .Set(nameof(Items).FirstCharToLowerCase(), Items);
    }
}
