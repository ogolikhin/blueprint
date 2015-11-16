/******************************************************************************************************************************
Name:			PostFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PostFile]
GO

CREATE PROCEDURE [dbo].[PostFile]
( 
    @FileName nvarchar(256),
    @FileType nvarchar(64),
    @FileContent varbinary(max),
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
           ,[FileContent]
           ,[FileSize])
	OUTPUT INSERTED.FileId INTO @op
    VALUES
           (GETDATE()
           ,@FileName
           ,@FileType
           ,@FileContent
		   ,DATALENGTH(@FileContent))
	SELECT  @FileId = t.ColGuid FROM @op t
END

GO
