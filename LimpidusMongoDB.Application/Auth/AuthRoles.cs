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

    /// <summary>
    /// Códigos estáveis de falha de autenticação. O front usa o código (não a mensagem)
    /// para escolher o texto traduzido; a mensagem serve como fallback seguro.
    /// </summary>
    public static class AuthErrorCodes
    {
        /// <summary>Requisição malformada (campos obrigatórios ausentes, type inválido). HTTP 400.</summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>Login/senha não conferem. HTTP 401.</summary>
        public const string InvalidCredentials = "invalid_credentials";

        /// <summary>SQL/Mongo/configuração indisponível. HTTP 503.</summary>
        public const string ServiceUnavailable = "auth_service_unavailable";

        /// <summary>Qualquer outra falha não prevista. HTTP 500.</summary>
        public const string Unexpected = "unexpected_error";
    }

    /// <summary>
    /// Mensagens devolvidas ao usuário final. Nunca incluem detalhe de infraestrutura.
    /// </summary>
    public static class AuthErrorMessages
    {
        public const string InvalidRequest = "Login e senha são obrigatórios.";
        public const string InvalidLoginType = "Tipo de login inválido. Use auto, franqueado ou project.";
        public const string InvalidCredentials = "Usuário ou senha inválidos.";
        public const string ServiceUnavailable = "Não foi possível acessar o serviço de autenticação. Tente novamente em alguns instantes.";
        public const string Unexpected = "Não foi possível concluir o login. Tente novamente.";
    }
}
