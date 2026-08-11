namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class SetClientAccessRequest
    {
        /// <summary>Override de dias do ProjectViewer. Null = voltar ao default 90.</summary>
        public int? MaxHistoryRangeDays { get; set; }

        /// <summary>Se omitido, mantém o valor atual.</summary>
        public bool? ShowUnperformedActivitiesToClient { get; set; }

        /// <summary>Se omitido, mantém o valor atual.</summary>
        public bool? AllowExcelExport { get; set; }
    }
}
