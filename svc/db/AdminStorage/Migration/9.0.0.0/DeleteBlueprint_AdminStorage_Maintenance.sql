DECLARE @blueprintDBDelete SYSNAME, @jobnameDelete SYSNAME

SET @blueprintDBDelete = DB_NAME()
SET @jobnameDelete = @blueprintDBDelete+N'_Maintenance'

-- drop the job if it exists
-- We can't do the following line, because we don't have access to the table in Amazon RDS:
--      IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobnameDelete)
BEGIN TRY
	EXEC msdb.dbo.sp_delete_job @job_name=@jobnameDelete, @delete_unused_schedule=1
END TRY
BEGIN CATCH
END CATCH
