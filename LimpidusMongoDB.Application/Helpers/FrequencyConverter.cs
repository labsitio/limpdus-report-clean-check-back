namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Helper para converter frequências e períodos do formato legado para o novo formato
    /// </summary>
    public static class FrequencyConverter
    {
        /// <summary>
        /// Converte a frequência numérica do legado para o tipo de frequência
        /// Exemplo: "260" pode representar dias do ano, "52" = semanal, etc.
        /// </summary>
        public static string ConvertFrequencyType(string frequencia)
        {
            if (string.IsNullOrWhiteSpace(frequencia))
                return "weekly"; // padrão

            // Tenta converter para número
            if (int.TryParse(frequencia, out int freqValue))
            {
                return freqValue switch
                {
                    1 => "yearly",
                    2 => "semi-annual",
                    4 => "quarterly",
                    6 => "bimonthly",
                    12 => "monthly",
                    26 => "biweekly",
                    52 => "weekly",
                    260 => "weekly", // 5 dias por semana * 52 semanas
                    365 => "everyday",
                    _ => "weekly" // padrão
                };
            }

            return "weekly"; // padrão
        }

        /// <summary>
        /// Converte o período do legado (ex: "LV" = Segunda a Sexta) para array de dias da semana
        /// Dias da semana: 0=Domingo, 1=Segunda, 2=Terça, 3=Quarta, 4=Quinta, 5=Sexta, 6=Sábado
        /// </summary>
        public static List<short> ConvertPeriodoToWeekDays(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                return new List<short> { 1, 2, 3, 4, 5 }; // padrão: segunda a sexta

            var weekDays = new List<short>();
            periodo = periodo.ToUpper();

            // Mapeamento de letras para dias da semana
            // D = Domingo (0), S = Segunda (1), T = Terça (2), Q = Quarta (3), 
            // Q = Quinta (4), S = Sexta (5), S = Sábado (6)
            // Mas como há ambiguidade, vamos usar padrões comuns:
            
            if (periodo.Contains("LV") || periodo == "LV")
            {
                // Segunda a Sexta (L=Segunda, V=Sexta)
                weekDays.AddRange(new short[] { 1, 2, 3, 4, 5 });
            }
            else if (periodo.Contains("DS") || periodo == "DS")
            {
                // Domingo a Sábado (todos os dias)
                weekDays.AddRange(new short[] { 0, 1, 2, 3, 4, 5, 6 });
            }
            else if (periodo.Contains("SS") || periodo == "SS")
            {
                // Sábado e Domingo (fim de semana)
                weekDays.AddRange(new short[] { 0, 6 });
            }
            else
            {
                // Tenta mapear letra por letra
                foreach (char c in periodo)
                {
                    switch (c)
                    {
                        case 'D':
                            if (!weekDays.Contains(0)) weekDays.Add(0); // Domingo
                            break;
                        case 'S':
                            // Pode ser Segunda, Sexta ou Sábado - vamos adicionar Segunda e Sexta por padrão
                            if (!weekDays.Contains(1)) weekDays.Add(1); // Segunda
                            if (!weekDays.Contains(5)) weekDays.Add(5); // Sexta
                            break;
                        case 'T':
                            if (!weekDays.Contains(2)) weekDays.Add(2); // Terça
                            break;
                        case 'Q':
                            // Pode ser Quarta ou Quinta - vamos adicionar ambas
                            if (!weekDays.Contains(3)) weekDays.Add(3); // Quarta
                            if (!weekDays.Contains(4)) weekDays.Add(4); // Quinta
                            break;
                        case 'L':
                            if (!weekDays.Contains(1)) weekDays.Add(1); // Segunda (Lunes)
                            break;
                        case 'V':
                            if (!weekDays.Contains(5)) weekDays.Add(5); // Sexta (Viernes)
                            break;
                    }
                }

                // Se não encontrou nenhum dia, usa padrão
                if (weekDays.Count == 0)
                {
                    weekDays.AddRange(new short[] { 1, 2, 3, 4, 5 });
                }
            }

            return weekDays.OrderBy(d => d).ToList();
        }
    }
}
