/******************************************************************************************************************************
Name:			ReadFileHead

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileHead]
GO

CREATE PROCEDURE [FileStore].[ReadFileHead]
(
	@FileId uniqueidentifier
)
AS
BEGIN
 	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
	,[StoredTime]
	,[ExpiredTime]
	,[FileName]
	,[FileType]
	,[ChunkCount]
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO