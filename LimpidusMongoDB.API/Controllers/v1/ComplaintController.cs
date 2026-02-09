using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    /// <summary>
    /// Complaint / issues found API. Used by React frontend (route with query params a=legacyProjectId, b=legacyAreaId, sent=1).
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly ILogger<ComplaintController> _logger;

        public ComplaintController(IComplaintService complaintService, ILogger<ComplaintController> logger)
        {
            _complaintService = complaintService;
            _logger = logger;
        }

        /// <summary>
        /// Returns project name and area name for display on the complaint form.
        /// </summary>
        /// <param name="legacyProjectId">Legacy project ID (WORK_HEADER_ID). Maps to query param 'a' on frontend URL.</param>
        /// <param name="legacyAreaId">Legacy area ID (WORK_AREA_ID). Maps to query param 'b' on frontend URL.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("data")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ComplaintDataResponse), description: "Project and area data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, description: "Project or area not found")]
        public async Task<IActionResult> GetData(
            [FromQuery] int legacyProjectId,
            [FromQuery] int legacyAreaId,
            CancellationToken cancellationToken = default)
        {
            var data = await _complaintService.GetDataAsync(legacyProjectId, legacyAreaId, cancellationToken);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        /// <summary>
        /// Receives the complaint form, gets recipients from DB, normalizes emails and sends one email per recipient (max 20).
        /// </summary>
        /// <remarks>
        /// On 200 with sent=false and reason "no_recipients", frontend should show thank-you screen and redirect with sent=1.
        /// </remarks>
        [HttpPost("send")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ComplaintSendResponse), description: "Sent successfully or no recipients")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, description: "Invalid legacyProjectId or legacyAreaId")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: "Internal error")]
        public async Task<IActionResult> Send(
            [FromBody] ComplaintSendRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                return BadRequest();

            if (request.LegacyProjectId <= 0 || request.LegacyAreaId <= 0)
            {
                return BadRequest(new { message = "legacyProjectId and legacyAreaId must be greater than zero." });
            }

            try
            {
                var response = await _complaintService.SendAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending complaint. LegacyProjectId={LegacyProjectId}, LegacyAreaId={LegacyAreaId}", request.LegacyProjectId, request.LegacyAreaId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while processing the complaint." });
            }
        }
    }
}
