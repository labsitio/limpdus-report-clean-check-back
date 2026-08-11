/*
  Hierarquia de níveis/grupos — limpcalc (sql2.limpidus.com.br)

  Tabelas:
    TBL_NIVEIS_GRUPO  (árvore: FATHER_ID; raiz = -1)
    TBL_NIVEIS        (nome/ordem/tipo do nível)
    TBL_NIVEIS_TIPO   (nome do tipo)
    FRANQ_LOGIN       (usuário no nó: TBL_NIVEIS_GRUPO_ID / TBL_NIVEIS_ID)
    GRUPOS_USER       (ID_FRANQ -> ID_TBLGRUPOS)
    TBL_GRUPOS        (nome do grupo)

  Observação: OUTER JOIN não é permitido na parte recursiva do CTE no SQL Server;
  por isso TBL_NIVEIS entra via subquery escalar no PATH_NOMES e JOIN só no SELECT final.
*/

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

------------------------------------------------------------
-- 1) Árvore completa (path, profundidade, nível, tipo, qtd usuários)
------------------------------------------------------------
;WITH Hierarquia AS (
    SELECT
        g.TBL_NIVEIS_GRUPO_ID,
        g.FATHER_ID,
        g.CHILD_ID,
        g.NIVEL AS NIVEL_CAMPO,
        g.TBL_NIVEIS_ID,
        CAST(CAST(g.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20)) AS NVARCHAR(MAX)) AS PATH_IDS,
        CAST(
            ISNULL(
                (SELECT n0.TBL_NIVEIS_NOME FROM dbo.TBL_NIVEIS n0 WHERE n0.TBL_NIVEIS_ID = g.TBL_NIVEIS_ID),
                N'#' + CAST(g.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20))
            ) AS NVARCHAR(MAX)
        ) AS PATH_NOMES,
        0 AS PROFUNDIDADE
    FROM dbo.TBL_NIVEIS_GRUPO AS g
    WHERE g.FATHER_ID = -1
       OR g.FATHER_ID IS NULL

    UNION ALL

    SELECT
        c.TBL_NIVEIS_GRUPO_ID,
        c.FATHER_ID,
        c.CHILD_ID,
        c.NIVEL,
        c.TBL_NIVEIS_ID,
        CAST(h.PATH_IDS + N' > ' + CAST(c.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20)) AS NVARCHAR(MAX)),
        CAST(
            h.PATH_NOMES + N' > ' + ISNULL(
                (SELECT n1.TBL_NIVEIS_NOME FROM dbo.TBL_NIVEIS n1 WHERE n1.TBL_NIVEIS_ID = c.TBL_NIVEIS_ID),
                N'#' + CAST(c.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20))
            ) AS NVARCHAR(MAX)
        ),
        h.PROFUNDIDADE + 1
    FROM dbo.TBL_NIVEIS_GRUPO AS c
    INNER JOIN Hierarquia AS h
        ON c.FATHER_ID = h.TBL_NIVEIS_GRUPO_ID
)
SELECT
    h.PROFUNDIDADE,
    REPLICATE(N'|  ', h.PROFUNDIDADE) + N'+- ' + ISNULL(n.TBL_NIVEIS_NOME, N'(sem nome)') AS ARVORE,
    h.PATH_IDS,
    h.PATH_NOMES,
    h.TBL_NIVEIS_GRUPO_ID,
    h.FATHER_ID,
    h.CHILD_ID,
    h.NIVEL_CAMPO,
    h.TBL_NIVEIS_ID,
    n.TBL_NIVEIS_NOME,
    n.TBL_NIVEIS_ORDEM,
    t.TBL_NIVEIS_TIPO_ID,
    t.TBL_NIVEIS_TIPO_NOME,
    (
        SELECT COUNT(*)
        FROM dbo.FRANQ_LOGIN AS fl
        WHERE fl.TBL_NIVEIS_GRUPO_ID = h.TBL_NIVEIS_GRUPO_ID
    ) AS QTD_USUARIOS_NO_NO,
    (
        SELECT COUNT(*)
        FROM dbo.FRANQ_LOGIN AS fl
        WHERE fl.TBL_NIVEIS_GRUPO_ID = h.TBL_NIVEIS_GRUPO_ID
          AND ISNULL(fl.ATIVO, 0) = 1
    ) AS QTD_USUARIOS_ATIVOS
FROM Hierarquia AS h
LEFT JOIN dbo.TBL_NIVEIS AS n
    ON n.TBL_NIVEIS_ID = h.TBL_NIVEIS_ID
LEFT JOIN dbo.TBL_NIVEIS_TIPO AS t
    ON t.TBL_NIVEIS_TIPO_ID = n.TBL_NIVEIS_TIPO
ORDER BY h.PATH_IDS
OPTION (MAXRECURSION 100);

