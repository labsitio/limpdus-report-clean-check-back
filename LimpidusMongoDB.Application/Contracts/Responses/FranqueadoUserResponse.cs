namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class FranqueadoUserResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsFranqueado { get; set; } = true;
    }
}
