using LimpidusMongoDB.Application.CustomAttributes;
using LimpidusMongoDB.Application.Helpers;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("project")]
    public class ProjectEntity : BaseEntity
    {
        public int LegacyId { get; set; }
        public string Name { get; set; }
        public int TotalM2 { get; set; }
        public int DaysYear { get; set; }
        public int Factor { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string TelephoneNumber { get; set; }
        public string CellphoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int Level { get; set; }

        public UpdateDefinition<ProjectEntity> GetUpdateDefinition() =>
            Builders<ProjectEntity>.Update
                .Set(nameof(Name).FirstCharToLowerCase(), Name)
                .Set(nameof(TotalM2).FirstCharToLowerCase(), TotalM2)
                .Set(nameof(DaysYear).FirstCharToLowerCase(), DaysYear)
                .Set(nameof(Factor).FirstCharToLowerCase(), Factor)
                .Set(nameof(Address).FirstCharToLowerCase(), Address)
                .Set(nameof(Contact).FirstCharToLowerCase(), Contact)
                .Set(nameof(TelephoneNumber).FirstCharToLowerCase(), TelephoneNumber)
                .Set(nameof(CellphoneNumber).FirstCharToLowerCase(), CellphoneNumber)
                .Set(nameof(RegistrationDate).FirstCharToLowerCase(), RegistrationDate)
                .Set(nameof(Level).FirstCharToLowerCase(), Level);
    }
}