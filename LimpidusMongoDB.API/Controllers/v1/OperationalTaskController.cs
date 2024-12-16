using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]/")]
    public class OperationalTaskController : ControllerBase
    {
        private readonly IOperationalTaskService _operationalTaskService;

        public OperationalTaskController(IOperationalTaskService operationalTaskService) => _operationalTaskService = operationalTaskService;

        //TODO: Quando subir para um servidor, ajustar o remarks removendo a frase "(local por enquanto)".
        //TODO: Quando for criar os Posts, colocar uma regra para inserir os Ids das OperationalTasks nos Items.

        /// <summary>
        /// GET para obter todas as tarefas operacionais.
        /// </summary>
        /// <remarks>
        /// Exemplo (local por enquanto) de requisição para obter as tarefas operacionais:
        /// 
        ///     Request:
        ///     GET /v1/OperationalTask
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "data": [ 
        ///         {
        ///            "id": "6579ff3ef6fcb18398bf9790",
        ///            "legacyId": "853",
        ///            "name": "Esvaziar lixo (cozinha)",
        ///            "description": "Esvaziar todos os cestos de lixo de cozinha",
        ///            "quickTask": true,
        ///            "employeeId": 1,
        ///            "legacyProjectId": "4698",
        ///            "orderBy": 1,
        ///            "items": [
        ///             {
        ///                 "legacyId": "3685",
        ///                 "name": "Copa",
        ///                 "orderBy": 0
        ///             },
        ///             {
        ///                 "legacyId": "53686",
        ///                 "name": "Copa 2",
        ///                 "orderBy": 1
        ///             }
        ///            ]
        ///             },
        ///             {
        ///            "id": "6579ff3ef6fcb18398bf9791",
        ///            "legacyId": "854",
        ///            "name": "Limpar Janelas",
        ///            "description": "limpar todas as janelas",
        ///            "quickTask": true,
        ///            "employeeId": 1,
        ///            "legacyProjectId": "4698",
        ///            "orderBy": 3,
        ///            "items": [
        ///             {
        ///                 "legacyId": "53685",
        ///                 "name": "Copa",
        ///                 "orderBy": 0
        ///             },
        ///             {
        ///                 "legacyId": "53686",
        ///                 "name": "Copa 2",
        ///                 "orderBy": 1
        ///             },
        ///             {
        ///                 "legacyId": "53687",
        ///                 "name": "Copa 3",
        ///                 "orderBy": 2
        ///             }
        ///            ]
        ///             },
        ///             .
        ///             .
        ///             .
        ///        ]
        ///      }      
        /// </remarks>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAllOperationalTasks()
        {
            var result = await _operationalTaskService.GetAllOperationalTasks();

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}