namespace LimpidusMongoDB.Application.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "limpidus-clean-check";
        public string Audience { get; set; } = "limpidus-clean-check";
        public int ExpirationHours { get; set; } = 12;
    }
}
