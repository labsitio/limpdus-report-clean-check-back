using LimpidusMongoDB.Application.Data.Entities;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        public Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default);

        public Task<IEnumerable<TEntity>> FindAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default);

        public Task<TEntity> FindOneAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default);

        public Task<TEntity> FindByIdAsync(string id, CancellationToken cancellationToken = default);

        public Task<TEntity> FindByHistoryIdAsync(string id, CancellationToken cancellationToken = default);

        public Task<bool> Exists(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default);

        public Task InsertOneAsync(TEntity user, CancellationToken cancellationToken = default);

        public Task UpdateOneAsync(TEntity entity, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default);

        public Task UpdateOneAsync(string id, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default);

        public Task UpdateOneAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default);

        public Task DeleteOneAsync(string id, CancellationToken cancellationToken = default);

        public Task DeleteManyAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default);
    }
}