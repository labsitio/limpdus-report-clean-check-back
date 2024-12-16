using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    public class ReportController : BaseV1Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Envia report por email
        /// </summary>
        /// <param name="legacyId">Id do projeto</param>
        /// <param name="employeeId">Id do funcionario</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("legacyProjectId/{legacyId}/employee/{employeeId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendReport(
            [FromRoute] int legacyId,
            [FromRoute] string employeeId,
            [FromForm] ReportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _reportService.SendReportAsync(request, cancellationToken);

            return result.Success ? NoContent() : BadRequest(result);
        }
    }
}
