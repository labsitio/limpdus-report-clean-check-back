namespace LimpidusMongoDB.Application.Data.Entities
{
    /// <summary>
    /// Representa a estrutura de Area do banco SQL Server (sistema WebForms)
    /// </summary>
    public class SqlServerAreaEntity
    {
        public int AreaId { get; set; }
        public int WorkAreaId { get; set; }
        public string Area { get; set; }
        public int Metros2 { get; set; }
        public string Densidade { get; set; }
        public int WorkHeaderId { get; set; }
    }
}
