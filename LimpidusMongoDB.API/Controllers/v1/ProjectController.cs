using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]/")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAreaActivityService _areaActivityService;

        public ProjectController(
            IProjectService projectService,
            IAreaActivityService areaActivityService)
        {
            _projectService = projectService;
            _areaActivityService = areaActivityService;
        }

        //TODO: Quando subir para um servidor, ajustar o remarks removendo a frase "(local por enquanto)".
        //TODO: Quando for criar os Posts, colocar uma regra para inserir os Ids dos Projects nos Employees.

        /// <summary>
        /// GET para obter todos os projetos.
        /// </summary>
        /// <remarks>
        /// Exemplo (local por enquanto) de requisição para obter todos os projetos:
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

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter projeto por id legado
        /// </summary>
        /// <param name="legacyId">Id legado do projeto</param>
        /// <remarks>
        /// Exemplo de requisição para obter projeto por id:
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
            var result = await _projectService.GetByLegacyIdAsync(legacyId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter areas vinculadas ao funcionario
        /// </summary>
        /// <param name="legacyId">Id legado do projeto</param>
        /// <param name="employeeId">Id do funcionario</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição para obter areas vinculadas ao funcionario:
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
            CancellationToken cancellationToken)
        {
            var result = await _areaActivityService.GetByProjectIdAndEmployeeIdAsync(legacyId, employeeId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter projeto por id
        /// </summary>
        /// <param name="id">Id do projeto</param>
        /// <remarks>
        /// Exemplo de requisição para obter projeto por id:
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

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET para obter áreas e atividades relacionadas ao projeto
        /// </summary>
        /// <param name="id">Id do projeto</param>
        /// <remarks>
        /// Exemplo de requisição para obter areas e atividades do projeto:
        /// 
        ///     Request:
        ///     GET /v1/Project/{id}/AreaActivity
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "data": [
        ///         {
        ///           "id": "...",
        ///           "name": "Cozinha",
        ///           "description": "Responsável pela limpeza completa da cozinha.",
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
        public async Task<IActionResult> GetAreaActivitesByProject([FromRoute] int id)
        {
            var result = await _areaActivityService.GetByProjectIdAsync(id);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// POST para criar ou atualizar projeto
        /// </summary>
        /// <param name="request">Objeto com dados do projeto</param>
        /// <remarks>
        /// Exemplo de requisição para salvar projeto:
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