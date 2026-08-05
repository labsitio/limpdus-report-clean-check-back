using System.Net;
using LimpidusMongoDB.Application.Auth;
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
        /// Login unificado. type omitido/auto: tenta FRANQ_LOGIN e depois WORK_HEADER.
        /// type franqueado ou project forca um caminho. Retorna JWT Franqueado | Consultor | Admin | ProjectViewer.
        /// </summary>
        [HttpPost("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(LoginResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (result.Success)
                return Ok(result);

            // Só credencial inválida é 401; indisponibilidade de SQL/Mongo não pode
            // se passar por erro de senha.
            var status = result.Code switch
            {
                AuthErrorCodes.InvalidRequest => HttpStatusCode.BadRequest,
                AuthErrorCodes.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                AuthErrorCodes.Unexpected => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.Unauthorized
            };

            return StatusCode((int)status, result);
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
