namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class ClientActivityOptionResponse
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

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

        /// <summary>Se o cliente vê atividades no expand do histórico.</summary>
        public bool ShowActivitiesToClient { get; set; } = true;

        /// <summary>Null = todas as atividades do catálogo.</summary>
        public List<string>? ClientVisibleActivityItemIds { get; set; }

        /// <summary>Se o ProjectViewer pode exportar Excel.</summary>
        public bool AllowExcelExport { get; set; }

        /// <summary>Catálogo único de atividades do projeto (AreaActivity items).</summary>
        public List<ClientActivityOptionResponse> AvailableActivities { get; set; } = new();
    }
}
