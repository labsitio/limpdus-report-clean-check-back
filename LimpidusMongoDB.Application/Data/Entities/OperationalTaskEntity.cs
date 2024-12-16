using LimpidusMongoDB.Application.CustomAttributes;

namespace LimpidusMongoDB.Application.Data.Entities
{
    [CollectionName("operationalTask")]
    public class OperationalTaskEntity : BaseEntity
    {
        public int LegacyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int EmployeeId { get; set; }
        public string LegacyProjectId { get; set; }
        public int OrderBy { get; set; }
    }
}