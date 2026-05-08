using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Filtra áreas/itens pela data de referência usando weekDays (0=Domingo … 6=Sábado),
    /// alinhado a <see cref="FrequencyConverter.ConvertPeriodoToWeekDays"/>.
    /// </summary>
    public static class AreaActivityScheduleFilter
    {
        private static readonly StringComparison Cmp = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Retorna cópias das áreas contendo apenas itens (tarefas) que devem aparecer no dia de <paramref name="referenceDate"/>.
        /// Áreas sem nenhum item após o filtro são omitidas.
        /// </summary>
        public static IEnumerable<AreaActivityResponse> FilterAreasByReferenceDate(
            IEnumerable<AreaActivityResponse> areas,
            DateTime referenceDate)
        {
            var day = (short)referenceDate.Date.DayOfWeek;

            foreach (var area in areas)
            {
                if (!FrequencyAllowsDay(area.Frequency, day))
                    continue;

                var items = area.Items?
                    .Where(item => FrequencyAllowsDay(item.Frequency, day))
                    .ToList();

                if (items == null || items.Count == 0)
                    continue;

                yield return CloneArea(area, items);
            }
        }

        private static AreaActivityResponse CloneArea(AreaActivityResponse area, List<AreaActivityItemResponse> items) =>
            new()
            {
                Id = area.Id,
                Name = area.Name,
                Description = area.Description,
                QuickTask = area.QuickTask,
                TotalM2 = area.TotalM2,
                EmployeeId = area.EmployeeId,
                HeaderId = area.HeaderId,
                OrderBy = area.OrderBy,
                Frequency = area.Frequency,
                Items = items,
                ProjectId = area.ProjectId,
            };

        /// <summary>
        /// true = item/área entra no dia <paramref name="day"/> (0..6).
        /// </summary>
        public static bool FrequencyAllowsDay(AreaActivityFrequencyResponse? frequency, short day) =>
            frequency == null || FrequencyAllowsDay(frequency.Type, frequency.WeekDays, day);

        public static bool FrequencyAllowsDay(AreaActivityFrequencyEntity? frequency, short day) =>
            frequency == null || FrequencyAllowsDay(frequency.Type, frequency.WeekDays, day);

        public static bool FrequencyAllowsDay(string? type, IEnumerable<short>? weekDays, short day)
        {
            if (IsEverydayType(type))
                return true;

            var days = weekDays?.ToList();
            if (days == null || days.Count == 0)
                return true;

            return days.Contains(day);
        }

        private static bool IsEverydayType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return false;

            var t = type.Trim();
            return t.Equals("everyday", Cmp)
                   || t.Equals("daily", Cmp)
                   || t.Equals("365", Cmp)
                   || t.Contains("diário", Cmp)
                   || t.Contains("diario", Cmp)
                   || t.Contains("todos", Cmp);
        }
    }
}
