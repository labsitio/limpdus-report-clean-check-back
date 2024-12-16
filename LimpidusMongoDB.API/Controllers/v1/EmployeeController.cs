using System.Net;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// POST para criar ou atualizar funcionario
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        /// Exemplo de requisição para salvar funcionario:
        /// 
        ///     Request:
        ///     POST /v1/Employee
        /// 
        ///     Body:
        ///     {
        ///       "id": "string",
        ///       "legacyId": 0,
        ///       "firstName": "string",
        ///       "lastName": "string",
        ///       "number": 0,
        ///       "observation": "string",
        ///       "projectId": "string"
        ///     }
        ///     
        ///     Response:
        ///     {
        ///       "success": true,
        ///       "data": "{id-funcionario}"
        ///     }
        /// </remarks>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveProject([FromBody] EmployeeRequest request)
        {
            var result = await _employeeService.SaveAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
