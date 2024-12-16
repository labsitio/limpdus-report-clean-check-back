namespace LimpidusMongoDB.Application.Data.Entities
{
    public class AreaActivityFrequencyEntity
    {
        public string Type { get; set; }
        public IEnumerable<short> WeekDays { get; set; }
    }
}
