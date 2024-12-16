namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class SpreadsheetRequest
    {
        public string Name { get; set; }
        public string SheetName { get; set; }
        public string[][] Data { get; set; }
    }
}