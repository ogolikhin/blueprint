
-- =========================================================
-- Author:		Alex Tkachev
-- Create date: March 14, 2017
-- Description:	Create FileStorage Maintenance Job
-- Parameters:  Instance name (like 'Blueprint')
-- =========================================================


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileStorageMaintenanceJob]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FileStorageMaintenanceJob]
GO

CREATE PROCEDURE [dbo].[FileStorageMaintenanceJob]
	@db_name nvarchar(128)
AS
BEGIN
/****** Object:  Job [Blueprint_FileStorage_Maintenance]  ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]  ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN

EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16);
DECLARE @databaseName nvarchar(128) = CONCAT(@db_name, N'_FileStorage');
DECLARE @jobName nvarchar(128) = CONCAT(@db_name, N'_FileStorage_Maintenance');

-- If Job exists then delete it
BEGIN try
    EXEC [msdb].dbo.sp_delete_job  @job_name = @jobName ;  
END TRY
BEGIN CATCH
--	PRINT N'NO JOB with this name'
END CATCH;

/********  Create a new job   ************/
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=@jobName, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'FileStorage maintenance', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/******  Step [Delete expired files from FileStorage] ******/
DECLARE @cmd nvarchar(max) = N'[FileStore].DeleteExpiredFiles';

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
		@database_name=@databaseName, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/******      Schedule the job       ******/
DECLARE @scheduleName nvarchar(128) = CONCAT(@db_name , N'_FileStorage_Maintenance_Schedule');
DECLARE @newJobId uniqueidentifier = NEWID();

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=@scheduleName, 
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
		@schedule_uid= @newJobId
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
