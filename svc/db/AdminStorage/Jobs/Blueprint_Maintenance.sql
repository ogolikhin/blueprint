DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @adminStorage SYSNAME, @fileStorage SYSNAME, @owner_login_name SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = N'Blueprint' -- REPLACE --
SET @owner_login_name = N'sa' -- REPLACE --
SET @jobname = @blueprintDB+N'_Maintenance'
SET @adminStorage = @blueprintDB+N'_AdminStorage'
SET @fileStorage = @blueprintDB+N'_FileStorage'

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
		@description=N'Blueprint maintenance', 
		@owner_login_name=@owner_login_name, @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 1 - Delete expired files from FileStorage
SET @cmd = N'
-- Log a message
EXEC ' + @adminStorage + N'.[dbo].[WriteDBLog] N''Executing Job: ' + @jobname + N' - Delete expired files from FileStorage''

-- Delete files
DELETE FROM [dbo].[Files] Where ExpiredTime <= GETDATE()'
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete expired files from FileStorage', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@fileStorage, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 2 - Delete old logs from AdminStorage
SET @cmd = N'
-- Log a message
EXEC ' + @adminStorage + N'.[dbo].[WriteDBLog] N''Executing Job: ' + @jobname + N' - Delete old logs from AdminStorage''

-- Delete log entries
EXECUTE [dbo].[DeleteLogs] '
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete old logs from AdminStorage', 
		@step_id=2, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@adminStorage, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Schedule
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Blueprint_Maintenance_Schedule', 
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
		@active_end_time=235959, 
		@schedule_uid=N'14e05a95-0af9-4b9e-8549-e38cd0f91d11'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO


