using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class SqlServerDataAccessFactory : ISqlServerDataAccessFactory
    {
        public ISqlServerDataAccess Create(string connectionString)
        {
            return new SqlServerDataAccess(connectionString);
        }
    }
}
