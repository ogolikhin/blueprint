-- =======================================================
-- Author:		Alex Tkachev
-- Create date: March 15, 2017
-- Description:	Create AdminStorage Maintenance Job
-- =======================================================
CREATE PROCEDURE AdminStorageMaintenanceJob
	@db_name nvarchar(128)
AS
BEGIN
/****** Object:  Job [Blueprint_AdminStorage_Maintenance]    Script Date: 3/15/2017 9:41:29 AM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 3/15/2017 9:41:29 AM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
DECLARE @databaseName nvarchar(128) = CONCAT(@db_name, N'_AdminStorage');
DECLARE @jobName nvarchar(128) = CONCAT(@db_name, N'_AdminStorage_Maintenance');

-- If Job exists then delete it
BEGIN try
    EXEC [msdb].dbo.sp_delete_job  @job_name = @jobName ;  
END TRY
BEGIN CATCH
--	PRINT N'No Job with this name'
END CATCH;

EXEC @ReturnCode =  msdb.dbo.sp_add_job 
		@job_name=@jobName, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Admin storage maintenance', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/****** Object:  Step [Delete old logs from AdminStorage]    Script Date: 3/15/2017 9:41:29 AM ******/
DECLARE @cmd nvarchar(max) = N'[AdminStore].[DeleteLogs]';

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep 
		@job_id=@jobId, 
		@step_name=N'Delete old logs from AdminStorage', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, 
		@subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@databaseName, 
		@flags=0

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/******      Schedule the job       ******/
DECLARE @scheduleName nvarchar(128) = CONCAT(@db_name , N'_AdminStorage_Maintenance_Schedule');

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule 
		@job_id=@jobId, 
		@name=@scheduleName, 
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

END
GO


