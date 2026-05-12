namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class AreaActivityRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool QuickTask { get; set; }
        public int TotalM2 { get; set; }
        public string EmployeeId { get; set; }
        public string HeaderId { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequency Frequency { get; set; }
        public IEnumerable<AreaActivityItemRequest> Items { get; set; }
        public int ProjectId { get; set; }
    }

    public class AreaActivityItemRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short OrderBy { get; set; }
        public AreaActivityFrequency Frequency { get; set; }
    }

    /// <summary>
    /// <see cref="WeekDays"/> usa o mesmo critério que <see cref="System.DayOfWeek"/> / JavaScript <c>Date.getDay()</c>:
    /// 0 = Domingo, 1 = Segunda, …, 5 = Sexta, 6 = Sábado.
    /// Ex.: domingo a quinta = [0,1,2,3,4]. Segunda a sexta = [1,2,3,4,5]. Não deslocar +1 em cima de índices de checkbox se domingo já for 0.
    /// </summary>
    public class AreaActivityFrequency
    {
        public string Type { get; set; }
        public IEnumerable<short> WeekDays { get; set; }
    }
}
