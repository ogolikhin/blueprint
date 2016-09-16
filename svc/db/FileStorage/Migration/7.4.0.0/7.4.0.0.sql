
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