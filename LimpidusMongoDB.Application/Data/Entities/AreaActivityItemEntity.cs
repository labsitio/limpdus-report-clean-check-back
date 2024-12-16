namespace LimpidusMongoDB.Application.Data.Entities
{
    public class AreaActivityItemEntity
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequencyEntity Frequency { get; set; }
    }
}
