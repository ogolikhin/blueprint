/******************************************************************************************************************************
Name:			PostFileHead

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PostFileHead]
GO

CREATE PROCEDURE [dbo].[PostFileHead]
( 
    @FileName nvarchar(256),
    @FileType nvarchar(64),
	@ChunkCount int,
	@FileSize bigint,
	@FileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    INSERT INTO [dbo].[Files]  
           ([StoredTime]
           ,[FileName]
           ,[FileType]
           ,[ChunkCount]
           ,[FileSize])
	OUTPUT INSERTED.FileId INTO @op
    VALUES
           (GETDATE()
           ,@FileName
           ,@FileType
           ,@ChunkCount
		   ,@FileSize)
	SELECT  @FileId = t.ColGuid FROM @op t
END

GO
