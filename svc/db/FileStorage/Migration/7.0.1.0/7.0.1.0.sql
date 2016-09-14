
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
 	EXEC [FileStorage].[SetSchemaVersion] @value = N'7.0.1';
GO
set noexec off
-- --------------------------------------------------