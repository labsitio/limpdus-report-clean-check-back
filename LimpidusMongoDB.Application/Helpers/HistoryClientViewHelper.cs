using LimpidusMongoDB.Application.Contracts.Responses;
using HistoryListResponse = LimpidusMongoDB.Application.Services.Interfaces.HistoryListResponse;
using HistoryUserResponse = LimpidusMongoDB.Application.Services.Interfaces.HistoryUserResponse;

namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Regras de visão do histórico para ProjectViewer (cliente).
    /// </summary>
    public static class HistoryClientViewHelper
    {
        /// <summary>
        /// Sem permissão: só áreas com atividades e todas realizadas.
        /// Com <paramref name="showUnperformed"/>: áreas com atividades (inclui não realizadas).
        /// </summary>
        public static void ApplyForProjectViewer(
            HistoryListResponse list,
            bool showUnperformed)
        {
            if (list?.Data == null)
                return;

            list.Data = list.Data
                .Where(row => IsVisibleToClient(row, showUnperformed))
                .ToList();

            list.Departments = list.Data
                .Select(x => x.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .OrderBy(x => x)
                .Distinct()
                .ToList();

            list.Employees = list.Data
                .GroupBy(x => x.EmployeeName + " " + x.EmployeeLastName)
                .Select(g => new HistoryUserResponse
                {
                    Name = g.First().EmployeeName,
                    LastName = g.First().EmployeeLastName
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.LastName)
                .ToList();
        }

        public static bool IsVisibleToClient(HistoryAuditResponse row, bool showUnperformed)
        {
            if (row?.Items == null || row.Items.Count == 0)
                return false;

            if (showUnperformed)
                return true;

            return row.Items.All(i => i.Performed);
        }
    }
}
