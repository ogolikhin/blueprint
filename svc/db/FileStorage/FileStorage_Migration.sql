
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_FileStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
GO
-- --------------------------------------------------

-- Create Blueprint Roles
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_reader' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_reader]
	GRANT SELECT TO db_blueprint_reader
End
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_writer' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_writer]
	GRANT DELETE, INSERT, UPDATE TO db_blueprint_writer
End
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_executor' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_executor]
	GRANT EXECUTE TO db_blueprint_executor
End
GO


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

DECLARE @db_name AS nvarchar(128) = DB_NAME();
DECLARE @sql AS nvarchar(max);

SET @sql = N'ALTER DATABASE [' + @db_name + N'] SET COMPATIBILITY_LEVEL = 110'; -- SQL Server 2012
EXEC(@sql);

-- Create the FileStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'FileStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [FileStore]'
GO


-- Create the FileStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'FileStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [FileStore]'
GO

-- Migrate tables to the FileStore schema
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DbVersionInfo]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[DbVersionInfo]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Files]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[Files]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileChunks]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[FileChunks]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MigrationLog]') AND type in (N'U'))
ALTER SCHEMA FileStore TRANSFER [dbo].[MigrationLog]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P'))
ALTER SCHEMA FileStore TRANSFER [dbo].[DeleteFile]
GO



