using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Deduplicação de execuções de histórico (double-tap / reenvios).
    /// Alinhada a <c>HistoryService.SaveAsync</c> e <c>scripts/mongo/dedupe-history.mongosh.js</c>.
    /// </summary>
    public static class HistoryDeduper
    {
        /// <summary>
        /// Mantém um documento por chave de negócio (o de menor <c>_id</c> = inserção mais antiga).
        /// </summary>
        public static IReadOnlyList<HistoryEntity> Deduplicate(IEnumerable<HistoryEntity> histories)
        {
            var list = histories?.ToList() ?? new List<HistoryEntity>();
            if (list.Count <= 1)
                return list;

            return list
                .GroupBy(DedupeKey)
                .Select(g => g.OrderBy(x => x.Id.ToString(), StringComparer.Ordinal).First())
                .ToList();
        }

        public static string DedupeKey(HistoryEntity entity) =>
            string.Join(
                "|",
                entity.ProjectId.ToString(),
                entity.EmployeeId ?? string.Empty,
                entity.AreaTaskId ?? string.Empty,
                TruncateToUnixSeconds(entity.EndDate).ToString(),
                NormalizeJustification(entity.Justification),
                ItemsFingerprintFromEntity(entity.Items));

        public static bool IsDuplicateSubmission(HistoryEntity existing, HistoryRequest incoming)
        {
            if (existing.ProjectId != incoming.ProjectId)
                return false;
            if (!string.Equals(existing.EmployeeId, incoming.EmployeeId, StringComparison.Ordinal))
                return false;
            if (!string.Equals(existing.AreaTaskId, incoming.AreaTaskId, StringComparison.Ordinal))
                return false;
            if (Math.Abs((existing.EndDate - incoming.EndDate).TotalSeconds) > 5)
                return false;
            if (!string.Equals(NormalizeJustification(existing.Justification), NormalizeJustification(incoming.Justification), StringComparison.Ordinal))
                return false;
            return string.Equals(
                ItemsFingerprintFromEntity(existing.Items),
                ItemsFingerprintFromRequest(incoming.Items),
                StringComparison.Ordinal);
        }

        public static string NormalizeJustification(HistoryJustificationEntity? j) =>
            j == null ? "\u001f" : $"{j.Information ?? string.Empty}\u001f{j.Reason ?? string.Empty}";

        public static string NormalizeJustification(JustificationRequest? j) =>
            j == null ? "\u001f" : $"{j.Information ?? string.Empty}\u001f{j.Reason ?? string.Empty}";

        public static string ItemsFingerprintFromEntity(IEnumerable<HistoryItemEntity>? items) =>
            string.Join(
                "|",
                (items ?? Enumerable.Empty<HistoryItemEntity>())
                    .Select(x => $"{x.Id}\u001f{x.Performed}\u001f{x.OrderBy?.ToString() ?? string.Empty}")
                    .OrderBy(x => x, StringComparer.Ordinal));

        public static string ItemsFingerprintFromRequest(IEnumerable<HistoryItemRequest>? items) =>
            string.Join(
                "|",
                (items ?? Enumerable.Empty<HistoryItemRequest>())
                    .Select(x => $"{x.Id}\u001f{x.Performed}\u001f{x.OrderBy?.ToString() ?? string.Empty}")
                    .OrderBy(x => x, StringComparer.Ordinal));

        private static long TruncateToUnixSeconds(DateTime value)
        {
            var utc = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
            return new DateTimeOffset(utc).ToUnixTimeSeconds();
        }
    }
}
