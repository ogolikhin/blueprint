-- Drop all functions associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
	AND (OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]', 'FN') IS NOT NULL)
	DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual];
GO

-- Drop all procedures associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
BEGIN
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
END
GO

-- Drop all types associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
	AND (TYPE_ID(N'[dbo].[LogsType]') IS NOT NULL)
	DROP TYPE [dbo].[LogsType];
GO
