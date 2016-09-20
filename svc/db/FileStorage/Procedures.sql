
/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFile]
GO

CREATE PROCEDURE [FileStore].[DeleteFile]
(
	@FileId uniqueidentifier,
	@ExpiredTime datetime
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON
	
	DECLARE @CurrentTime datetime;
	SELECT @CurrentTime = GETUTCDATE();
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@CurrentTime, @ExpiredTime);

	SET NOCOUNT ON

    UPDATE [FileStore].[Files] SET ExpiredTime = @ExpiredTime
    WHERE [FileId] = @FileId

	SELECT @@ROWCOUNT
END

GO
/******************************************************************************************************************************
Name:			DeleteFileChunk

Description: 
			
Change History:
Date			Name					Change
2015/12/03		Albert					Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[DeleteFileChunk]
GO

CREATE PROCEDURE [FileStore].[DeleteFileChunk]
(
	@FileId uniqueidentifier,
	@ChunkNumber int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

    DELETE FROM [FileStore].[FileChunks] 
    WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNumber

	SELECT @@ROWCOUNT
END

GO
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
/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[GetStatus]
GO

CREATE PROCEDURE [FileStore].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [FileStore].[DbVersionInfo] WHERE [Id] = 1;       
END

GO
/******************************************************************************************************************************
Name:			HeadFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[HeadFile]
GO

CREATE PROCEDURE [FileStore].[HeadFile]
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
	,[FileSize]
	FROM [FileStore].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			[InsertFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Renamed procedure
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileHead]
GO

CREATE PROCEDURE [FileStore].[InsertFileHead]
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
	SET @ExpiredTime = [FileStore].[ValidateExpiryTime](@StoredTime, @ExpiredTime);

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    INSERT INTO [FileStore].[Files]  
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

/******************************************************************************************************************************
Name:			[InsertFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[InsertFileChunk]
GO

CREATE PROCEDURE [FileStore].[InsertFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int,
	@ChunkSize int,
	@ChunkContent varbinary(max)
)
AS
BEGIN

    INSERT INTO [FileStore].[FileChunks]  
           ([FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent])
    VALUES
           (@FileId
           ,@ChunkNum
           ,@ChunkSize
           ,@ChunkContent)
END

GO

/******************************************************************************************************************************
Name:			[ReadFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadFileChunk]
GO

CREATE PROCEDURE [FileStore].[ReadFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [FileId]
           ,[ChunkNum]
           ,[ChunkSize]
		   ,[ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO

/******************************************************************************************************************************
Name:			[UpdateFileHead]

Description: 
			
Change History:
Date			Name					Change
2015/11/23		Albert Wong				Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[UpdateFileHead]
GO

CREATE PROCEDURE [FileStore].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[FileSize] = @FileSize,
		[ChunkCount] = @ChunkCount 
	WHERE 
		[FileId] = @FileId;
END

GO

/******************************************************************************************************************************
Name:			[ReadChunkContent]

Description: 
			
Change History:
Date			Name					Change
2015/11/24		CRichards				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[ReadChunkContent]
GO

CREATE PROCEDURE [FileStore].[ReadChunkContent]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [ChunkContent]
	FROM [FileStore].[FileChunks]
	WHERE [FileId] = @FileId AND [ChunkNum] = @ChunkNum

END

GO
/******************************************************************************************************************************
Name:			[MakeFilePermanent]

Description: 
			
Change History:
Date			Name					Change
2016/09/13		Alexander Utkin		    Initial
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[FileStore].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [FileStore].[MakeFilePermanent]
GO

CREATE PROCEDURE [FileStore].[MakeFilePermanent]
( 
    @FileId uniqueidentifier
)
AS
BEGIN

	UPDATE 
		[FileStore].[Files]
    SET
		[ExpiredTime] = NULL
	WHERE 
		[FileId] = @FileId;

	SELECT @@ROWCOUNT
END

GO
