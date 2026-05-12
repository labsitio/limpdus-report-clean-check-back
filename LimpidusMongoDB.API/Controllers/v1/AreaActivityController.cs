using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AreaActivityController : ControllerBase
    {
        private readonly IAreaActivityService _areaActivityService;

        public AreaActivityController(IAreaActivityService areaActivityService)
        {
            _areaActivityService = areaActivityService;
        }

        /// <summary>
        /// GET para obter itens da area
        /// </summary>
        /// <param name="id">Id da area</param>
        /// <param name="referenceDate">Opcional. Se omitido, usa hoje (America/Sao_Paulo). Filtra itens por weekDays.</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para obter itens da area:
        /// 
        ///     Request:
        ///     GET /v1/AreaActivity/{id}/Items
        ///     
        ///     Response:
        ///     [
        ///       {
        ///         "id": "string",
        ///         "name": "string",
        ///         "orderBy": 0,
        ///         "frequency": {
        ///           "type": "string",
        ///           "weekDays": [
        ///             0
        ///           ]
        ///         }
        ///       }
        ///     ]
        /// </remarks>
        [HttpGet("{id}/Items")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<AreaActivityItemResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetItemsByAreaId(
            [FromRoute] string id,
            [FromQuery] DateTime? referenceDate,
            CancellationToken cancellationToken)
        {
            var scheduleDate = referenceDate?.Date ?? BrazilScheduleDate.TodayInSaoPaulo();
            var result = await _areaActivityService.GetItemsByAreaIsAsync(id, scheduleDate, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// POST para gravar áreas (upsert). Com **mais de um** objeto no body, remove do Mongo áreas do projeto que não vieram na lista
        /// (com um único <c>employeeId</c> no array, a remoção limita-se a esse funcionário; com vários <c>employeeId</c>, ao projeto inteiro).
        /// Com **apenas um** objeto no body **não** há remoção em massa — só grava/atualiza esse documento (evita apagar outras áreas ao salvar uma).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para salvar areas:
        /// 
        ///     Request:
        ///     POST /v1/AreaActivity
        ///     
        ///     Body:
        ///     [
        ///       {
        ///         "id": "string",
        ///         "name": "string",
        ///         "description": "string",
        ///         "quickTask": true,
        ///         "totalM2": 0,
        ///         "employeeId": "string",
        ///         "headerId": "string",
        ///         "orderBy": 0,
        ///         "frequency": {
        ///           "type": "string",
        ///           "weekDays": [
        ///             0
        ///           ]
        ///         },
        ///         "items": [
        ///           {
        ///             "id": "string",
        ///             "name": "string",
        ///             "orderBy": 0,
        ///             "frequency": {
        ///               "type": "string",
        ///               "weekDays": [
        ///                 0
        ///               ]
        ///             }
        ///           }
        ///         ],
        ///         "projectId": 0
        ///       }
        ///     ]
        /// </remarks>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(AreaActivityResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveAreaActivities(IEnumerable<AreaActivityRequest> request, CancellationToken cancellationToken)
        {
            var result = await _areaActivityService.SaveAsync(request, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
