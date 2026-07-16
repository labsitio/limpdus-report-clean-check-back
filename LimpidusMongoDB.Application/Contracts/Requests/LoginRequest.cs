using System.ComponentModel.DataAnnotations;

namespace LimpidusMongoDB.Application.Contracts.Requests
{
    public class LoginRequest
    {
        /// <summary>
        /// "franqueado" (FRANQ_LOGIN) ou "project" (WORK_HEADER LOGIN/SENHA).
        /// </summary>
        [Required]
        public string Type { get; set; } = "project";

        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
