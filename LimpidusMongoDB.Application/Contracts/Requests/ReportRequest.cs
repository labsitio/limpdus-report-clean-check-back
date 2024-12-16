using Microsoft.AspNetCore.Http;

namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class ReportRequest
    {
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
