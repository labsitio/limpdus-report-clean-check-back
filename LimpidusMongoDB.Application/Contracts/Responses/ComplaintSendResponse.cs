namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class ComplaintSendResponse
    {
        public bool Sent { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
