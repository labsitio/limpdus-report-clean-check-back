namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class SetClientAccessRequest
    {
        /// <summary>Override de dias do ProjectViewer. Null = voltar ao default 90.</summary>
        public int? MaxHistoryRangeDays { get; set; }

        /// <summary>Se omitido, mantém o valor atual.</summary>
        public bool? ShowActivitiesToClient { get; set; }

        /// <summary>Se omitido, mantém o valor atual.</summary>
        public bool? AllowExcelExport { get; set; }

        /// <summary>
        /// ItemIds visíveis ao cliente. Null = todas.
        /// Envie lista vazia para nenhuma atividade visível.
        /// Se omitido (campo ausente via merge), o service trata null como "todas" quando o body envia explicitamente null.
        /// </summary>
        public List<string>? ClientVisibleActivityItemIds { get; set; }

        /// <summary>
        /// Quando true, aplica ClientVisibleActivityItemIds mesmo se null (todas).
        /// Front sempre envia true ao salvar a seção de atividades.
        /// </summary>
        public bool UpdateVisibleActivities { get; set; } = true;
    }
}
