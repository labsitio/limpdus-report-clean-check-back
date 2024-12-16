namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class AreaActivityRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int TotalM2 { get; set; }
        public string EmployeeId { get; set; }
        public string HeaderId { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequency Frequency { get; set; }
        public IEnumerable<AreaActivityItemRequest> Items { get; set; }
        public int ProjectId { get; set; }
    }

    public class AreaActivityItemRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequency Frequency { get; set; }
    }

    public class AreaActivityFrequency
    {
        public string Type { get; set; }
        public IEnumerable<short> WeekDays { get; set; }
    }
}
