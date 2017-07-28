-- Drop all functions associated with dbo schema
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual]
GO

-- Drop all procedures associated with dbo schema
DECLARE @name AS sysname, @tsql AS nvarchar(max), @beginCount AS int, @endCount AS int;
DECLARE NameCursor CURSOR FOR
SELECT o.[name] FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
WHILE (0 = 0)
BEGIN
	SELECT @beginCount = COUNT(*) FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
	OPEN NameCursor;
	FETCH NEXT FROM NameCursor INTO @name;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		SET @tsql = N'DROP PROCEDURE [dbo].[' + CAST(@name AS nvarchar(256)) + N'];';
		BEGIN TRY
			EXEC(@tsql);
		END TRY
		BEGIN CATCH
		END CATCH
		FETCH NEXT FROM NameCursor INTO @name;
	END
	CLOSE NameCursor;
	SELECT @endCount = COUNT(*) FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
	IF ((@endCount = 0) OR (@endCount = @beginCount))
		BREAK;
END
DEALLOCATE NameCursor;
GO

-- Drop all types associated with dbo schema
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogsType' AND ss.name = N'dbo')
DROP TYPE [dbo].[LogsType]
GO
