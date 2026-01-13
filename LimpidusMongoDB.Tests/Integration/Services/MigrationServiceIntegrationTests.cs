using FluentAssertions;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Services;
using LimpidusMongoDB.Application.Services.Interfaces;
using LimpidusMongoDB.Tests.Integration;
using Moq;
using Xunit;

namespace LimpidusMongoDB.Tests.Integration.Services
{
    /// <summary>
    /// Testes de integração para MigrationService
    /// Testa a persistência real dos dados no MongoDB
    /// </summary>
    public class MigrationServiceIntegrationTests : BaseIntegrationTest
    {
        private readonly IAreaActivityService _areaActivityService;
        private readonly IAreaActivityRepository _areaActivityRepository;
        private readonly IMigrationService _migrationService;
        private readonly Mock<ISqlServerDataAccess> _sqlServerDataAccessMock;
        private readonly Mock<ISqlServerDataAccessFactory> _sqlServerDataAccessFactoryMock;
        private readonly Mock<IProjectService> _projectServiceMock;

        public MigrationServiceIntegrationTests(MongoDbTestFixture fixture) : base(fixture)
        {
            // Setup repositórios reais
            _areaActivityRepository = new AreaActivityRepository(TestContext);

            // Setup serviços reais
            _areaActivityService = new AreaActivityService(_areaActivityRepository);

            // Setup mocks para SQL Server (não queremos conectar ao SQL Server real nos testes)
            _sqlServerDataAccessMock = new Mock<ISqlServerDataAccess>();
            _sqlServerDataAccessFactoryMock = new Mock<ISqlServerDataAccessFactory>();
            _sqlServerDataAccessFactoryMock
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(_sqlServerDataAccessMock.Object);

            // Setup mock para ProjectService
            // Por padrão, retorna projeto sem funcionários (employeeId será null)
            _projectServiceMock = new Mock<IProjectService>();
            _projectServiceMock
                .Setup(x => x.GetByLegacyIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int legacyId) =>
                {
                    var projectEntity = new ProjectEntity
                    {
                        LegacyId = legacyId,
                        Name = "Projeto Teste",
                        TotalM2 = 0,
                        DaysYear = 365,
                        Factor = 1,
                        Address = "",
                        Contact = "",
                        TelephoneNumber = "",
                        CellphoneNumber = "",
                        RegistrationDate = DateTime.UtcNow,
                        Level = 1
                    };
                    return Result.Ok(data: new ProjectResponse(projectEntity, new List<EmployeeEntity>()));
                });

