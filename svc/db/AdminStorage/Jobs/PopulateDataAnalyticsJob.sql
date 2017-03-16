
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Alex Tkachev
-- Create date: March 16, 2017
-- Description:	Create Data Analytics Job
-- =============================================
CREATE PROCEDURE PopulateDataAnalyticsJob
	@db_name nvarchar(128)
AS
BEGIN
	/****** Object:  Job [PopulateDataAnalytics]    Script Date: 3/16/2017 9:02:13 AM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 3/16/2017 9:02:14 AM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
DECLARE @databaseName nvarchar(128) = CONCAT(@db_name, N'Analytics');
DECLARE @jobName nvarchar(128) = CONCAT(@db_name, N'_PopulateDataAnalytics');

-- If Job exists then delete it
BEGIN TRY
    EXEC [msdb].dbo.sp_delete_job  @job_name = @jobName ;  
END TRY
BEGIN CATCH
--	PRINT N'NO JOB with this name'
END CATCH;

/********  Create a new job   ************/
EXEC @ReturnCode =  msdb.dbo.sp_add_job 
		@job_name=@jobName, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/****** Object:  Step [Run PopulateDataAnalytics]    Script Date: 3/16/2017 9:02:14 AM ******/
DECLARE @cmd nvarchar(max) = N'[dbw].PopulateDataAnalytics';

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep 
		@job_id=@jobId, 
		@step_name=N'Run PopulateDataAnalytics', 
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
		@flags=4

IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

/******      Schedule the job       ******/
DECLARE @scheduleName nvarchar(128) = CONCAT(@db_name , N'_PopulateDataAnalytics');

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule 
		@job_id=@jobId, 
		@name=@scheduleName, 
		@enabled=0, 
		@freq_type=1, 
		@freq_interval=0, 
		@freq_subday_type=0, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20170309, 
		@active_end_date=99991231, 
		@active_start_time=190000, 
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
