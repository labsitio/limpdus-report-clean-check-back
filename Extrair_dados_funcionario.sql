SELECT TOP 1
	 '' AS [firstName]
	,'' AS [lastName]
	,PROJ_FUN.FUNCIONARIO AS [number]
	,PROJ_FUN.OBS AS [observation]
	,PROJ_FUN.WORK_HEADER_ID AS [projectId]
FROM
	WORK_FUNCIONARIO AS PROJ_FUN
--FOR JSON AUTO
;
