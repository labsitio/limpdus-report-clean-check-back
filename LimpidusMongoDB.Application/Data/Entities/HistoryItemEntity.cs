namespace LimpidusMongoDB.Application.Data.Entities
{
    public class HistoryItemEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short? OrderBy { get; set; }
        public DateTime EndDate { get; set; }
        public bool Performed { get; set; }
    }
}
