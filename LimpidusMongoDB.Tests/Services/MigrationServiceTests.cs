using FluentAssertions;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Services;
using LimpidusMongoDB.Application.Services.Interfaces;
using Moq;
using Xunit;

namespace LimpidusMongoDB.Tests.Services
{
    public class MigrationServiceTests
    {
        private readonly Mock<IAreaActivityService> _areaActivityServiceMock;
        private readonly Mock<ISqlServerDataAccessFactory> _sqlServerDataAccessFactoryMock;
        private readonly Mock<ISqlServerDataAccess> _sqlServerDataAccessMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IEmployeeService> _employeeServiceMock;
        private readonly MigrationService _migrationService;

        public MigrationServiceTests()
        {
            _areaActivityServiceMock = new Mock<IAreaActivityService>();
            _sqlServerDataAccessFactoryMock = new Mock<ISqlServerDataAccessFactory>();
            _sqlServerDataAccessMock = new Mock<ISqlServerDataAccess>();
            _projectServiceMock = new Mock<IProjectService>();
            _employeeServiceMock = new Mock<IEmployeeService>();

            _sqlServerDataAccessFactoryMock
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(_sqlServerDataAccessMock.Object);

            _migrationService = new MigrationService(
                _areaActivityServiceMock.Object,
                _sqlServerDataAccessFactoryMock.Object,
                _projectServiceMock.Object,
                _employeeServiceMock.Object);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoNaoEncontraAreas_DeveRetornarErro()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";

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
            _sqlServerDataAccessFactoryMock.Verify(f => f.Create(connectionString), Times.Once);
            _sqlServerDataAccessMock.Verify(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
            _areaActivityServiceMock.Verify(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoEncontraAreasESalvaComSucesso_DeveRetornarSucesso()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 53681,
                    Area = "Hall Elevadores / Recepção",
                    Metros2 = 50,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            var tarefas = new List<SqlServerTarefaEntity>
            {
                new()
                {
                    Tarefa = 1,
                    Descricao = "Esvaziar cesto lixo (sacos)",
                    Periodo = "LV",
                    FrequenciaDias = 260,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 40,
                    WorkHeaderId = projectId,
                    WorkAreaId = 53681
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarefas);

            _areaActivityServiceMock
                .Setup(x => x.GetByProjectIdAsync(projectId))
                .ReturnsAsync(Result.Ok(data: new List<AreaActivityResponse>()));

            _areaActivityServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();

            _sqlServerDataAccessFactoryMock.Verify(f => f.Create(connectionString), Times.Once);
            _sqlServerDataAccessMock.Verify(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
            _sqlServerDataAccessMock.Verify(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
            _areaActivityServiceMock.Verify(x => x.SaveAsync(
                It.Is<IEnumerable<AreaActivityRequest>>(requests =>
                    requests.Any(r => r.Name == "Hall Elevadores / Recepção" &&
                                     r.HeaderId == "53681" &&
                                     r.Items.Any(i => i.Name == "Esvaziar cesto lixo (sacos)"))),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoAreaJaExiste_DeveAtualizarEmVezDeCriar()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";
            const string existingAreaId = "existing-area-id-123";

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 53681,
                    Area = "Hall Elevadores / Recepção",
                    Metros2 = 50,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            var existingAreas = new List<AreaActivityResponse>
            {
                new()
                {
                    Id = existingAreaId,
                    Name = "Hall Elevadores / Recepção",
                    ProjectId = projectId
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SqlServerTarefaEntity>());

            _areaActivityServiceMock
                .Setup(x => x.GetByProjectIdAsync(projectId))
                .ReturnsAsync(Result.Ok(data: existingAreas));

            _areaActivityServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeTrue();

            _areaActivityServiceMock.Verify(x => x.SaveAsync(
                It.Is<IEnumerable<AreaActivityRequest>>(requests =>
                    requests.Any(r => r.Id == existingAreaId && r.Name == "Hall Elevadores / Recepção")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoSalvarFalha_DeveRetornarErro()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";
            const string errorMessage = "Erro ao salvar no MongoDB";

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = 53681,
                    Area = "Hall Elevadores / Recepção",
                    Metros2 = 50,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SqlServerTarefaEntity>());

            _areaActivityServiceMock
                .Setup(x => x.GetByProjectIdAsync(projectId))
                .ReturnsAsync(Result.Ok(data: new List<AreaActivityResponse>()));

            _areaActivityServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Error(errorMessage));

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain(errorMessage);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoOcorreExcecao_DeveRetornarErro()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";
            const string exceptionMessage = "Erro de conexão";

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoTarefasSaoAgrupadasCorretamente_DeveMapearParaItems()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";
            const int workAreaId = 53681;

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = workAreaId,
                    Area = "Hall Elevadores / Recepção",
                    Metros2 = 50,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            var tarefas = new List<SqlServerTarefaEntity>
            {
                new()
                {
                    Tarefa = 1,
                    Descricao = "Tarefa 1",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 40,
                    WorkHeaderId = projectId,
                    WorkAreaId = workAreaId
                },
                new()
                {
                    Tarefa = 2,
                    Descricao = "Tarefa 2",
                    Periodo = "LV",
                    FrequenciaDias = 52,
                    FrequenciaNome = "weekly",
                    Ordem = 1,
                    Metros2 = 30,
                    WorkHeaderId = projectId,
                    WorkAreaId = workAreaId
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarefas);

            _areaActivityServiceMock
                .Setup(x => x.GetByProjectIdAsync(projectId))
                .ReturnsAsync(Result.Ok(data: new List<AreaActivityResponse>()));

            _areaActivityServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeTrue();

            _areaActivityServiceMock.Verify(x => x.SaveAsync(
                It.Is<IEnumerable<AreaActivityRequest>>(requests =>
                    requests.Any(r => r.Items.Count() == 2 &&
                                     r.Items.Any(i => i.Name == "Tarefa 1" && i.OrderBy == 1) &&
                                     r.Items.Any(i => i.Name == "Tarefa 2" && i.OrderBy == 2))),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MigrateFromSqlServerAsync_QuandoHeaderIdEhMapeadoCorretamente_DeveUsarWorkAreaId()
        {
            // Arrange
            const int projectId = 4698;
            const string connectionString = "test-connection-string";
            const int workAreaId = 53681;

            var areas = new List<SqlServerAreaEntity>
            {
                new()
                {
                    AreaId = 1,
                    WorkAreaId = workAreaId,
                    Area = "Hall Elevadores / Recepção",
                    Metros2 = 50,
                    Densidade = "",
                    WorkHeaderId = projectId
                }
            };

            _sqlServerDataAccessMock
                .Setup(x => x.GetAreasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(areas);

            _sqlServerDataAccessMock
                .Setup(x => x.GetTarefasAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SqlServerTarefaEntity>());

            _areaActivityServiceMock
                .Setup(x => x.GetByProjectIdAsync(projectId))
                .ReturnsAsync(Result.Ok(data: new List<AreaActivityResponse>()));

            _areaActivityServiceMock
                .Setup(x => x.SaveAsync(It.IsAny<IEnumerable<AreaActivityRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            // Act
            var result = await _migrationService.MigrateFromSqlServerAsync(
                projectId,
                connectionString);

            // Assert
            result.Success.Should().BeTrue();

            _areaActivityServiceMock.Verify(x => x.SaveAsync(
                It.Is<IEnumerable<AreaActivityRequest>>(requests =>
                    requests.Any(r => r.HeaderId == workAreaId.ToString())),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
