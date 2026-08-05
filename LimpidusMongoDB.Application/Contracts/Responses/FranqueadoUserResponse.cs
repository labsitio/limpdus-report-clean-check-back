using LimpidusMongoDB.Application.Auth;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class FranqueadoUserResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsFranqueado { get; set; } = true;
        public bool IsConsultor { get; set; }
        /// <summary>Admin | Consultor | Franqueado — alinhado ao login.</summary>
        public string Role { get; set; } = AuthRoles.Franqueado;
        public int? NivelId { get; set; }
        public string NivelNome { get; set; } = string.Empty;
        public int? NivelGrupoId { get; set; }
        /// <summary>Grupos da intranet (TBL_GRUPOS), separados por vírgula.</summary>
        public string Grupos { get; set; } = string.Empty;
    }
}
