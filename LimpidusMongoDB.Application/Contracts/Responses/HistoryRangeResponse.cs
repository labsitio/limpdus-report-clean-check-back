namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class HistoryRangeResponse
    {
        public int LegacyId { get; set; }

        /// <summary>Override persistido no Mongo (null = usar default do ProjectViewer).</summary>
        public int? MaxHistoryRangeDays { get; set; }

        /// <summary>Default aplicado ao cliente quando não há override.</summary>
        public int DefaultProjectViewerDays { get; set; }

        /// <summary>
        /// Teto efetivo para o usuário autenticado.
        /// Admin: null (sem limite). Franqueado/Consultor: 365. ProjectViewer: override ?? 90.
        /// </summary>
        public int? EffectiveMaxDays { get; set; }
    }
}
