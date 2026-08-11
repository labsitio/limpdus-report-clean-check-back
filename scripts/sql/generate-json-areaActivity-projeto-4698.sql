/*
  Gera um JSON (array) compatível com POST /v1/AreaActivity para o projeto legado 4698.
  - Nomes e METROS2 vêm de dbo.WORK_AREA (tem de existir no mesmo servidor).
  - Tarefas vêm da tabela #Tarefas (dados exportados do legado).

  Uso (SSMS):
    1) Ajuste @EmployeeMongoId (ObjectId 24 hex do funcionário no Mongo).
    2) Execute o script inteiro.
    3) Copie o resultado da última coluna (JSON) e envie como body no POST.

  weekDays: 0=Dom … 6=Sáb (alinhado à API). RT/LV tratados como rotina seg–sex;
  312 = seg–sáb; 208 = seg–qui (4x/semana aprox.); 26 quinzenal; 12 mensal; 4 trimestral.
*/

SET NOCOUNT ON;

DECLARE @ProjectId INT = 4698;
DECLARE @EmployeeMongoId NVARCHAR(32) = N'COLE_OBJECTID_24_HEX_FUNCIONARIO_MONGO';

DROP TABLE IF EXISTS #Tarefas;
CREATE TABLE #Tarefas (
    WORK_AREA_ID INT NOT NULL,
    Tarefa       INT NOT NULL,
    Descricao    NVARCHAR(500) NOT NULL,
    Periodo      NVARCHAR(20) NOT NULL,
    FrequenciaDias INT NOT NULL
);

