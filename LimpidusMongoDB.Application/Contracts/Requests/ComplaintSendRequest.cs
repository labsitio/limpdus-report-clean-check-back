namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class ComplaintSendRequest
    {
        public int LegacyProjectId { get; set; }
        public int LegacyAreaId { get; set; }
        public List<string> Problems { get; set; } = new();
        public string Comments { get; set; } = string.Empty;
    }
}
