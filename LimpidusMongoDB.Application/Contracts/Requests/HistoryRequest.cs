namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class HistoryRequest
    {
        public int ProjectId { get; set; }
        public string EmployeeId { get; set; }
        public string AreaTaskId { get; set; }
        public string AreaTaskName { get; set; }
        public DateTime EndDate { get; set; }
        public HistoryUserRequest User { get; set; }
        public IEnumerable<HistoryItemRequest> Items { get; set; }
        public JustificationRequest Justification { get; set; }
    }
}