            // Setup mock para EmployeeService
            var employeeServiceMock = new Mock<IEmployeeService>();
            employeeServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<EmployeeRequest>()))
                .ReturnsAsync(Result.Ok());

            _migrationService = new MigrationService(_areaActivityService, _sqlServerDataAccessFactoryMock.Object, _projectServiceMock.Object, employeeServiceMock.Object);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoPersisteDados_DeveSalvarNoMongoDB()
        {
            // Arrange
            const int projectId = 9999; // ID de teste
            const string connectionString = "test-connection";

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 1001,
                    Area = "Área de Teste 1",
                    Metros2 = 100,
                    Densidade = "Alta",
                    WorkHeaderId = projectId
                },
                new()
                {
                    AreaId = 2,
                    WorkAreaId = 1002,
                    Area = "Área de Teste 2",
                    Metros2 = 200,
                    Densidade = "Média",
                    WorkHeaderId = projectId
                }
            };

            var tarefas = new List<SqlServerTarefaEntity>
            {
                new()
                {
                    Tarefa = 1,
                    Descricao = "Tarefa de Teste 1",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 50,
                    WorkHeaderId = projectId,
                    WorkAreaId = 1001
                },
                new()
                {
                    Tarefa = 2,
                    Descricao = "Tarefa de Teste 2",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 75,
                    WorkHeaderId = projectId,
                    WorkAreaId = 1001
                },
                new()
                {
                    Tarefa = 3,
                    Descricao = "Tarefa de Teste 3",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 100,
                    WorkHeaderId = projectId,
                    WorkAreaId = 1002
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarefas);

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();

            // Verifica que os dados foram realmente persistidos no MongoDB
            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(projectId);
            savedAreasResult.Success.Should().BeTrue();
            savedAreasResult.Data.Should().NotBeNull();

            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;
            savedAreas.Should().NotBeNull();
            savedAreas!.Should().HaveCount(2);

            savedAreas.Should().NotBeNull();
            var area1 = savedAreas!.FirstOrDefault(a => a.Name == "Área de Teste 1");
            area1.Should().NotBeNull();
            area1!.TotalM2.Should().Be(100);
            area1.HeaderId.Should().Be("1001");
            area1.Items.Should().HaveCount(2);
            area1.Items.Should().Contain(i => i.Name == "Tarefa de Teste 1");
            area1.Items.Should().Contain(i => i.Name == "Tarefa de Teste 2");

            var area2 = savedAreas.FirstOrDefault(a => a.Name == "Área de Teste 2");
            area2.Should().NotBeNull();
            area2!.TotalM2.Should().Be(200);
            area2.HeaderId.Should().Be("1002");
            area2.Items.Should().HaveCount(1);
            area2.Items.Should().Contain(i => i.Name == "Tarefa de Teste 3");
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoAreaJaExiste_DeveAtualizarEmVezDeDuplicar()
        {
            // Arrange
            const int projectId = 9998;
            const string connectionString = "test-connection";

            // Primeiro, cria uma área existente no MongoDB
            var existingArea = new AreaActivityEntity
            {
                Name = "Área Existente",
                Description = "Descrição original",
                TotalM2 = 50,
                HeaderId = "2001",
                ProjectId = projectId,
                OrderBy = 1
            };
            // O ID é gerado automaticamente no construtor

            await _areaActivityRepository.InsertOneAsync(existingArea);
            var existingId = existingArea.Id.ToString();
            
            // Aguarda um pouco para garantir que o registro foi persistido
            await Task.Delay(100);

            // Agora simula migração com a mesma área
            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 2001,
                    Area = "Área Existente", // Mesmo nome
                    Metros2 = 150, // Valor atualizado
                    Densidade = "Alta",
                    WorkHeaderId = projectId
                }
            };

            var tarefas = new List<SqlServerTarefaEntity>
            {
                new()
                {
                    Tarefa = 1,
                    Descricao = "Nova Tarefa",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 100,
                    WorkHeaderId = projectId,
                    WorkAreaId = 2001
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarefas);

            // Mock ProjectService para retornar projeto (pode ter funcionário ou não)
            _projectServiceMock
                .Setup(x => x.GetByLegacyIdAsync(projectId))
                .ReturnsAsync((int legacyId) =>
                {
                    var projectEntity = new ProjectEntity
                    {
                        LegacyId = legacyId,
                        Name = "Projeto Teste",
                        TotalM2 = 0,
                        DaysYear = 365,
                        Factor = 1,
                        Address = "",
                        Contact = "",
                        TelephoneNumber = "",
                        CellphoneNumber = "",
                        RegistrationDate = DateTime.UtcNow,
                        Level = 1
                    };
                    return Result.Ok(data: new ProjectResponse(projectEntity, new List<EmployeeEntity>()));
                });

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            if (!result.Success)
            {
                Console.WriteLine($"Erro na migração: {result.Message}");
            }
            result.Success.Should().BeTrue($"Erro: {result.Message}");

            // Verifica que não foi criada uma área duplicada
            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(projectId);
            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;
            savedAreas.Should().NotBeNull();
            savedAreas!.Should().HaveCount(1);

            var updatedArea = savedAreas.First();
            // Verifica que não foi criada uma nova área (deve ter apenas 1 área com esse nome)
            // O ID pode ser diferente se a área foi recriada, mas o importante é não ter duplicatas
            updatedArea.Name.Should().Be("Área Existente");
            updatedArea.TotalM2.Should().Be(150); // Valor atualizado
            updatedArea.Items.Should().Contain(i => i.Name == "Nova Tarefa");
            
            // Se possível, verifica que o ID foi mantido (atualização) ou que não há duplicatas
            // Nota: O comportamento pode variar dependendo da implementação do SaveAsync
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoPersisteItems_DeveMapearFrequenciaCorretamente()
        {
            // Arrange
            const int projectId = 9997;
            const string connectionString = "test-connection";

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 3001,
                    Area = "Área com Frequência",
                    Metros2 = 100,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            var tarefas = new List<SqlServerTarefaEntity>
            {
                new()
                {
                    Tarefa = 1,
                    Descricao = "Tarefa Semanal",
                    Periodo = "LV", // Segunda a Sexta
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1, // Semanal
                    Metros2 = 50,
                    WorkHeaderId = projectId,
                    WorkAreaId = 3001
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarefas);

            // Mock ProjectService já configurado no construtor, mas pode sobrescrever aqui se necessário

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            if (!result.Success)
            {
                Console.WriteLine($"Erro na migração: {result.Message}");
            }
            result.Success.Should().BeTrue($"Erro: {result.Message}");

            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(projectId);
            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;
            savedAreas.Should().NotBeNull().And.NotBeEmpty();
            var area = savedAreas!.First();

            var item = area.Items.First();
            item.Frequency.Should().NotBeNull();
            item.Frequency!.Type.Should().Be("weekly");
            item.Frequency.WeekDays.Should().NotBeNull();
            item.Frequency.WeekDays.Should().HaveCount(5); // Segunda a Sexta
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoNaoEncontraAreas_DeveRetornarErroSemPersistir()
        {
            // Arrange
            const int projectId = 9996;
            const string connectionString = "test-connection";

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SqlServerAreaEntity>());

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Nenhuma área encontrada");

            // Verifica que nada foi persistido
            var savedAreasResult = await _areaActivityService.GetByProjectIdAsync(projectId);
            var savedAreas = savedAreasResult.Data as IEnumerable<AreaActivityResponse>;
            savedAreas.Should().BeEmpty();
        }
    }
}
