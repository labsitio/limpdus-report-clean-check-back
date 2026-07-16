using System.ComponentModel.DataAnnotations;

namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class LoginRequest
    {
        public string Type { get; set; } = "auto";

        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
