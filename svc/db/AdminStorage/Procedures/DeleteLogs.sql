IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[DeleteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[DeleteLogs]
GO

CREATE PROCEDURE [AdminStore].[DeleteLogs] 
AS
BEGIN
  -- Get the number of days to keep from config settings - DEFAULT 7
  DECLARE @days int
  SELECT @days=c.[Value] FROM ConfigSettings c WHERE c.[Key] = N'DaysToKeepInLogs'
  SELECT @days=COALESCE(@days, 7) 

  -- Delete old log records
  DELETE FROM [Logs] WHERE [Timestamp] <= DATEADD(DAY, @days*-1, SYSDATETIMEOFFSET()) 
END

GO