INSERT INTO #Tarefas (WORK_AREA_ID, Tarefa, Descricao, Periodo, FrequenciaDias) VALUES
(53683, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(53683, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53683, 15, N'Remover marcas de paredes, portas e divisórias.', N'LV', 52),
(53683, 27, N'Mop pó', N'LV', 260),
(53683, 29, N'Mop úmido', N'RT', 260),
(53685, 3, N'Levar o lixo coletado', N'LV', 260),
(53685, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53685, 27, N'Mop pó', N'LV', 260),
(53685, 29, N'Mop úmido', N'RT', 260),
(53685, 179, N'Limpeza de refrigerador (*)', N'LV', 52),
(53685, 222, N'Esvaziar lixo (cozinha)', N'LV', 312),
(53685, 232, N'Limpeza úmida de mesas (*)', N'LV', 260),
(53685, 236, N'Limpeza de forno microondas (*)', N'LV', 52),
(53687, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(53687, 3, N'Levar o lixo coletado', N'LV', 260),
(53687, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(53687, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53687, 13, N'Limpar pés de cadeiras', N'RT', 26),
(53687, 17, N'Limpeza de telefones', N'RT', 52),
(53687, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(53687, 24, N'Aspiração completa', N'RT', 52),
(53689, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(53689, 3, N'Levar o lixo coletado', N'LV', 260),
(53689, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(53689, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53689, 13, N'Limpar pés de cadeiras', N'RT', 26),
(53689, 17, N'Limpeza de telefones', N'RT', 52),
(53689, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(53689, 24, N'Aspiração completa', N'RT', 52),
(53695, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(53695, 3, N'Levar o lixo coletado', N'LV', 260),
(53695, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(53695, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53695, 13, N'Limpar pés de cadeiras', N'RT', 26),
(53695, 14, N'Tirar poeira alta - detalhada', N'LV', 12),
(53695, 17, N'Limpeza de telefones', N'RT', 52),
(53695, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(53695, 24, N'Aspiração completa', N'RT', 52),
(53695, 86, N'Aspirar móveis de escritório(*)', N'RT', 4),
(53697, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(53697, 3, N'Levar o lixo coletado', N'LV', 260),
(53697, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(53697, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(53697, 13, N'Limpar pés de cadeiras', N'RT', 26),
(53697, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(53697, 24, N'Aspiração completa', N'RT', 52),
(53697, 86, N'Aspirar móveis de escritório(*)', N'RT', 4),
(63941, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63941, 3, N'Levar o lixo coletado', N'LV', 260),
(63941, 45, N'Limpeza completa de WC individual (*)', N'RT', 260),
(63943, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63943, 3, N'Levar o lixo coletado', N'LV', 260),
(63943, 45, N'Limpeza completa de WC individual (*)', N'RT', 260),
(63947, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63947, 3, N'Levar o lixo coletado', N'LV', 260),
(63947, 44, N'Limpeza completa de WC coletivo (*)', N'RT', 260),
(63949, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63949, 3, N'Levar o lixo coletado', N'LV', 260),
(63949, 45, N'Limpeza completa de WC individual (*)', N'RT', 260),
(63951, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63951, 3, N'Levar o lixo coletado', N'LV', 260),
(63951, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(63951, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(63951, 13, N'Limpar pés de cadeiras', N'RT', 26),
(63951, 15, N'Remover marcas de paredes, portas e divisórias.', N'LV', 52),
(63951, 17, N'Limpeza de telefones', N'RT', 52),
(63951, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(63951, 24, N'Aspiração completa', N'RT', 52),
(63951, 52, N'Limpeza de divisória de vidro', N'LV', 26),
(63951, 232, N'Limpeza úmida de mesas (*)', N'LV', 0),
(63953, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(63953, 3, N'Levar o lixo coletado', N'LV', 260),
(63953, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(63953, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(63953, 13, N'Limpar pés de cadeiras', N'RT', 26),
(63953, 15, N'Remover marcas de paredes, portas e divisórias.', N'LV', 52),
(63953, 17, N'Limpeza de telefones', N'RT', 52),
(63953, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(63953, 24, N'Aspiração completa', N'RT', 52),
(63953, 232, N'Limpeza úmida de mesas (*)', N'LV', 0),
(64025, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(64025, 27, N'Mop pó', N'LV', 260),
(64025, 29, N'Mop úmido', N'RT', 260),
(64025, 51, N'Limpeza de porta de vidro (*)', N'LV', 260),
(64027, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(64027, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64027, 15, N'Remover marcas de paredes, portas e divisórias.', N'LV', 52),
(64027, 24, N'Aspiração completa', N'RT', 52),
(64029, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(64029, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64029, 15, N'Remover marcas de paredes, portas e divisórias.', N'LV', 52),
(64029, 24, N'Aspiração completa', N'RT', 52),
(64029, 232, N'Limpeza úmida de mesas (*)', N'LV', 0),
(64029, 233, N'Limpeza úmida de cadeiras (*)', N'LV', 260),
(64031, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(64031, 3, N'Levar o lixo coletado', N'LV', 260),
(64031, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(64031, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64031, 13, N'Limpar pés de cadeiras', N'RT', 26),
(64031, 17, N'Limpeza de telefones', N'RT', 52),
(64031, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(64031, 24, N'Aspiração completa', N'RT', 52),
(64033, 3, N'Levar o lixo coletado', N'LV', 260),
(64033, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64033, 19, N'Limpeza de pias (*)', N'LV', 260),
(64033, 29, N'Mop úmido', N'RT', 260),
(64035, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64035, 11, N'Tirar poeira baixa', N'LV', 52),
(64035, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(64195, 1, N'Esvaziar cesto lixo (sacos)', N'LV', 260),
(64195, 3, N'Levar o lixo coletado', N'LV', 260),
(64195, 7, N'Tirar pó de mesas e superficies horizontais.', N'RT', 260),
(64195, 9, N'Tirar pó de superficies altas.', N'LV', 52),
(64195, 13, N'Limpar pés de cadeiras', N'RT', 26),
(64195, 17, N'Limpeza de telefones', N'RT', 52),
(64195, 20, N'Tirar pó de persianas com retentor', N'RT', 52),
(64195, 22, N'Aspirar linhas de tráfego', N'RT', 208),
(64195, 24, N'Aspiração completa', N'RT', 52),
(64195, 72, N'Tirar poeira de equipamentos', N'LV', 260);

;WITH Dedup AS (
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY WORK_AREA_ID, Tarefa ORDER BY Tarefa) AS rn
    FROM #Tarefas
)
DELETE FROM Dedup WHERE rn > 1;

/* Uma linha por área (ord estável); tarefas agregadas em JSON */
;WITH DistinctAreas AS (
    SELECT
        b.WORK_AREA_ID,
        b.AREA,
        b.METROS2
    FROM dbo.WORK_AREA AS b WITH (NOLOCK)
    INNER JOIN #Tarefas AS t ON t.WORK_AREA_ID = b.WORK_AREA_ID
    WHERE b.WORK_HEADER_ID = @ProjectId
    GROUP BY b.WORK_AREA_ID, b.AREA, b.METROS2
),
Areas AS (
    SELECT
        d.WORK_AREA_ID,
        d.AREA,
        d.METROS2,
        DENSE_RANK() OVER (ORDER BY d.WORK_AREA_ID) AS ord
    FROM DistinctAreas AS d
),
Base AS (
    SELECT
        a.WORK_AREA_ID,
        a.AREA,
        a.METROS2,
        a.ord,
        t.Tarefa,
        t.Descricao,
        t.Periodo,
        t.FrequenciaDias,
        freqType = CASE t.FrequenciaDias
            WHEN 1 THEN N'yearly'
            WHEN 2 THEN N'semi-annual'
            WHEN 4 THEN N'quarterly'
            WHEN 6 THEN N'bimonthly'
            WHEN 12 THEN N'monthly'
            WHEN 26 THEN N'biweekly'
            WHEN 52 THEN N'weekly'
            WHEN 260 THEN N'weekly'
            WHEN 365 THEN N'everyday'
            WHEN 208 THEN N'weekly'
            WHEN 312 THEN N'weekly'
            ELSE N'weekly'
        END,
        weekDaysJson = CASE
            WHEN t.FrequenciaDias = 312 THEN N'[1,2,3,4,5,6]'
            WHEN t.FrequenciaDias = 208 THEN N'[1,2,3,4]'
            WHEN t.FrequenciaDias IN (260, 52) AND (t.Periodo = N'LV' OR t.Periodo = N'RT') THEN N'[1,2,3,4,5]'
            WHEN t.FrequenciaDias = 0 THEN N'[1,2,3,4,5]'
            WHEN t.FrequenciaDias = 26 THEN N'[1,2,3,4,5]'
            WHEN t.FrequenciaDias = 12 THEN N'[1,2,3,4,5,6]'
            WHEN t.FrequenciaDias = 4 THEN N'[0,1,2,3,4,5,6]'
            ELSE N'[1,2,3,4,5]'
        END
    FROM Areas AS a
    INNER JOIN #Tarefas AS t ON t.WORK_AREA_ID = a.WORK_AREA_ID
),
ItemsJson AS (
    SELECT
        a.WORK_AREA_ID,
        a.AREA,
        a.METROS2,
        a.ord,
        itemsJson = (
            SELECT
                CAST(i.Tarefa AS VARCHAR(20)) AS id,
                i.Descricao AS name,
                i.Tarefa AS orderBy,
                (
                    SELECT
                        i.freqType AS type,
                        JSON_QUERY(i.weekDaysJson) AS weekDays
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                ) AS frequency
            FROM Base AS i
            WHERE i.WORK_AREA_ID = a.WORK_AREA_ID
            ORDER BY i.Tarefa
            FOR JSON PATH
        )
    FROM Areas AS a
)
SELECT (
    SELECT
        CAST(N'' AS NVARCHAR(10)) AS id,
        x.AREA AS name,
        CAST(N'' AS NVARCHAR(10)) AS description,
        CAST(0 AS BIT) AS quickTask,
        x.METROS2 AS totalM2,
        @EmployeeMongoId AS employeeId,
        CAST(x.WORK_AREA_ID AS NVARCHAR(20)) AS headerId,
        x.ord AS orderBy,
        @ProjectId AS projectId,
        JSON_QUERY(x.itemsJson) AS items
    FROM ItemsJson AS x
    ORDER BY x.WORK_AREA_ID
    FOR JSON PATH
) AS areaActivityJson;
