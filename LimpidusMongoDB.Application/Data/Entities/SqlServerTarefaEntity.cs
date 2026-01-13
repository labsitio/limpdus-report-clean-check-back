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
        public int FrequenciaDias { get; set; } // FREQUENCIA_DIAS (da tabela WORK_TAREFAS)
        public string FrequenciaNome { get; set; } // FREQUENCIA_NOME (da tabela WORK_TBL_FREQ)
        public int Ordem { get; set; } // ORDEM (da tabela WORK_TAREFAS)
        public int Metros2 { get; set; }
        public int WorkHeaderId { get; set; }
        public int WorkAreaId { get; set; }
    }
}
