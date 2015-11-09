/******************************************************************************************************************************
Name:			EndSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndSession]
GO

CREATE PROCEDURE [dbo].[EndSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
	UPDATE [dbo].[Sessions] SET BeginTime = NULL, EndTime = @EndTime where SessionId = @SessionId;
	COMMIT TRANSACTION;
END
GO

--GRANT  EXECUTE  ON [dbo].[EndSession]  TO [Blueprint]
--GO
