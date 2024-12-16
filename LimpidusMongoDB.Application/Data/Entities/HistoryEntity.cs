using LimpidusMongoDB.Application.CustomAttributes;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("history")]
    public class HistoryEntity : BaseEntity
    {
        public int ProjectId { get; set; }
        public string EmployeeId { get; set; }
        public string AreaTaskId { get; set; }
        public string AreaTaskName { get; set; }
        public DateTime EndDate { get; set; }
        public HistoryUserEntity User { get; set; }
        public IEnumerable<HistoryItemEntity> Items { get; set; }
        public HistoryJustificationEntity Justification { get; set; }
    }
}