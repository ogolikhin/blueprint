/******************************************************************************************************************************
Name:			GetFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFile]
GO

CREATE PROCEDURE [dbo].[GetFile]
(
	@FileId uniqueidentifier
)
AS
BEGIN
 	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[FileName]
	,[FileType]
	,[FileContent]
	,[FileSize]
	FROM [dbo].[Files]
	WHERE [FileId] = @FileId
END

GO

--GRANT  EXECUTE  ON [dbo].[GetFile]  TO [Blueprint]
--GO
