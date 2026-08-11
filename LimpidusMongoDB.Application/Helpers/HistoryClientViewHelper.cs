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
        /// Filtra áreas incompletas e aplica permissões de atividades na lista do relatório.
        /// </summary>
        public static void ApplyForProjectViewer(
            HistoryListResponse list,
            bool showActivities,
            IReadOnlyCollection<string>? visibleItemIds)
        {
            if (list?.Data == null)
                return;

            var allowed = visibleItemIds == null
                ? null
                : new HashSet<string>(
                    visibleItemIds.Where(id => !string.IsNullOrWhiteSpace(id)),
                    StringComparer.OrdinalIgnoreCase);

            list.Data = list.Data
                .Where(IsFullyCompletedForClient)
                .Select(row => ApplyActivityVisibility(row, showActivities, allowed))
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

        /// <summary>
        /// Área só aparece para o cliente se tiver atividades e todas foram realizadas.
        /// Sem itens (nada para expandir) = não mostra.
        /// </summary>
        public static bool IsFullyCompletedForClient(HistoryAuditResponse row)
        {
            if (row?.Items == null || row.Items.Count == 0)
                return false;

            return row.Items.All(i => i.Performed);
        }

        private static HistoryAuditResponse ApplyActivityVisibility(
            HistoryAuditResponse row,
            bool showActivities,
            HashSet<string>? allowedIds)
        {
            if (!showActivities)
            {
                row.Items = null;
                return row;
            }

            if (row.Items == null)
                return row;

            if (allowedIds == null)
                return row;

            row.Items = row.Items
                .Where(i =>
                    (!string.IsNullOrWhiteSpace(i.Id) && allowedIds.Contains(i.Id))
                    || (!string.IsNullOrWhiteSpace(i.Name) && allowedIds.Contains(i.Name)))
                .ToList();

            return row;
        }
    }
}
