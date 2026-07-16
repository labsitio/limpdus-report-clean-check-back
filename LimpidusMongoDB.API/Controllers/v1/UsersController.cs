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
    /// <summary>
    /// Gestão de franqueados: listar e marcar/desmarcar Admin (GRUPOS_USER grupo 1).
    /// </summary>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public class UsersController : BaseV1Controller
    {
        private readonly IUserManagementService _userManagement;

        public UsersController(IUserManagementService userManagement) =>
            _userManagement = userManagement;

        /// <summary>
        /// Lista franqueados ativos (FRANQ_LOGIN) com flag IsAdmin (GRUPOS_USER grupo 1).
        /// </summary>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<FranqueadoUserResponse>))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var result = await _userManagement.ListFranqueadosAsync(cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Promove ou remove Admin (INSERT/DELETE em GRUPOS_USER com ID_TBLGRUPOS = 1).
        /// Não altera outros grupos do franqueado.
        /// </summary>
        [HttpPut("{franqId:int}/admin")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(FranqueadoUserResponse))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetAdmin(
            int franqId,
            [FromBody] SetAdminRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Body obrigatório." });

            var result = await _userManagement.SetAdminAsync(User, franqId, request.IsAdmin, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
