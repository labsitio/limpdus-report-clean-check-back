namespace LimpidusMongoDB.Application.Services.Interfaces
{
    /// <summary>
    /// Factory para criar instâncias de ISqlServerDataAccess
    /// </summary>
    public interface ISqlServerDataAccessFactory
    {
        ISqlServerDataAccess Create(string connectionString);
    }
}
