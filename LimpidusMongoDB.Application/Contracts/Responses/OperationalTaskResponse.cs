using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class OperationalTaskResponse
    {
        public OperationalTaskResponse(OperationalTaskEntity operationalTask, IEnumerable<ItemOperationalTaskEntity> itemList)
        {
            Id = operationalTask.Id.ToString();
            LegacyId = operationalTask.LegacyId.ToString();
            Name = operationalTask.Name;
            Description = operationalTask.Description;
            QuickTask = operationalTask.QuickTask;
            EmployeeId = operationalTask.EmployeeId;
            LegacyProjectId = operationalTask.LegacyProjectId.ToString();
            OrderBy = operationalTask.OrderBy;
            Items = itemList.Select(x => (ItemOperationalTaskResponse)x);
        }

        public string Id { get; set; }
        public string LegacyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int EmployeeId { get; set; }
        public string LegacyProjectId { get; set; }
        public int OrderBy { get; set; }
        public IEnumerable<ItemOperationalTaskResponse> Items { get; set; }
    }
}