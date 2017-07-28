
-- -----------------------------------------------------------------------------------------------
-- Migration 8.2.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([dbo].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0) 
	set noexec on
Print 'Migrating 8.2.0.0 ...'
-- -----------------------------------------------------------------------------------------------

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

DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
-- We can't do the following line, because we don't have access to the table in Amazon RDS:
--      IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN TRY
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END TRY
BEGIN CATCH
END CATCH

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
		@description=N'Blueprint admin storage maintenance', 
		@job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 1 - Delete old logs from AdminStorage
SET @cmd = N'[AdminStore].[DeleteLogs]'
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete old logs from AdminStorage', 
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

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'8.2.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------
