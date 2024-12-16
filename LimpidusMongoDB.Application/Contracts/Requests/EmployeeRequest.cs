namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class EmployeeRequest
    {
        public string Id { get; set; }
        public int LegacyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
        public string Observation { get; set; }
        public string ProjectId { get; set; }
    }
}
