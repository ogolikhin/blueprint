/******************************************************************************************************************************
Name:			GetLogs

Description:	Returns last @limit records from Logs table
			
Change History:
Date			Name					Change
Feb 25 2016		Dmitry Lopyrev			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLogs]
GO


CREATE PROCEDURE [dbo].[GetLogs]  
(
  @limit int = 0
)
AS
BEGIN
	DECLARE @total Int
	SELECT @total = COUNT(*) FROM [Logs]	
	
	IF @limit > 0 AND @limit <= @total
	BEGIN
		SELECT * FROM [Logs] ORDER BY id 
			OFFSET @total - @limit ROWS
			FETCH NEXT @limit ROWS ONLY;
	END
	ELSE
		SELECT * FROM [Logs] ORDER BY id 
	
END



GO