/******************************************************************************************************************************
Name:			IsSchemaVersionLessOrEqual

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [FileStore].[IsSchemaVersionLessOrEqual]
GO

CREATE FUNCTION [FileStore].[IsSchemaVersionLessOrEqual]
(
	@value AS nvarchar(max)
)
RETURNS bit
AS
BEGIN
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);
WHILE (@value4 IS NULL)
BEGIN
	SET @value4 = @value3;
	SET @value3 = @value2;
	SET @value2 = @value1;
	SET @value1 = 0;
END;

DECLARE @schemaVersion AS nvarchar(max);
SELECT TOP(1) @schemaVersion = [SchemaVersion] FROM [FileStore].[DbVersionInfo] WHERE ([SchemaVersion] IS NOT NULL);
DECLARE @schemaVersion1 AS int = CAST(PARSENAME(@schemaVersion, 1) AS int);
DECLARE @schemaVersion2 AS int = CAST(PARSENAME(@schemaVersion, 2) AS int);
DECLARE @schemaVersion3 AS int = CAST(PARSENAME(@schemaVersion, 3) AS int);
DECLARE @schemaVersion4 AS int = CAST(PARSENAME(@schemaVersion, 4) AS int);
WHILE (@schemaVersion4 IS NULL)
BEGIN
	SET @schemaVersion4 = @schemaVersion3;
	SET @schemaVersion3 = @schemaVersion2;
	SET @schemaVersion2 = @schemaVersion1;
	SET @schemaVersion1 = 0;
END;

RETURN CASE WHEN
	((@schemaVersion4 > @value4) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 > @value3)) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 = @value3) AND (@schemaVersion2 > @value2)) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 = @value3) AND (@schemaVersion2 = @value2) AND (@schemaVersion1 > @value1)))
THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END;
END

GO
/******************************************************************************************************************************
Name:			SetSchemaVersion

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[SetSchemaVersion]
GO

CREATE PROCEDURE [FileStore].[SetSchemaVersion]
(
	@value AS nvarchar(max)
)
AS
PRINT 'Setting Schema Version to ' + @value;
-- Integrity check
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);

IF EXISTS (SELECT * FROM [FileStore].[DbVersionInfo])
	BEGIN 
		UPDATE [FileStore].[DbVersionInfo] SET [SchemaVersion] = @value FROM [FileStore].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [FileStore].[DbVersionInfo] SELECT 1, @value;
	END 

GO


-- --------------------------------------------------
-- Migration 7.0.1.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0) 
	set noexec on
Print 'Migrating 7.0.1.0 ...'
-- --------------------------------------------------

/******************************************************************************************************************************
Name:			SetSchemaVersion

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[SetSchemaVersion]
GO

CREATE PROCEDURE [FileStore].[SetSchemaVersion]
(
	@value AS nvarchar(max)
)
AS
PRINT 'Setting Schema Version to ' + @value;
-- Integrity check
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);

IF EXISTS (SELECT * FROM [FileStore].[DbVersionInfo])
	BEGIN 
		UPDATE [FileStore].[DbVersionInfo] SET [SchemaVersion] = @value FROM [FileStore].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [FileStore].[DbVersionInfo] SELECT 1, @value;
	END 

GO
/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFile]
GO

CREATE PROCEDURE [FileStore].[DeleteFile]
(
	@FileId uniqueidentifier,
	@ExpiredTime datetime
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON
	
	DECLARE @CurrentTime datetime;
	SELECT @CurrentTime = GETUTCDATE();
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@CurrentTime, @ExpiredTime);

	SET NOCOUNT ON

    UPDATE [FileStore].[Files] SET ExpiredTime = @ExpiredTime
    WHERE [FileId] = @FileId

	SELECT @@ROWCOUNT
END

GO
/******************************************************************************************************************************
Name:			DeleteFileChunk

Description: 
			
Change History:
Date			Name					Change
2015/12/03		Albert					Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFileChunk]
GO

CREATE PROCEDURE [FileStore].[DeleteFileChunk]
(
	@FileId uniqueidentifier,
	@ChunkNumber int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

    DELETE FROM [FileStore].[FileChunks] 
    WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNumber

	SELECT @@ROWCOUNT
END

GO
/******************************************************************************************************************************
Name:			ReadFileHead

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileHead]
GO

CREATE PROCEDURE [FileStore].[ReadFileHead]
(
	@FileId uniqueidentifier
)
AS
BEGIN
 	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[ExpiredTime]
	,[FileName]
	,[FileType]
	,[ChunkCount]
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[GetStatus]
GO

CREATE PROCEDURE [FileStore].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [FileStore].[DbVersionInfo] WHERE [Id] = 1;       
END

GO
/******************************************************************************************************************************
Name:			HeadFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[HeadFile]
GO

CREATE PROCEDURE [FileStore].[HeadFile]
(
	@FileId uniqueidentifier
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[ExpiredTime]
	,[FileName]
	,[FileType]
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			[InsertFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Renamed procedure
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileHead]
GO

CREATE PROCEDURE [FileStore].[InsertFileHead]
( 
    @FileName nvarchar(256),
    @FileType nvarchar(64),
    @ExpiredTime datetime,
	@ChunkCount int,
	@FileSize bigint,
	@FileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @StoredTime datetime;
	SET @StoredTime = GETUTCDATE();
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@StoredTime, @ExpiredTime);

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    INSERT INTO [FileStore].[Files]  
           ([StoredTime]
           ,[FileName]
           ,[FileType]
           ,[ExpiredTime]
           ,[ChunkCount]
           ,[FileSize])
	OUTPUT INSERTED.FileId INTO @op
    VALUES
           (@StoredTime
           ,@FileName
           ,@FileType
           ,@ExpiredTime
           ,@ChunkCount
		   ,@FileSize)
	SELECT  @FileId = t.ColGuid FROM @op t
END

GO

/******************************************************************************************************************************
Name:			[InsertFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileChunk]
GO

CREATE PROCEDURE [FileStore].[InsertFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int,
	@ChunkSize int,
	@ChunkContent varbinary(max)
)
AS
BEGIN

    INSERT INTO [FileStore].[FileChunks]  
           ([FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent])
    VALUES
           (@FileId
           ,@ChunkNum
           ,@ChunkSize
           ,@ChunkContent)
END

GO

/******************************************************************************************************************************
Name:			[ReadFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileChunk]
GO

CREATE PROCEDURE [FileStore].[ReadFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO

/******************************************************************************************************************************
Name:			[UpdateFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/23		Albert Wong				Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[UpdateFileHead]
GO

CREATE PROCEDURE [FileStore].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[FileSize] = @FileSize,
		[ChunkCount] = @ChunkCount 
	WHERE 
		[FileId] = @FileId;
END

GO

/******************************************************************************************************************************
Name:			[ReadChunkContent]

Description: 
			
Change History:
Date			Name					Change
2015/11/24		CRichards				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadChunkContent]
GO

CREATE PROCEDURE [FileStore].[ReadChunkContent]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO

DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END

BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

-- Add Job
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=@jobname, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Blueprint file storage maintenance', 
		@job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 1 - Delete expired files from FileStorage
SET @cmd = N'
-- Delete files
DELETE FROM [FileStore].[Files] Where ExpiredTime <= GETDATE()'
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete expired files from FileStorage', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@blueprintDB, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Schedule
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=@schedulename, 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20160101, 
		@active_end_date=99991231, 
		@active_start_time=10000, 
		@active_end_time=235959
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'7.0.1';
GO
set noexec off
-- --------------------------------------------------

-- --------------------------------------------------
-- Migration 7.1.0.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0) 
	set noexec on
Print 'Migrating 7.1.0.0 ...'
-- --------------------------------------------------


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'7.1.0';
GO
set noexec off
-- --------------------------------------------------

-- --------------------------------------------------
-- Migration 7.2.0.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'7.2.0') <> 0) 
	set noexec on
Print 'Migrating 7.2.0.0 ...'
-- --------------------------------------------------


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.2.0') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'7.2.0';
GO
set noexec off
-- --------------------------------------------------

-- --------------------------------------------------
-- Migration 7.3.0.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0) 
	set noexec on
Print 'Migrating 7.3.0.0 ...'
-- --------------------------------------------------

-- -----------------------------------------------------------------------------
-- Modify the database filegrowth if it has not changed from the prior defaults
-- -----------------------------------------------------------------------------
DECLARE @db_name AS NVARCHAR(128) = DB_NAME();
DECLARE @sql AS NVARCHAR(max);
DECLARE @file_name AS SYSNAME

SELECT @file_name = d.name FROM sys.database_files d WHERE d.type = 0 AND d.is_percent_growth = 0 AND d.growth = 1280
IF (@file_name IS NOT NULL) 
BEGIN
    SET @sql = N'ALTER DATABASE [' + @db_name + '] MODIFY FILE ( NAME = N''' + @file_name + ''', FILEGROWTH = 10% )'

    EXEC(@sql);
END 

GO

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'7.3.0';
GO
set noexec off
-- --------------------------------------------------

-- --------------------------------------------------
-- Migration 7.4.0.0
-- --------------------------------------------------
IF NOT ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0) 
	set noexec on
Print 'Migrating 7.4.0.0 ...'
-- --------------------------------------------------

-- Drop all functions associated with dbo schema
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateExpiryTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ValidateExpiryTime]
GO

-- Drop all procedures associated with dbo schema
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFileChunk]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[HeadFile]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileChunk]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileHead]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadChunkContent]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileChunk]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileHead]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetSchemaVersion]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetSchemaVersion]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MakeFilePermanent]
GO

DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END

BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

-- Add Job
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=@jobname, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Blueprint file storage maintenance', 
		@job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 1 - Delete expired files from FileStorage
SET @cmd = N'
-- Delete files
DELETE FROM [FileStore].[Files] Where ExpiredTime <= GETDATE()'
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete expired files from FileStorage', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@blueprintDB, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Schedule
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=@schedulename, 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20160101, 
		@active_end_date=99991231, 
		@active_start_time=10000, 
		@active_end_time=235959
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([FileStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
 	EXEC [FileStore].[SetSchemaVersion] @value = N'7.4.0';
GO
set noexec off
-- --------------------------------------------------

/******************************************************************************************************************************
Name:			ValidateExpiryTime

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ValidateExpiryTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [FileStore].[ValidateExpiryTime]
GO

CREATE FUNCTION [FileStore].[ValidateExpiryTime]
(
	--CurrentTime needs to be a parameter for InsertFileHead usage, to have stored time and expire time equal if set to expire now.	
	@currentTime AS datetime,
	@expiredTime AS datetime
)
RETURNS datetime
AS
BEGIN
	IF @expiredTime IS NOT NULL AND @expiredTime < @currentTime
	begin
		SET @expiredTime = @currentTime;
	end
	return @expiredTime;
END

GO

/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFile]
GO

CREATE PROCEDURE [FileStore].[DeleteFile]
(
	@FileId uniqueidentifier,
	@ExpiredTime datetime
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON
	
	DECLARE @CurrentTime datetime;
	SELECT @CurrentTime = GETUTCDATE();
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@CurrentTime, @ExpiredTime);

	SET NOCOUNT ON

    UPDATE [FileStore].[Files] SET ExpiredTime = @ExpiredTime
    WHERE [FileId] = @FileId

	SELECT @@ROWCOUNT
END

GO
/******************************************************************************************************************************
Name:			DeleteFileChunk

Description: 
			
Change History:
Date			Name					Change
2015/12/03		Albert					Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFileChunk]
GO

CREATE PROCEDURE [FileStore].[DeleteFileChunk]
(
	@FileId uniqueidentifier,
	@ChunkNumber int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

    DELETE FROM [FileStore].[FileChunks] 
    WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNumber

	SELECT @@ROWCOUNT
END

GO
/******************************************************************************************************************************
Name:			ReadFileHead

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileHead]
GO

CREATE PROCEDURE [FileStore].[ReadFileHead]
(
	@FileId uniqueidentifier
)
AS
BEGIN
 	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[ExpiredTime]
	,[FileName]
	,[FileType]
	,[ChunkCount]
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[GetStatus]
GO

CREATE PROCEDURE [FileStore].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [FileStore].[DbVersionInfo] WHERE [Id] = 1;       
END

GO
/******************************************************************************************************************************
Name:			HeadFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[HeadFile]
GO

CREATE PROCEDURE [FileStore].[HeadFile]
(
	@FileId uniqueidentifier
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[ExpiredTime]
	,[FileName]
	,[FileType]
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			[InsertFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Renamed procedure
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileHead]
GO

CREATE PROCEDURE [FileStore].[InsertFileHead]
( 
    @FileName nvarchar(256),
    @FileType nvarchar(64),
    @ExpiredTime datetime,
	@ChunkCount int,
	@FileSize bigint,
	@FileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @StoredTime datetime;
	SET @StoredTime = GETUTCDATE();
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@StoredTime, @ExpiredTime);

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    INSERT INTO [FileStore].[Files]  
           ([StoredTime]
           ,[FileName]
           ,[FileType]
           ,[ExpiredTime]
           ,[ChunkCount]
           ,[FileSize])
	OUTPUT INSERTED.FileId INTO @op
    VALUES
           (@StoredTime
           ,@FileName
           ,@FileType
           ,@ExpiredTime
           ,@ChunkCount
		   ,@FileSize)
	SELECT  @FileId = t.ColGuid FROM @op t
END

GO

/******************************************************************************************************************************
Name:			[InsertFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileChunk]
GO

CREATE PROCEDURE [FileStore].[InsertFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int,
	@ChunkSize int,
	@ChunkContent varbinary(max)
)
AS
BEGIN

    INSERT INTO [FileStore].[FileChunks]  
           ([FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent])
    VALUES
           (@FileId
           ,@ChunkNum
           ,@ChunkSize
           ,@ChunkContent)
END

GO

/******************************************************************************************************************************
Name:			[ReadFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileChunk]
GO

CREATE PROCEDURE [FileStore].[ReadFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO

/******************************************************************************************************************************
Name:			[UpdateFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/23		Albert Wong				Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[UpdateFileHead]
GO

CREATE PROCEDURE [FileStore].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[FileSize] = @FileSize,
		[ChunkCount] = @ChunkCount 
	WHERE 
		[FileId] = @FileId;
END

GO

/******************************************************************************************************************************
Name:			[ReadChunkContent]

Description: 
			
Change History:
Date			Name					Change
2015/11/24		CRichards				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadChunkContent]
GO

CREATE PROCEDURE [FileStore].[ReadChunkContent]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO
/******************************************************************************************************************************
Name:			[MakeFilePermanent]

Description: 
			
Change History:
Date			Name					Change
2016/09/13		Alexander Utkin		    Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[MakeFilePermanent]
GO

CREATE PROCEDURE [FileStore].[MakeFilePermanent]
( 
    @FileId uniqueidentifier
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[ExpiredTime] = NULL
	WHERE 
		[FileId] = @FileId;

	SELECT @@ROWCOUNT
END

GO
