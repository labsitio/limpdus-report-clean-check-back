using LimpidusMongoDB.Application.Data.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Data
{
    public class LimpidusContextDB
    {
        public IMongoDatabase Database { get; }

        public LimpidusContextDB(IConfiguration configuration)
        {
            try
            {
                var database = configuration.GetSection("AppSettings:Database").Value;
                var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration.GetConnectionString("LimpidusDB")));
                var client = new MongoClient(mongoClientSettings);
                Database = client.GetDatabase(database);
                SetCamelCaseNameConvention();
                MapClasses();
            }
            catch (Exception ex)
            {
                throw new MongoException("Unable to connect to the database", ex);
            }
        }

        private void SetCamelCaseNameConvention()
        {
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camelCase", conventionPack, _ => true);
        }

        protected void RegisterMap<TEntity>() where TEntity : BaseEntity
        {
            BsonClassMap.RegisterClassMap<TEntity>(i =>
            {
                i.AutoMap();
                i.SetIgnoreExtraElements(true);
            });
        }

        protected void MapClasses()
        {
            RegisterMap<AreaActivityEntity>();
            RegisterMap<ProjectEntity>();
            RegisterMap<EmployeeEntity>();
            RegisterMap<OperationalTaskEntity>();
            RegisterMap<ItemOperationalTaskEntity>();
            RegisterMap<HistoryEntity>();
            RegisterMap<ItemHistoryEntity>();
            RegisterMap<UserEntity>();
            RegisterMap<JustificationEntity>();
        }
    }
}