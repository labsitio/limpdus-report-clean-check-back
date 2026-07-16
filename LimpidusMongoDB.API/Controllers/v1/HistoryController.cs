using System.Net;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using HistoryListResponse = LimpidusMongoDB.Application.Services.Interfaces.HistoryListResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [Authorize]
    public class HistoryController : BaseV1Controller
    {
        private readonly IHistoryService _historyService;
        private readonly IProjectAccessService _projectAccess;

        public HistoryController(IHistoryService historyService, IProjectAccessService projectAccess)
        {
            _historyService = historyService;
            _projectAccess = projectAccess;
        }

        /// <summary>
        /// GET Buscar historico do funcionario
        /// </summary>
        [HttpGet("legacyProjectId/{legacyId}/employee/{employeeId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<HistoryResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesByProjectAndEmployee(
            [FromRoute] int legacyId,
            [FromRoute] string employeeId,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            var result = await _historyService.GetByProjectIdAndEmployeeIdAsync(legacyId, employeeId, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            if (!_projectAccess.CanSeeSensitiveHistory(User) && result.Data is IEnumerable<HistoryResponse> items)
            {
                foreach (var item in items)
                    item.Justification = null;
            }

            return Ok(result);
        }

        /// <summary>
        /// GET Buscar historico do projeto (lista audit / relatório web)
        /// </summary>
        [HttpGet("legacyProjectId/{legacyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<HistoryResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesByProject(
            [FromRoute] int legacyId,
            [FromQuery] HistoryQueryRequest request,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            ApplyFranqueadoCompletedOnlyRule(request);

            var result = await _historyService.GetByProjectIdAsync(legacyId, request, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            if (!_projectAccess.CanSeeSensitiveHistory(User) && result.Data is HistoryListResponse list && list.Data != null)
            {
                foreach (var row in list.Data)
                    row.Justification = null;
            }

            return Ok(result);
        }

        /// <summary>
        /// GET exportar historico em planilha (somente Franqueado / Admin)
        /// </summary>
        [Authorize(Policy = AuthPolicies.CanExportReports)]
        [HttpGet("export/legacyProjectId/{legacyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesInSpreadsheet(
            [FromRoute] int legacyId,
            [FromQuery] HistoryQueryRequest request,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            if (!_projectAccess.CanExport(User))
                return Forbid();

            ApplyFranqueadoCompletedOnlyRule(request);

            var result = await _historyService.GetHistoriesInSpreadsheet(legacyId, request, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            var spreadsheet = (string)result.Data;
            byte[] fileData = System.IO.File.ReadAllBytes(spreadsheet);

            return File(fileData, System.Net.Mime.MediaTypeNames.Application.Octet, spreadsheet);
        }

        /// <summary>
        /// POST para salvar historico do funcionario (mobile / operacional)
        /// </summary>
        [HttpPost("legacyProjectId/{legacyId}/employee/{employeeId}")]
        [SwaggerResponse((int)HttpStatusCode.Created)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveHistory(
            [FromRoute] int legacyId,
            [FromRoute] string employeeId,
            [FromBody] IEnumerable<HistoryRequest> requests,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            if (requests != null && requests.Any(r => r.ProjectId != 0 && r.ProjectId != legacyId))
                return Forbid();

            var result = await _historyService.SaveAsync(requests, cancellationToken);

            return result.Success ? Created(Request.Path, result) : BadRequest(result);
        }

        private void ApplyFranqueadoCompletedOnlyRule(HistoryQueryRequest request)
        {
            if (_projectAccess.IsFranqueado(User))
                request.Status = true;
        }
    }
}
