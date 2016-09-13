
/******************************************************************************************************************************
Name:			SetSchemaVersion

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetSchemaVersion]
GO

CREATE PROCEDURE [dbo].[SetSchemaVersion]
(
	@value AS nvarchar(max)
)
AS
PRINT 'Setting Schema Version to ' + @value;
-- Integrity check
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);

IF EXISTS (SELECT * FROM [dbo].[DbVersionInfo])
	BEGIN 
		UPDATE [dbo].[DbVersionInfo] SET [SchemaVersion] = @value FROM [dbo].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [dbo].[DbVersionInfo] SELECT 1, @value;
	END 

GO
/******************************************************************************************************************************
Name:			DeleteFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFile]
GO

CREATE PROCEDURE [dbo].[DeleteFile]
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
	SET @ExpiredTime = [dbo].[ValidateExpiryTime](@CurrentTime, @ExpiredTime);

	SET NOCOUNT ON

    UPDATE [dbo].[Files] SET ExpiredTime = @ExpiredTime
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFileChunk]
GO

CREATE PROCEDURE [dbo].[DeleteFileChunk]
(
	@FileId uniqueidentifier,
	@ChunkNumber int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

    DELETE FROM [dbo].[FileChunks] 
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileHead]
GO

CREATE PROCEDURE [dbo].[ReadFileHead]
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
	FROM [dbo].[Files]
	WHERE [FileId] = @FileId
END

GO
/******************************************************************************************************************************
Name:			GetStatus

Description:    Returns the version of the database.
			
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus]
AS
BEGIN
       SELECT [SchemaVersion] FROM [dbo].[DbVersionInfo] WHERE [Id] = 1;       
END

GO
/******************************************************************************************************************************
Name:			HeadFile

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HeadFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[HeadFile]
GO

CREATE PROCEDURE [dbo].[HeadFile]
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
	FROM [dbo].[Files]
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

/******************************************************************************************************************************
Name:			[InsertFileChunk]

Description: 
			
Change History:
Date			Name					Change
2015/11/19		Albert Wong				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFileChunk]
GO

CREATE PROCEDURE [dbo].[InsertFileChunk]
( 
    @FileId uniqueidentifier,
    @ChunkNum int,
	@ChunkSize int,
	@ChunkContent varbinary(max)
)
AS
BEGIN

    INSERT INTO [dbo].[FileChunks]  
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadFileChunk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadFileChunk]
GO

CREATE PROCEDURE [dbo].[ReadFileChunk]
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
	FROM [dbo].[FileChunks]
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateFileHead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateFileHead]
GO

CREATE PROCEDURE [dbo].[UpdateFileHead]
( 
    @FileId uniqueidentifier,
	@FileSize bigint,
	@ChunkCount int
)
AS
BEGIN

	UPDATE 
		[dbo].[Files]
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadChunkContent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadChunkContent]
GO

CREATE PROCEDURE [dbo].[ReadChunkContent]
( 
    @FileId uniqueidentifier,
    @ChunkNum int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	SELECT [ChunkContent]
	FROM [dbo].[FileChunks]
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MakeFilePermanent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MakeFilePermanent]
GO

CREATE PROCEDURE [dbo].[MakeFilePermanent]
( 
    @FileId uniqueidentifier
)
AS
BEGIN

	UPDATE 
		[dbo].[Files]
    SET
		[ExpiredTime] = NULL
	WHERE 
		[FileId] = @FileId;

	SELECT @@ROWCOUNT
END

GO
