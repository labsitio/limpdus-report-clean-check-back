using System.Net;
using LimpidusMongoDB.Application.Auth;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("v1/[controller]/")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAreaActivityService _areaActivityService;
        private readonly IProjectAccessService _projectAccess;

        public ProjectController(
            IProjectService projectService,
            IAreaActivityService areaActivityService,
            IProjectAccessService projectAccess)
        {
            _projectService = projectService;
            _areaActivityService = areaActivityService;
            _projectAccess = projectAccess;
        }

        //TODO: Quando subir para um servidor, ajustar o remarks removendo a frase "(local por enquanto)".
        //TODO: Quando for criar os Posts, colocar uma regra para inserir os Ids dos Projects nos Employees.

        /// <summary>
        /// GET para obter todos os projetos.
        /// </summary>
        /// <remarks>
        /// Exemplo (local por enquanto) de requisi��o para obter todos os projetos:
        /// 
        ///     Request:
        ///     GET /v1/Project
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "data": [ 
        ///         {
        ///            "id": "6579128c17158eeb4450f9e5",
        ///            "legacyId": "4698",
        ///            "name": "Limpidus - Cardoso de Melo (CC N3)",
        ///            "totalM2": 450,
        ///            "daysYear": 256,
        ///            "factor": 15,
        ///            "address": "",
        ///            "contact": "",
        ///            "telephoneNumber": "",
        ///            "cellphoneNumber": "",
        ///            "registrationDate": "2021-09-08T00:00:00Z",
        ///            "employees": [
        ///              {
        ///                  "number": 1,
        ///                  "observation": ""
        ///              }
        ///            ],
        ///            "level": 3
        ///          }
        ///        ]
        ///      }      
        /// </remarks>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllProjects()
        {
            var result = await _projectService.GetAllProjects();
            if (!result.Success)
                return BadRequest(result);

            if (!_projectAccess.IsAdmin(User) && result.Data is IEnumerable<ProjectResponse> projects)
            {
                var filtered = projects
                    .Where(p => _projectAccess.CanAccessLegacyProject(User, p.LegacyId))
                    .ToList();
                return Ok(LimpidusMongoDB.Application.Contracts.Result.Ok(data: filtered));
            }

            return Ok(result);
        }

        /// <summary>
        /// GET para obter projeto por id legado
        /// </summary>
        /// <param name="legacyId">Id legado do projeto</param>
        /// <remarks>
        /// Exemplo de requisi��o para obter projeto por id:
        /// 
        ///     Request:
        ///     GET /v1/Project/legacyId/{legacyId}
        ///     
        ///     Response:
        ///     {
        ///       "success": true,
        ///       "data": {
        ///         "id": "6579128c17158eeb4450f9e5",
        ///         "legacyId": "4698",
        ///         "name": "Limpidus - Cardoso de Melo (CC N3)",
        ///         "totalM2": 450,
        ///         "daysYear": 256,
        ///         "factor": 15,
        ///         "address": "",
        ///         "contact": "",
        ///         "telephoneNumber": "",
        ///         "cellphoneNumber": "",
        ///         "registrationDate": "2021-09-08T00:00:00Z",
        ///         "employees": [
        ///           {
        ///               "number": 1,
        ///               "observation": ""
        ///           }
        ///         ],
        ///         "level": 3
        ///       }
        ///     }
        /// </remarks>
        [HttpGet("legacyId/{legacyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ProjectResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProjectByLegadyId(int legacyId)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            var result = await _projectService.GetByLegacyIdAsync(legacyId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET limite de historico do projeto (override + teto efetivo para o usuario atual).
        /// Persistencia: campo <c>maxHistoryRangeDays</c> no documento Mongo <c>project</c>.
        /// </summary>
        [HttpGet("legacyId/{legacyId}/history-range")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(HistoryRangeResponse))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetHistoryRange(
            [FromRoute] int legacyId,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            var overrideDays = await _projectService.GetMaxHistoryRangeDaysAsync(legacyId, cancellationToken);
            int? effective = null;
            if (_projectAccess.IsAdmin(User))
                effective = null;
            else if (_projectAccess.IsFranqueado(User) || _projectAccess.IsConsultor(User))
                effective = HistoryRangeLimits.FranqueadoMaxDays;
            else if (_projectAccess.IsProjectViewer(User))
                effective = HistoryRangeLimits.EffectiveProjectViewerDays(overrideDays);

            return Ok(LimpidusMongoDB.Application.Contracts.Result.Ok(data: new HistoryRangeResponse
            {
                LegacyId = legacyId,
                MaxHistoryRangeDays = overrideDays,
                DefaultProjectViewerDays = HistoryRangeLimits.ProjectViewerDefaultDays,
                EffectiveMaxDays = effective
            }));
        }

        /// <summary>
        /// PUT override do range maximo de historico do ProjectViewer neste projeto (somente Admin).
        /// Body: <c>{ "maxHistoryRangeDays": 180 }</c> ou <c>null</c> para voltar ao default 90.
        /// </summary>
        [Authorize(Policy = AuthPolicies.AdminOnly)]
        [HttpPut("legacyId/{legacyId}/history-range")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(HistoryRangeResponse))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetHistoryRange(
            [FromRoute] int legacyId,
            [FromBody] SetHistoryRangeRequest request,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.IsAdmin(User))
                return Forbid();

            var result = await _projectService.SetMaxHistoryRangeDaysAsync(
                legacyId,
                request?.MaxHistoryRangeDays,
                cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter areas vinculadas ao funcionario
        /// </summary>
        /// <param name="legacyId">Id legado do projeto</param>
        /// <param name="employeeId">Id do funcionario</param>
        /// <param name="referenceDate">Opcional. Data calendario (ex. 2026-05-09). Se omitido, usa hoje em America/Sao_Paulo. Filtra por weekDays (0=Dom…6=Sab).</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisi��o para obter areas vinculadas ao funcionario:
        /// 
        ///     Request:
        ///     GET /v1/Project/legacyId/{legacyId}/Employee/{employeeId}/AreaActivity
        ///     
        ///     Response:
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
        [HttpGet("legacyId/{legacyId}/Employee/{employeeId}/AreaActivity")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<AreaActivityResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreaActivitiesByProjectAndEmployee(
            [FromRoute] int legacyId,
            [FromRoute] string employeeId,
            [FromQuery] DateTime? referenceDate,
            CancellationToken cancellationToken)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, legacyId))
                return Forbid();

            var scheduleDate = referenceDate?.Date ?? BrazilScheduleDate.TodayInSaoPaulo();
            var result = await _areaActivityService.GetByProjectIdAndEmployeeIdAsync(legacyId, employeeId, scheduleDate, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter projeto por id
        /// </summary>
        /// <param name="id">Id do projeto</param>
        /// <remarks>
        /// Exemplo de requisi��o para obter projeto por id:
        /// 
        ///     Request:
        ///     GET /v1/Project/{id}
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "data": {
        ///         "id": "6579128c17158eeb4450f9e5",
        ///         "legacyId": "4698",
        ///         "name": "Limpidus - Cardoso de Melo (CC N3)",
        ///         "totalM2": 450,
        ///         "daysYear": 256,
        ///         "factor": 15,
        ///         "address": "",
        ///         "contact": "",
        ///         "telephoneNumber": "",
        ///         "cellphoneNumber": "",
        ///         "registrationDate": "2021-09-08T00:00:00Z",
        ///         "employees": [
        ///           {
        ///               "number": 1,
        ///               "observation": ""
        ///           }
        ///         ],
        ///         "level": 3
        ///       }
        ///     }
        /// </remarks>
        [HttpGet("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ProjectResponse))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProjectById([FromRoute] string id)
        {
            var result = await _projectService.GetByIdAsync(id);
            if (!result.Success)
                return BadRequest(result);

            if (result.Data is ProjectResponse project
                && !_projectAccess.CanAccessLegacyProject(User, project.LegacyId))
                return Forbid();

            return Ok(result);
        }

        /// <summary>
        /// GET para obter �reas e atividades relacionadas ao projeto
        /// </summary>
        /// <param name="id">Id do projeto</param>
        /// <param name="referenceDate">Opcional. Data yyyy-MM-dd. Se omitido, usa hoje em America/Sao_Paulo.</param>
        /// <remarks>
        /// Exemplo de requisi��o para obter areas e atividades do projeto:
        /// 
        ///     Request:
        ///     GET /v1/Project/{id}/AreaActivity?referenceDate=2026-05-09
        ///     
        ///     referenceDate (query): opcional. Se omitido, usa a data atual em America/Sao_Paulo. Filtra itens cuja lista weekDays inclui o dia da semana dessa data (0=Domingo..6=Sabado).
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "data": [
        ///         {
        ///           "id": "...",
        ///           "name": "Cozinha",
        ///           "description": "Respons�vel pela limpeza completa da cozinha.",
        ///           "quickTask": false,
        ///           "totalM2": 30,
        ///           "employeeId": "...",
        ///           "headerId": "Header-1",
        ///           "orderBy": 1,
        ///           "items": [
        ///             {
        ///               "id": "1",
        ///               "name": "Limpar geladeira",
        ///               "orderBy": 1,
        ///               "frequency": {
        ///                 "type": "weekly",
        ///                 "weekDays": [
        ///                   2,
        ///                   6
        ///                 ]
        ///               }
        ///             },
        ///           ],
        ///           "projectId": 1
        ///         },
        ///       ]
        ///     }
        /// </remarks>
        [HttpGet("{id}/AreaActivity")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<AreaActivityResponse>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreaActivitesByProject([FromRoute] int id, [FromQuery] DateTime? referenceDate)
        {
            if (!_projectAccess.CanAccessLegacyProject(User, id))
                return Forbid();

            var scheduleDate = referenceDate?.Date ?? BrazilScheduleDate.TodayInSaoPaulo();
            var result = await _areaActivityService.GetByProjectIdAsync(id, scheduleDate);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// POST para criar ou atualizar projeto
        /// </summary>
        /// <param name="request">Objeto com dados do projeto</param>
        /// <remarks>
        /// Exemplo de requisi��o para salvar projeto:
        /// 
        ///     Request:
        ///     POST /v1/Project
        ///     
        ///     Body:
        ///     {
        ///       "id": "string",
        ///       "legacyId": 0,
        ///       "name": "string",
        ///       "totalM2": 0,
        ///       "daysYear": 0,
        ///       "factor": 0,
        ///       "address": "string",
        ///       "contact": "string",
        ///       "telephoneNumber": "string",
        ///       "cellphoneNumber": "string",
        ///       "registrationDate": "2024-04-14T00:46:35.368Z",
        ///       "employees": [
        ///         {
        ///           "id": "string",
        ///           "legacyId": 0,
        ///           "firstName": "string",
        ///           "lastName": "string",
        ///           "number": 0,
        ///           "observation": "string",
        ///           "projectId": "string"
        ///         }
        ///       ],
        ///       "level": 0
        ///     }
        ///     
        ///     Response:
        ///     {
        ///       "success": true
        ///     }
        /// </remarks>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveProject([FromBody] ProjectRequest request)
        {
            var result = await _projectService.SaveAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
