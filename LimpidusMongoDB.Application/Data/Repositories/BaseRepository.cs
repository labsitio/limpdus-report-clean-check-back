using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Helpers;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly IMongoCollection<TEntity> _entityCollection;

        protected BaseRepository(LimpidusContextDB context)
            => _entityCollection = context.Database.GetCollection<TEntity>(Utils.GetCollectionName<TEntity>());

        public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _entityCollection.FindAsync(FilterDefinition<TEntity>.Empty, cancellationToken: cancellationToken);
            return await result.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default)
        {
            var result = await _entityCollection.FindAsync(filterDefinition, cancellationToken: cancellationToken);
            return await result.ToListAsync(cancellationToken);
        }

        public async Task<TEntity> FindOneAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default)
        {
            var result = await _entityCollection.FindAsync(filterDefinition, cancellationToken: cancellationToken);
            return await result.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TEntity> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = BaseEntity.FindByIdDefinition<TEntity>(id);
            return await FindOneAsync(filter, cancellationToken);
        }

        public async Task<TEntity> FindByHistoryIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = BaseEntity.FindByHistoryIdDefinition<TEntity>(id);
            return await FindOneAsync(filter, cancellationToken);
        }

        public async Task<bool> Exists(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default)
        {
            var result = await _entityCollection.CountDocumentsAsync(filterDefinition, cancellationToken: cancellationToken);
            return result > 0;
        }

        public async Task InsertOneAsync(TEntity user, CancellationToken cancellationToken = default)
            => await _entityCollection.InsertOneAsync(user, cancellationToken: cancellationToken);

        public async Task UpdateOneAsync(TEntity entity, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            updateDefinition = entity.SetUpdateDateAndGetDefinition(updateDefinition);
            await _entityCollection.UpdateOneAsync(entity.FindByIdDefinition<TEntity>(), updateDefinition, cancellationToken: cancellationToken);
        }

        public async Task UpdateOneAsync(string id, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            updateDefinition = BaseEntity.UpdateDateDefinition(updateDefinition);
            await _entityCollection.UpdateOneAsync(BaseEntity.FindByIdDefinition<TEntity>(id), updateDefinition, cancellationToken: cancellationToken);
        }

        public async Task UpdateOneAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
        {
            updateDefinition = BaseEntity.UpdateDateDefinition(updateDefinition);
            await _entityCollection.UpdateOneAsync(filterDefinition, updateDefinition, cancellationToken: cancellationToken);
        }

        public async Task DeleteOneAsync(string id, CancellationToken cancellationToken = default)
        {
            var filterDefinition = BaseEntity.FindByIdDefinition<TEntity>(id);
            await _entityCollection.DeleteOneAsync(filterDefinition, cancellationToken: cancellationToken);
        }

        public async Task DeleteManyAsync(FilterDefinition<TEntity> filterDefinition, CancellationToken cancellationToken = default)
        {
            await _entityCollection.DeleteManyAsync(filterDefinition, cancellationToken);
        }
    }
}