DECLARE @db_id AS int = DB_ID();
DECLARE @kills AS nvarchar(max) = N'';
SELECT @kills = @kills+N'KILL '+CAST([spid] AS nvarchar(16))+N'; ' FROM [sys].[sysprocesses] WHERE ([dbid] = @db_id) AND ([spid] <> @@SPID);
IF(LEN(@kills) > 0)
	BEGIN TRY
		EXEC(@kills);
	END TRY
	BEGIN CATCH
	END CATCH
GO

DECLARE @db_id AS int = DB_ID();
DECLARE @db_name AS nvarchar(128) = DB_NAME();
IF NOT EXISTS(SELECT * FROM [sys].[sysprocesses] WHERE ([dbid] = @db_id) AND ([spid] <> @@SPID))
	AND EXISTS(SELECT * FROM [sys].[databases] WHERE ([database_id] = @db_id) AND ([is_read_committed_snapshot_on] = 0))
	BEGIN TRY
		EXEC(N'ALTER DATABASE [' + @db_name + N'] SET ALLOW_SNAPSHOT_ISOLATION ON');
		EXEC(N'ALTER DATABASE [' + @db_name + N'] SET READ_COMMITTED_SNAPSHOT ON');
	END TRY
	BEGIN CATCH
	END CATCH
GO