------------------------------------------------------------
-- 2) Usuários em cada nó + grupos (GRUPOS_USER / TBL_GRUPOS)
------------------------------------------------------------
;WITH Hierarquia AS (
    SELECT
        g.TBL_NIVEIS_GRUPO_ID,
        g.FATHER_ID,
        g.TBL_NIVEIS_ID,
        CAST(CAST(g.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20)) AS NVARCHAR(MAX)) AS PATH_IDS,
        CAST(
            ISNULL(
                (SELECT n0.TBL_NIVEIS_NOME FROM dbo.TBL_NIVEIS n0 WHERE n0.TBL_NIVEIS_ID = g.TBL_NIVEIS_ID),
                N'#' + CAST(g.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20))
            ) AS NVARCHAR(MAX)
        ) AS PATH_NOMES,
        0 AS PROFUNDIDADE
    FROM dbo.TBL_NIVEIS_GRUPO AS g
    WHERE g.FATHER_ID = -1
       OR g.FATHER_ID IS NULL

    UNION ALL

    SELECT
        c.TBL_NIVEIS_GRUPO_ID,
        c.FATHER_ID,
        c.TBL_NIVEIS_ID,
        CAST(h.PATH_IDS + N' > ' + CAST(c.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20)) AS NVARCHAR(MAX)),
        CAST(
            h.PATH_NOMES + N' > ' + ISNULL(
                (SELECT n1.TBL_NIVEIS_NOME FROM dbo.TBL_NIVEIS n1 WHERE n1.TBL_NIVEIS_ID = c.TBL_NIVEIS_ID),
                N'#' + CAST(c.TBL_NIVEIS_GRUPO_ID AS NVARCHAR(20))
            ) AS NVARCHAR(MAX)
        ),
        h.PROFUNDIDADE + 1
    FROM dbo.TBL_NIVEIS_GRUPO AS c
    INNER JOIN Hierarquia AS h
        ON c.FATHER_ID = h.TBL_NIVEIS_GRUPO_ID
),
GruposAgg AS (
    SELECT
        gu.ID_FRANQ,
        STRING_AGG(CAST(tg.ID AS NVARCHAR(20)) + N':' + ISNULL(tg.NOME, N'?'), N' | ')
            WITHIN GROUP (ORDER BY tg.NOME) AS GRUPOS
    FROM dbo.GRUPOS_USER AS gu
    LEFT JOIN dbo.TBL_GRUPOS AS tg
        ON tg.ID = gu.ID_TBLGRUPOS
    GROUP BY gu.ID_FRANQ
)
SELECT
    h.PROFUNDIDADE,
    h.PATH_IDS,
    h.PATH_NOMES,
    h.TBL_NIVEIS_GRUPO_ID,
    h.FATHER_ID,
    h.TBL_NIVEIS_ID,
    n.TBL_NIVEIS_NOME,
    t.TBL_NIVEIS_TIPO_NOME,
    fl.ID                AS FRANQ_LOGIN_ID,
    fl.NOME              AS USUARIO_NOME,
    fl.LOGIN             AS USUARIO_LOGIN,
    fl.EMAIL,
    fl.ATIVO,
    fl.TBL_NIVEIS_ID     AS USUARIO_TBL_NIVEIS_ID,
    fl.TBL_NIVEIS_GRUPO_ID AS USUARIO_TBL_NIVEIS_GRUPO_ID,
    ga.GRUPOS
FROM Hierarquia AS h
LEFT JOIN dbo.TBL_NIVEIS AS n
    ON n.TBL_NIVEIS_ID = h.TBL_NIVEIS_ID
LEFT JOIN dbo.TBL_NIVEIS_TIPO AS t
    ON t.TBL_NIVEIS_TIPO_ID = n.TBL_NIVEIS_TIPO
LEFT JOIN dbo.FRANQ_LOGIN AS fl
    ON fl.TBL_NIVEIS_GRUPO_ID = h.TBL_NIVEIS_GRUPO_ID
LEFT JOIN GruposAgg AS ga
    ON ga.ID_FRANQ = fl.ID
ORDER BY h.PATH_IDS, fl.NOME
OPTION (MAXRECURSION 100);

------------------------------------------------------------
-- 3) Diagnóstico: nós órfãos (FATHER_ID sem pai existente)
------------------------------------------------------------
SELECT
    g.TBL_NIVEIS_GRUPO_ID,
    g.FATHER_ID,
    g.TBL_NIVEIS_ID,
    n.TBL_NIVEIS_NOME,
    N'Órfão: FATHER_ID não encontrado em TBL_NIVEIS_GRUPO' AS PROBLEMA
FROM dbo.TBL_NIVEIS_GRUPO AS g
LEFT JOIN dbo.TBL_NIVEIS AS n
    ON n.TBL_NIVEIS_ID = g.TBL_NIVEIS_ID
LEFT JOIN dbo.TBL_NIVEIS_GRUPO AS p
    ON p.TBL_NIVEIS_GRUPO_ID = g.FATHER_ID
WHERE g.FATHER_ID IS NOT NULL
  AND g.FATHER_ID <> -1
  AND p.TBL_NIVEIS_GRUPO_ID IS NULL;