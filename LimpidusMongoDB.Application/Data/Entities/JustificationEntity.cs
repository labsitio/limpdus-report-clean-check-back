using LimpidusMongoDB.Application.CustomAttributes;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("justification")]
    public class JustificationEntity : BaseEntity
    {
        public string Information { get; set; }

        public string Reason { get; set; }

        public string HistoryId { get; set; }
    }
}