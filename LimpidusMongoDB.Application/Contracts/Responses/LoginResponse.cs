namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class AllowedProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsFranqueado { get; set; }
        public bool IsAdmin { get; set; }
        public int? FranqId { get; set; }
        public int IdProjeto { get; set; }
        public string Nome { get; set; } = string.Empty;
        public IReadOnlyList<AllowedProjectResponse> AllowedProjects { get; set; } = Array.Empty<AllowedProjectResponse>();
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Teto efetivo de dias no histórico para o usuário atual.
        /// Admin: null (sem limite). Franqueado/Consultor: 365. ProjectViewer: override do projeto ou 90.
        /// </summary>
        public int? MaxHistoryRangeDays { get; set; }
    }
}
