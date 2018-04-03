DECLARE @blueprintDB SYSNAME, @jobname SYSNAME

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'

-- drop the job if it exists
-- We can't do the following line, because we don't have access to the table in Amazon RDS:
--      IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN TRY
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END TRY
BEGIN CATCH
END CATCH
