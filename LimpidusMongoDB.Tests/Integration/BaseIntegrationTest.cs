using LimpidusMongoDB.Application.Data;
using LimpidusMongoDB.Application.Data.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Xunit;

namespace LimpidusMongoDB.Tests.Integration
{
    /// <summary>
    /// Classe base para testes de integração com MongoDB
    /// Configura uma base de dados de teste e limpa os dados após cada teste
    /// </summary>
    public abstract class BaseIntegrationTest : IClassFixture<MongoDbTestFixture>, IDisposable
    {
        protected readonly LimpidusContextDB TestContext;
        protected readonly IMongoDatabase TestDatabase;
        private readonly MongoDbTestFixture _fixture;
        private bool _disposed;

        protected BaseIntegrationTest(MongoDbTestFixture fixture)
        {
            _fixture = fixture;
            TestContext = fixture.TestContext;
            TestDatabase = fixture.TestDatabase;
        }

        /// <summary>
        /// Limpa todas as coleções de teste após cada teste
        /// </summary>
        /// <remarks>
        /// LIMPEZA COMENTADA: Descomente para limpar dados automaticamente após cada teste
        /// </remarks>
        public void Dispose()
        {
            if (!_disposed)
            {
                // LIMPEZA AUTOMÁTICA DESABILITADA - Dados permanecerão no banco para consulta
                // Descomente a linha abaixo para reativar a limpeza automática
                CleanupTestData();
                _disposed = true;
            }
        }

        /// <summary>
        /// Remove todos os dados das coleções de teste
        /// </summary>
        /// <remarks>
        /// Para reativar a limpeza automática, descomente a chamada deste método no Dispose()
        /// </remarks>
        private void CleanupTestData()
        {
            try
            {
                var collections = new[]
                {
                    "areaActivity",
                    "project",
                    "employee",
                    "operationalTask",
                    "itemOperationalTask",
                    "history",
                    "itemHistory",
                    "user",
                    "justification"
                };

                foreach (var collectionName in collections)
                {
                    var collection = TestDatabase.GetCollection<object>(collectionName);
                    collection.DeleteMany(FilterDefinition<object>.Empty);
                }
            }
            catch (Exception ex)
            {
                // Log do erro, mas não falha o teste
                Console.WriteLine($"Erro ao limpar dados de teste: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpa uma coleção específica
        /// </summary>
        protected void CleanupCollection(string collectionName)
        {
            var collection = TestDatabase.GetCollection<object>(collectionName);
            collection.DeleteMany(FilterDefinition<object>.Empty);
        }
    }
}
