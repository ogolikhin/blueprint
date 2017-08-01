/******************************************************************************************************************************
Name:			GetLogs

Description:	Returns last @limit records from Logs table
			
Change History:
Date			Name					Change
Feb 25 2016		Dmitry Lopyrev			Initial Version
Jun 7 2016		Dmitry Lopyrev			Updated
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLogs]
GO


CREATE PROCEDURE [AdminStore].[GetLogs]  
(
  @recordlimit int,
  @recordid int = null
)
AS
BEGIN
	DECLARE @total int
	DECLARE @fetch int
	DECLARE @id int

	SET @id = IsNULL(@recordid, 0);

	SELECT @total = COUNT(*) FROM [Logs] where @id = 0 OR ID <= @id 	

	SET @fetch = IIF(@recordlimit < 0, @total, @recordlimit)

	SELECT TOP (@fetch) * FROM [Logs] WHERE @id = 0 OR ID <= @id ORDER BY Id DESC

END



GO


