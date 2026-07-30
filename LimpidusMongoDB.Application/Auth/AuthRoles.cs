namespace LimpidusMongoDB.Application.Auth
{
    public static class AuthRoles
    {
        public const string Franqueado = "Franqueado";
        public const string Consultor = "Consultor";
        public const string Admin = "Admin";
        public const string ProjectViewer = "ProjectViewer";

        /// <summary>Franqueado + Consultor + Admin (export / relatório completo).</summary>
        public const string FranqueadoOrAdmin = Franqueado + "," + Consultor + "," + Admin;
        public const string AnyAuthenticated = Franqueado + "," + Consultor + "," + Admin + "," + ProjectViewer;
    }

    public static class AuthClaims
    {
        public const string FranqId = "franqId";
        public const string LegacyProjectId = "legacyProjectId";
        public const string IsFranqueado = "isFranqueado";
        public const string AllowedProjectIds = "allowedProjectIds";
        public const string DisplayName = "displayName";
    }

    public static class AuthPolicies
    {
        public const string CanExportReports = "CanExportReports";
        public const string AdminOnly = "AdminOnly";
    }

    public static class AuthLoginTypes
    {
        public const string Auto = "auto";
        public const string Franqueado = "franqueado";
        public const string Project = "project";
    }
}
