namespace LimpidusMongoDB.Application.Data.Entities
{
    /// <summary>
    /// Representa a estrutura de Tarefa do banco SQL Server (sistema WebForms)
    /// </summary>
    public class SqlServerTarefaEntity
    {
        public int Tarefa { get; set; } // TAREFA_NUMERO
        public string Descricao { get; set; } // NOME_TAREFA
        public string Periodo { get; set; }
        public string Frequencia { get; set; }
        public int Metros2 { get; set; }
        public int WorkHeaderId { get; set; }
        public int WorkAreaId { get; set; }
    }
}
