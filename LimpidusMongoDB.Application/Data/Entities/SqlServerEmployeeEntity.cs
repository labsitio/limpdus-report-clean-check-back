namespace LimpidusMongoDB.Application.Data.Entities
{
    /// <summary>
    /// Representa a estrutura de Funcionário do banco SQL Server (sistema WebForms)
    /// Mapeia a tabela WORK_FUNCIONARIO
    /// </summary>
    public class SqlServerEmployeeEntity
    {
        public int WorkHeaderId { get; set; } // WORK_HEADER_ID
        public int Funcionario { get; set; } // FUNCIONARIO
        public TimeSpan? HoraEntra { get; set; } // HORAENTRA (time)
        public TimeSpan? HoraSai { get; set; } // HORASAI (time)
        public string Obs { get; set; } // OBS (text)
    }
}
