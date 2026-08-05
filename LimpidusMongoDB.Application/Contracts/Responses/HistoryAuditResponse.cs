namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class HistoryAuditResponse
    {
        public string Id { get; set; }
        public string Department { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeLastName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public TimeSpan Duration => DateStart - DateEnd;
        public bool Status { get; set; }
        public JustificationResponse? Justification { get; set; }

        /// <summary>
        /// Atividades da execução. Preenchido apenas para Clean Check N2/N3 (Level &gt;= 2); null em N1.
        /// </summary>
        public IList<HistoryItemResponse>? Items { get; set; }
    }
}