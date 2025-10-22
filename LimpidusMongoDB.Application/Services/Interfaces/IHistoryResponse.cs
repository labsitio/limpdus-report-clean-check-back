using LimpidusMongoDB.Application.Contracts.Responses;

namespace LimpidusMongoDB.Application.Services.Interfaces;

public class HistoryListResponse
{
    public IList<HistoryAuditResponse> Data { get; set; }
    public IList<string> Departments {get; set;}
    public IList<HistoryUserResponse> Employees { get; set; }
}

public class HistoryUserResponse
{
    public string Name {get; set;}
    public string LastName { get; set; }
}