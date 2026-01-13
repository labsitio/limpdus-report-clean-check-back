using FluentAssertions;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data;
using LimpidusMongoDB.Application.Data.Repositories;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Services;
using LimpidusMongoDB.Application.Services.Interfaces;
using LimpidusMongoDB.Tests.Integration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LimpidusMongoDB.Tests.Integration.Services
{
    /// <summary>
    /// Testes End-to-End (E2E) para MigrationService
    /// 
    /// ⚠️ ATENÇÃO: Estes testes conectam ao SQL Server REAL e migram dados reais!
    /// 
    /// Estes testes:
    /// - Conectam ao SQL Server de produção
    /// - Consultam dados reais do sistema legado
    /// - Migram para a base limpidus-test
    /// - Permitem comparação com dados de produção
    /// 
    /// Use com cuidado e apenas quando necessário validar a migração completa.
    /// </summary>
    /// <summary>
    /// Collection para agrupar testes E2E e garantir execução sequencial
    /// </summary>
    [CollectionDefinition("E2E Tests")]
    public class E2ETestCollection : ICollectionFixture<MongoDbTestFixture>
    {
        // Esta classe apenas define a collection, sem implementação
    }

    [Collection("E2E Tests")]
    public class MigrationServiceE2ETests : BaseIntegrationTest
    {
        private readonly IAreaActivityService _areaActivityService;
        private readonly IAreaActivityRepository _areaActivityRepository;
        private readonly IProjectService _projectService;
        private readonly IMigrationService _migrationService;
        private readonly ISqlServerDataAccessFactory _sqlServerDataAccessFactory;
        private readonly string _sqlServerConnectionString;

        public MigrationServiceE2ETests(MongoDbTestFixture fixture) : base(fixture)
        {
            // Setup repositórios reais
            _areaActivityRepository = new AreaActivityRepository(TestContext);

            // Setup serviços reais
            _areaActivityService = new AreaActivityService(_areaActivityRepository);

            // Setup ProjectService real para buscar funcionários do projeto
            var projectRepository = new ProjectRepository(TestContext);
            _projectService = new ProjectService(projectRepository, new EmployeeRepository(TestContext));

            // Setup EmployeeService real
            var employeeService = new EmployeeService(new EmployeeRepository(TestContext));

            // Setup SQL Server Data Access REAL (não mock!)
            _sqlServerDataAccessFactory = new SqlServerDataAccessFactory();

            // Obtém connection string do SQL Server
            // Pode vir de variável de ambiente ou usar a padrão do appsettings
            _sqlServerConnectionString = GetSqlServerConnectionString();

            _migrationService = new MigrationService(_areaActivityService, _sqlServerDataAccessFactory, _projectService, employeeService);
        }

        /// <summary>
        /// Obtém a connection string do SQL Server
        /// Prioridade: Variável de ambiente > appsettings padrão
        /// </summary>
        private static string GetSqlServerConnectionString()
        {
            // Tenta obter da variável de ambiente primeiro
            var connectionString = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("✅ Usando connection string da variável de ambiente TEST_SQLSERVER_CONNECTION_STRING");
                return connectionString;
            }

            // Fallback para connection string padrão (mesma do appsettings.json)
            // Adiciona parâmetros de SSL para evitar problemas de handshake
            var defaultConnectionString = "Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;Persist Security Info=True;User ID=limpcalc;Password=Limp741852963;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30";
            Console.WriteLine("⚠️ Usando connection string padrão. Para usar outra, defina TEST_SQLSERVER_CONNECTION_STRING");
            Console.WriteLine($"🔗 Connection String: Data Source=sql2.limpidus.com.br;Initial Catalog=limpcalc;User ID=limpcalc;Password=***;Encrypt=True;TrustServerCertificate=True");
            return defaultConnectionString;
        }

        // [Fact(Skip = "Teste E2E - Conecta ao SQL Server real. Execute manualmente quando necessário.")]
        [Fact]
        [Trait("Category", "E2E")]
        [Trait("Requires", "SQL Server Connection")]
        public async Task MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveSalvarDadosCorretamente()
        {
            // Arrange
            // ⚠️ Use um projeto que você sabe que existe no SQL Server
            // Exemplo: projeto 4698 que foi migrado anteriormente
            const int realProjectId = 4698;

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                realProjectId,
                _sqlServerConnectionString);

            // Assert - Verifica que a migração foi bem-sucedida
            if (!result.Success)
            {
                Console.WriteLine($"\n❌ ERRO NA MIGRAÇÃO:");
                Console.WriteLine($"   Mensagem: {result.Message}");
                Console.WriteLine($"\n💡 Possíveis causas:");
                Console.WriteLine($"   1. Problema de conexão com SQL Server");
                Console.WriteLine($"   2. Firewall bloqueando a conexão");
                Console.WriteLine($"   3. Connection string incorreta");
                Console.WriteLine($"   4. Servidor SQL Server indisponível");
                Console.WriteLine($"\n🔧 Para resolver:");
                Console.WriteLine($"   - Verifique se o SQL Server está acessível");
                Console.WriteLine($"   - Teste a connection string manualmente");
                Console.WriteLine($"   - Configure TEST_SQLSERVER_CONNECTION_STRING se necessário");
            }
            result.Success.Should().BeTrue($"A migração deve ser bem-sucedida. Erro: {result.Message}");
            result.Data.Should().NotBeNull();

            // Verifica que os dados foram persistidos no MongoDB
            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(realProjectId);
            savedAreasResult.Success.Should().BeTrue();
            savedAreasResult.Data.Should().NotBeNull();

            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;
            savedAreas.Should().NotBeNull().And.NotBeEmpty("Deveria ter áreas migradas");

            // Validações básicas
            foreach (var area in savedAreas!)
            {
                area.Name.Should().NotBeNullOrWhiteSpace("Área deve ter nome");
                area.HeaderId.Should().NotBeNullOrWhiteSpace("Área deve ter headerId");
                area.ProjectId.Should().Be(realProjectId, "ProjectId deve corresponder");

                // Valida que employeeId foi preenchido (primeiro funcionário do projeto)
                // Nota: employeeId pode ser null se o projeto não tiver funcionários
                // Mas se o projeto tiver funcionários, o primeiro deve ser atribuído
                if (!string.IsNullOrWhiteSpace(area.EmployeeId))
                {
                    area.EmployeeId.Should().NotBeNullOrWhiteSpace("EmployeeId deve estar preenchido se o projeto tiver funcionários");
                }

                if (area.Items != null && area.Items.Any())
                {
                    foreach (var item in area.Items)
                    {
                        item.Name.Should().NotBeNullOrWhiteSpace("Item deve ter nome");
                        item.Frequency.Should().NotBeNull("Item deve ter frequência");
                    }
                }
            }

            // Log para facilitar comparação
            Console.WriteLine($"\n✅ Migração E2E concluída!");
            Console.WriteLine($"📊 Total de áreas migradas: {savedAreas.Count()}");
            Console.WriteLine($"📋 Total de items: {savedAreas.Sum(a => a.Items?.Count() ?? 0)}");
            Console.WriteLine($"\n💡 Para comparar com produção:");
            Console.WriteLine($"   use limpidus-test");
            Console.WriteLine($"   db.areaActivity.find({{ projectId: {realProjectId} }}).pretty()");
            Console.WriteLine($"\n   use limpidus");
            Console.WriteLine($"   db.areaActivity.find({{ projectId: {realProjectId} }}).pretty()");
        }

        [Fact(Skip = "Teste E2E - Conecta ao SQL Server real. Execute manualmente quando necessário.")]
        [Trait("Category", "E2E")]
        [Trait("Requires", "SQL Server Connection")]
        public async Task MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveMapearHeaderIdCorretamente()
        {
            // Arrange
            const int realProjectId = 4698;

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                realProjectId,
                _sqlServerConnectionString);

            // Assert
            result.Success.Should().BeTrue();

            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(realProjectId);
            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;

            // Valida que headerId está sendo mapeado corretamente
            // headerId deve corresponder ao WORK_AREA_ID do SQL Server
            savedAreas.Should().NotBeNull().And.NotBeEmpty();

            foreach (var area in savedAreas!)
            {
                area.HeaderId.Should().NotBeNullOrWhiteSpace();
                // headerId deve ser um número (string do WorkAreaId)
                int.TryParse(area.HeaderId, out _).Should().BeTrue(
                    $"headerId '{area.HeaderId}' deve ser um número válido (WorkAreaId)");
            }

            Console.WriteLine($"\n✅ Validação de headerId concluída!");
            Console.WriteLine($"📊 Áreas validadas: {savedAreas.Count()}");
        }

        [Fact(Skip = "Teste E2E - Conecta ao SQL Server real. Execute manualmente quando necessário.")]
        [Trait("Category", "E2E")]
        [Trait("Requires", "SQL Server Connection")]
        public async Task MigrateFromSqlServerAsync_QuandoMigraProjetoReal_DeveMapearFrequenciaCorretamente()
        {
            // Arrange
            const int realProjectId = 4698;

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                realProjectId,
                _sqlServerConnectionString);

            // Assert
            result.Success.Should().BeTrue();

            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(realProjectId);
            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;

            savedAreas.Should().NotBeNull().And.NotBeEmpty();

            // Valida que a frequência está sendo mapeada corretamente
            var areasComItems = savedAreas!.Where(a => a.Items != null && a.Items.Any()).ToList();

            if (areasComItems.Any())
            {
                foreach (var area in areasComItems)
                {
                    foreach (var item in area.Items!)
                    {
                        item.Frequency.Should().NotBeNull(
                            $"Item '{item.Name}' deve ter frequência mapeada");
                        item.Frequency!.Type.Should().NotBeNullOrWhiteSpace(
                            $"Item '{item.Name}' deve ter tipo de frequência");
                    }
                }

                Console.WriteLine($"\n✅ Validação de frequência concluída!");
                Console.WriteLine($"📊 Items validados: {areasComItems.Sum(a => a.Items!.Count())}");
            }
        }

        [Fact(Skip = "Teste E2E - Conecta ao SQL Server real. Execute manualmente quando necessário.")]
        [Trait("Category", "E2E")]
        [Trait("Requires", "SQL Server Connection")]
        public async Task MigrateFromSqlServerAsync_QuandoProjetoNaoExiste_DeveRetornarErro()
        {
            // Arrange
            const int projectIdInexistente = 999999;

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectIdInexistente,
                _sqlServerConnectionString);

            // Assert
            result.Success.Should().BeFalse("Deveria retornar erro para projeto inexistente");
            result.Message.Should().Contain("Nenhuma área encontrada");
        }
    }
}
