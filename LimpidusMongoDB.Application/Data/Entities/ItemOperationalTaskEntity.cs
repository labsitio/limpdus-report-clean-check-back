using LimpidusMongoDB.Application.CustomAttributes;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("itemOperationalTask")]
    public class ItemOperationalTaskEntity : BaseEntity
    {
        public string LegacyId { get; set; }

        public string Name { get; set; }

        public int OrderBy { get; set; }

        public string OperationalTaskId { get; set;}
    }
}