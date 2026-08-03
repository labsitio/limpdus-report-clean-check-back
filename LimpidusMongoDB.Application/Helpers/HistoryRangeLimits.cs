namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Limites de intervalo de datas no histórico (Clean Check).
    /// Override por projeto: campo <c>maxHistoryRangeDays</c> no documento Mongo <c>project</c>
    /// (nullable). Se preenchido, sobrescreve o default do ProjectViewer naquele projeto.
    /// </summary>
    public static class HistoryRangeLimits
    {
        /// <summary>Default do cliente (ProjectViewer) quando o projeto não tem override.</summary>
        public const int ProjectViewerDefaultDays = 90;

        /// <summary>Teto para Franqueado e Consultor.</summary>
        public const int FranqueadoMaxDays = 365;

        /// <summary>
        /// Resolve o teto efetivo do cliente: override do projeto ou 90 dias.
        /// </summary>
        public static int EffectiveProjectViewerDays(int? projectOverride) =>
            projectOverride is > 0 ? projectOverride.Value : ProjectViewerDefaultDays;
    }
}
