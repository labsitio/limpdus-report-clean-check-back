namespace LimpidusMongoDB.Application.Data.Entities
{
    /// <summary>
    /// Representa a estrutura de Projeto do banco SQL Server (sistema WebForms)
    /// Mapeia a tabela WORK_HEADER
    /// </summary>
    public class SqlServerProjectEntity
    {
        public int WorkHeaderId { get; set; } // WORK_HEADER_ID
        public string NomeProjeto { get; set; } // NOMEPROJETO
        public double TotalM2 { get; set; } // TOTALM2 (float)
        public int DiasAno { get; set; } // DIASANO
        public int OpcoesCalculo { get; set; } // OPCOESCALCULO
        public double Fator { get; set; } // FATOR (float)
        public string End1 { get; set; } // END1
        public string End2 { get; set; } // END2
        public string End3 { get; set; } // END3
        public string Contato { get; set; } // CONTATO
        public string Telefone { get; set; } // TELEFONE
        public string Celular { get; set; } // CELULAR
        public DateTime DataCad { get; set; } // DATACAD
        public int NivelProjeto { get; set; } // NIVEL_PROJETO
    }
}
