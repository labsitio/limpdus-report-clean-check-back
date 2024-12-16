using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class EmployeeResponse
    {
        public EmployeeResponse(
            string id,
            string firstName,
            string lastName,
            int number,
            string observation)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Number = number;
            Observation = observation;
        }

        public static implicit operator EmployeeResponse(EmployeeEntity employeeEntity)
        {
            return new EmployeeResponse(
                employeeEntity.Id.ToString(),
                employeeEntity.FirstName,
                employeeEntity.LastName,
                employeeEntity.Number,
                employeeEntity.Observation);
        }
        
        public EmployeeResponse(EmployeeEntity employeeEntity)
        {
            Id = employeeEntity.Id.ToString();
            FirstName = employeeEntity.FirstName;
            LastName = employeeEntity.LastName;
            Number = employeeEntity.Number;
            Observation = employeeEntity.Observation;
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public int Number { get; set; }
        public string Observation { get; set; }
    }
}