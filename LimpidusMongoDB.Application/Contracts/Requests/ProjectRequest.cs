namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class ProjectRequest
    {
        public string Id { get; set; }
        public int LegacyId { get; set; }
        public string Name { get; set; }
        public int TotalM2 { get; set; }
        public int DaysYear { get; set; }
        public int Factor { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string TelephoneNumber { get; set; }
        public string CellphoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public IEnumerable<EmployeeRequest> Employees { get; set; }
        public int Level { get; set; }
    }
}
