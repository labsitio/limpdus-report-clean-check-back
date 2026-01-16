using LimpidusMongoDB.Application.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Xunit;

namespace LimpidusMongoDB.Tests.Integration
{
    /// <summary>
    /// Fixture para configurar o MongoDB de teste uma única vez para todos os testes
    /// </summary>
    public class MongoDbTestFixture : IDisposable
    {
        public LimpidusContextDB TestContext { get; }
        public IMongoDatabase TestDatabase { get; }
        private readonly IConfiguration _configuration;
        private bool _disposed;

        public MongoDbTestFixture()
        {
            // Configuração para usar base de dados de teste
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:LimpidusDB", GetTestConnectionString() },
                { "AppSettings:Database", "limpidus-test" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            TestContext = new LimpidusContextDB(_configuration);
            TestDatabase = TestContext.Database;
        }

        /// <summary>
        /// Obtém a connection string de teste
        /// Usa a mesma connection string de produção, mas com base de dados diferente
        /// </summary>
        private static string GetTestConnectionString()
        {
            // Usa a mesma connection string, mas a base será "limpidus-test"
            // Isso permite usar o mesmo cluster MongoDB, mas isolado
            var connectionString = Environment.GetEnvironmentVariable("TEST_MONGODB_CONNECTION_STRING")
                ?? "mongodb+srv://producao:7diEnLIjhtCa5Xxr@cluster0.nmool17.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

            return connectionString;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // LIMPEZA DA BASE DESABILITADA - Base de dados permanecerá para consulta
                // Descomente a linha abaixo para deletar a base de dados inteira após todos os testes
                // TestDatabase.Client.DropDatabase("limpidus-test");
                _disposed = true;
            }
        }
    }
}
