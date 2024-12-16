using LimpidusMongoDB.Application.CustomAttributes;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("itemHistory")]
    public class ItemHistoryEntity : BaseEntity
    {
        public string LegacyId { get; set; }

        public string Name { get; set; }

        public string EndDate { get; set; }

        public bool Performed { get; set; }

        public string Description { get; set; }

        public int OrderBy { get; set; }

        public string HistoryId { get; set; }
    }
}