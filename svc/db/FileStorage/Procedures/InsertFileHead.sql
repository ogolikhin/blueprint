/******************************************************************************************************************************
Name:			[InsertFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Renamed procedure
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileHead]
GO

CREATE PROCEDURE [dbo].[InsertFileHead]
( 
    @FileName nvarchar(256),
    @FileType nvarchar(64),
    @ExpiredTime datetime,
	@ChunkCount int,
	@FileSize bigint,
	@FileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @StoredTime datetime;
	SET @StoredTime = GETUTCDATE();
	SET @ExpiredTime = [dbo].[ValidateExpiryTime](@StoredTime, @ExpiredTime);

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    INSERT INTO [dbo].[Files]  
           ([StoredTime]
           ,[FileName]
           ,[FileType]
           ,[ExpiredTime]
           ,[ChunkCount]
           ,[FileSize])
	OUTPUT INSERTED.FileId INTO @op
    VALUES
           (@StoredTime
           ,@FileName
           ,@FileType
           ,@ExpiredTime
           ,@ChunkCount
		   ,@FileSize)
	SELECT  @FileId = t.ColGuid FROM @op t
END

GO
