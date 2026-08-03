namespace LimpidusMongoDB.Application.Contracts.Requests
{
    /// <summary>
    /// Body do PUT history-range. <c>null</c> = remove override (volta ao default 90 dias para cliente).
    /// </summary>
    public class SetHistoryRangeRequest
    {
        public int? MaxHistoryRangeDays { get; set; }
    }
}
