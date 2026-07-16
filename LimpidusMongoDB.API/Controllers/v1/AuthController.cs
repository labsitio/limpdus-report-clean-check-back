using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [AllowAnonymous]
    public class AuthController : BaseV1Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Login unificado: type = "franqueado" (FRANQ_LOGIN + MD5) ou "project" (WORK_HEADER LOGIN/SENHA).
        /// Retorna JWT com role Franqueado | Admin | ProjectViewer.
        /// </summary>
        [HttpPost("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(LoginResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Atalho para login de franqueado (equivalente a type=franqueado).
        /// </summary>
        [HttpPost("franqueado")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(LoginResponse))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> LoginFranqueado([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            request.Type = "franqueado";
            return await Login(request, cancellationToken);
        }

        /// <summary>
        /// Atalho para login de projeto (viewer restrito). Preferido para app mobile de campo.
        /// </summary>
        [HttpPost("project")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(LoginResponse))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> LoginProject([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            request.Type = "project";
            return await Login(request, cancellationToken);
        }
    }
}
