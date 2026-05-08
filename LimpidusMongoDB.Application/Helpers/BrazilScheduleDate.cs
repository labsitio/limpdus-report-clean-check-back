namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Data de calendário para regras de agenda (áreas/tarefas por dia da semana) no fuso Brasil.
    /// </summary>
    public static class BrazilScheduleDate
    {
        public static readonly TimeZoneInfo SaoPauloTimeZone = ResolveSaoPauloTimeZone();

        /// <summary>Data local (00:00) em America/Sao_Paulo correspondente ao instante atual em UTC.</summary>
        public static DateTime TodayInSaoPaulo() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SaoPauloTimeZone).Date;

        private static TimeZoneInfo ResolveSaoPauloTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
        }
    }
}
