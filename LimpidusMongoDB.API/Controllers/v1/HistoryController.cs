using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    public class HistoryController : BaseV1Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService) => _historyService = historyService;

        /// <summary>
        /// GET Buscar historico do funcionario
        /// </summary>
        /// <param name="legacyId">Id do projeto</param>
        /// <param name="employeeId">Id do funcionario</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para obter historico do funcionario:
        /// 
        ///     Request:
        ///     GET /v1/History/legacyProjectId/{legacyId}/Employee/{employeeId}
        ///     
        ///     Response:
        ///     [
        ///       {
        ///         "id": "string",
        ///         "projectId": 0,
        ///         "employeeId": "string",
        ///         "areaTaskId": "string",
        ///         "areaTaskName": "string",
        ///         "endDate": "2024-04-30T04:25:37.103Z",
        ///         "user": {
        ///           "name": "string",
        ///           "lastName": "string"
        ///         },
        ///         "items": [
        ///           {
        ///             "id": "string",
        ///             "name": "string",
        ///             "orderBy": 0,
        ///             "endDate": "2024-04-30T04:25:37.103Z",
        ///             "performed": true
        ///           }
        ///         ],
        ///         "justification": {
        ///           "information": "string",
        ///           "reason": "string"
        ///         }
        ///       }
        ///     ]
        /// </remarks>
        [HttpGet("legacyProjectId/{legacyId}/employee/{employeeId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<HistoryResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesByProjectAndEmployee(
            [FromRoute] int legacyId,
            [FromRoute] string employeeId,
            CancellationToken cancellationToken)
        {
            var result = await _historyService.GetByProjectIdAndEmployeeIdAsync(legacyId, employeeId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        /// <summary>
        /// GET Buscar historico do funcionario
        /// </summary>
        /// <param name="legacyId">Id do projeto</param>
        /// <param name="request">query para filtrar</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para obter historico do funcionario:
        /// 
        ///     Request:
        ///     GET /v1/History/legacyProjectId/{legacyId}
        ///     
        ///     Response:
        ///     [ 
        ///          {
        ///               "id": "6745cd9ebc46626ae5227f90",
        ///               "department": "Corredores",
        ///               "employee": "Rapha Teste",
        ///               "dateStart": "2024-11-26T13:31:10.042Z",
        ///               "dateEnd": "2024-11-26T13:29:28Z",
        ///               "duration": "-00:01:42.0420000",
        ///               "status": "Pendente"
        ///          }
        ///     ]
        /// </remarks>
        [HttpGet("legacyProjectId/{legacyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<HistoryResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesByProject(
            [FromRoute] int legacyId,
            [FromQuery] HistoryQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _historyService.GetByProjectIdAsync(legacyId, request, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        /// <summary>
        /// GET Buscar historico do funcionario em planilha
        /// </summary>
        /// <param name="legacyId">Id do projeto</param>
        /// <param name="request">query para filtrar</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para obter a planilha de historico dos funcionarios:
        /// 
        ///     Request:
        ///     GET /v1/History/export/legacyProjectId/{legacyId}?+filtros
        ///     
        ///     Response:
        ///     Arquivo Excel
        /// </remarks>
        [HttpGet("export/legacyProjectId/{legacyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<HistoryResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetHistoriesInSpreadsheet(
            [FromRoute] int legacyId,
            [FromQuery] HistoryQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _historyService.GetHistoriesInSpreadsheet(legacyId, request, cancellationToken);

            var spreadsheet = (string)result.Data;

            byte[] fileData = System.IO.File.ReadAllBytes(spreadsheet);

            return result.Success ? File(fileData, System.Net.Mime.MediaTypeNames.Application.Octet, spreadsheet) : BadRequest(result);
        }

        /// <summary>
        /// POST para salvar historico do funcionario
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para salvar areas:
        /// 
        ///     Request:
        ///     POST /v1/History/legacyProjectId/{legacyId}/Employee/{employeeId}
        ///     
        ///     Body:
        ///     [
        ///       {
        ///         "projectId": 0,
        ///         "employeeId": "string",
        ///         "areaTaskId": "string",
        ///         "areaTaskName": "string",
        ///         "endDate": "2024-04-30T04:20:34.159Z",
        ///         "user": {
        ///           "name": "string",
        ///           "lastName": "string"
        ///         },
        ///         "items": [
        ///           {
        ///             "id": "string",
        ///             "name": "string",
        ///             "orderBy": 0,
        ///             "endDate": "2024-04-30T04:20:34.159Z",
        ///             "performed": true
        ///           }
        ///         ],
        ///         "justification": {
        ///           "information": "string",
        ///           "reason": "string"
        ///         }
        ///       }
        ///     ]
        /// </remarks>
        [HttpPost("legacyProjectId/{legacyId}/employee/{employeeId}")]
        [SwaggerResponse((int)HttpStatusCode.Created)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveHistory([FromBody] IEnumerable<HistoryRequest> requests, CancellationToken cancellationToken)
        {
            var result = await _historyService.SaveAsync(requests, cancellationToken);

            return result.Success ? Created(Request.Path, result) : BadRequest(result);
        }
    }
}