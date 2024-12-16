using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class HistoryResponse
    {
        public HistoryResponse(HistoryEntity history)
        {
            Id = history.Id.ToString();
            ProjectId = history.ProjectId;
            EmployeeId = history.EmployeeId;
            AreaTaskId = history.AreaTaskId.ToString();
            AreaTaskName = history.AreaTaskName;
            CreatedDate = history.CreatedDate;
            EndDate = history.EndDate;
            Items = history.Items?.Select(x => (HistoryItemResponse)x);
            User = (UserResponse)history.User;
            Justification = history.Justification != null ? (JustificationResponse)history.Justification : null;
        }

        public string Id { get; set; }
        public int ProjectId { get; set; }
        public string EmployeeId { get; set; }
        public string AreaTaskId { get; set; }
        public string AreaTaskName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EndDate { get; set; }
        public UserResponse User { get; set; } 
        public IEnumerable<HistoryItemResponse> Items { get; set; }
        public JustificationResponse Justification { get; set; }
    }
}