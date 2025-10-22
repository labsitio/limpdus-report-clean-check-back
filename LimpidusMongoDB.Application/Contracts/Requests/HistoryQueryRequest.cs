
namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class HistoryQueryRequest
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Department { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeLastname { get; set; }
        public bool? Status { get; set; }
    }
}