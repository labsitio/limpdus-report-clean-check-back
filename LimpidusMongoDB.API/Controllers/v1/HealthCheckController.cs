using LimpidusMongoDB.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace LimpidusMongoDB.Api.Controllers.v1
{

    [ApiController]
    [Route("v1/[controller]/")]
    public class HealthCheckController : ControllerBase
    {
        //TODO: Quando subir para um servidor, ajustar o remarks removendo a frase "(local por enquanto)".
        //TODO: Quando for pra valer, criar camadas para testar mesmo os serviços ao invés de ser um endpoint mockado.

        /// <summary>
        /// GET para checar a saúde das aplicações.
        /// </summary>
        /// <remarks>
        /// Exemplo (local por enquanto) de requisição para checar a saúde das aplicações:
        /// 
        ///     Request:
        ///     GET /v1/HealthCheck
        ///     
        ///     Response:        
        ///     {
        ///       "success": true,
        ///       "message": "Service Online"
        ///      }      
        /// </remarks>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        public IActionResult GetHealthCheck()
        {
            return Ok(Result.Ok(message: "Service Online"));
        }
    }
}