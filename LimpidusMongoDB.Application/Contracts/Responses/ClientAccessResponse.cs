namespace LimpidusMongoDB.Application.Contracts.Responses
{
    /// <summary>
    /// Configuração de acesso do ProjectViewer (cliente) por projeto.
    /// </summary>
    public class ClientAccessResponse
    {
        public int LegacyId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>Override de dias (null = default 90).</summary>
        public int? MaxHistoryRangeDays { get; set; }

        public int DefaultProjectViewerDays { get; set; }

        /// <summary>Teto efetivo do cliente: override ?? 90.</summary>
        public int EffectiveMaxDays { get; set; }

        /// <summary>
        /// Se true, o cliente também vê atividades/áreas não realizadas.
        /// Default false: só o que foi feito.
        /// </summary>
        public bool ShowUnperformedActivitiesToClient { get; set; }

        /// <summary>Se o ProjectViewer pode exportar Excel.</summary>
        public bool AllowExcelExport { get; set; }
    }
}
