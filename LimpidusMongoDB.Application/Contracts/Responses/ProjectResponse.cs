using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class ProjectResponse
    {
        public ProjectResponse(ProjectEntity project, IEnumerable<EmployeeEntity> employeeList)
        {
            Id = project.Id.ToString();
            LegacyId = project.LegacyId;
            Name = project.Name;
            TotalM2 = project.TotalM2;
            DaysYear = project.DaysYear;
            Factor = project.Factor;
            Address = project.Address;
            Contact = project.Contact;
            TelephoneNumber = project.TelephoneNumber;
            CellphoneNumber = project.CellphoneNumber;
            RegistrationDate = project.RegistrationDate;
            Employees = employeeList.Select(x => (EmployeeResponse)x);
            Level = project.Level;
        }

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
        public IEnumerable<EmployeeResponse> Employees { get; set; }
        public int Level { get; set; }
    }
}