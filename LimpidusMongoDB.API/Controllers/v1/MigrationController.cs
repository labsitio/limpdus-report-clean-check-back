using System.Net;
using LimpidusMongoDB.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LimpidusMongoDB.Api.Controllers.v1
{
    [ApiController]
    [Route("v1/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly IMigrationService _migrationService;
        private readonly IConfiguration _configuration;

        public MigrationController(IMigrationService migrationService, IConfiguration configuration)
        {
            _migrationService = migrationService;
            _configuration = configuration;
        }

        /// <summary>
        /// Migra dados do SQL Server (sistema WebForms) para o banco limpidus
        /// </summary>
        /// <param name="legacyProjectId">ID do projeto no sistema legado (WORK_HEADER_ID)</param>
        /// <param name="sqlServerConnectionString">String de conexão do SQL Server (opcional, usa do appsettings se não informado)</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /v1/Migration/from-sqlserver?legacyProjectId=4698
        ///     
        /// A connection string é obtida automaticamente do appsettings.json (SqlServerDB) se não informada
        /// </remarks>
        [HttpPost("from-sqlserver")]
        [SwaggerResponse((int)HttpStatusCode.OK, description: "Migração realizada com sucesso")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MigrateFromSqlServer(
            [FromQuery] int legacyProjectId,
            [FromQuery] string? sqlServerConnectionString = null,
            CancellationToken cancellationToken = default)
        {
            // Se não informou connection string, tenta pegar do appsettings
            if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
            {
                sqlServerConnectionString = _configuration.GetConnectionString("SqlServerDB");
                
                if (string.IsNullOrWhiteSpace(sqlServerConnectionString))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Connection string do SQL Server não encontrada. Informe via parâmetro ou configure 'SqlServerDB' no appsettings.json"
                    });
                }
            }

            var result = await _migrationService.MigrateFromSqlServerAsync(
                legacyProjectId,
                sqlServerConnectionString,
                cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
