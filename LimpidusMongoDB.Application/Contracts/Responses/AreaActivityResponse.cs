namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class AreaActivityResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int TotalM2 { get; set; }
        public string EmployeeId { get; set; }
        public string HeaderId { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequencyResponse Frequency { get; set; }
        public IEnumerable<AreaActivityItemResponse> Items { get; set; }
        public int ProjectId { get; set; }
    }

    public class AreaActivityItemResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequencyResponse Frequency { get; set; }
    }

    public class AreaActivityFrequencyResponse
    {
        public string Type { get; set; }
        public IEnumerable<short> WeekDays { get; set; }
    }
}
