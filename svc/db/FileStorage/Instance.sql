
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [FileStorage];
GO
SET NOCOUNT ON;
GO
-- --------------------------------------------------

/******************************************************************************************************************************
Name:			DbVersionInfo

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DbVersionInfo]') AND type in (N'U'))
DROP TABLE [dbo].[DbVersionInfo]
GO

CREATE TABLE [dbo].[DbVersionInfo](
	[Id] [int] NOT NULL,
	[SchemaVersion] [nvarchar](32) NULL,
 CONSTRAINT [PK_DbVersionInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/******************************************************************************************************************************
Name:			Files

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__Files__FileId__117F9D94]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Files] DROP CONSTRAINT [DF__Files__FileId__117F9D94]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Files]') AND type in (N'U'))
DROP TABLE [dbo].[Files]
GO

CREATE TABLE [dbo].[Files](
	[FileId] [uniqueidentifier] NOT NULL,
	[StoredTime] [datetime] NOT NULL,
	[FileName] [nvarchar](256) NOT NULL,
	[FileType] [nvarchar](128) NOT NULL,
	[FileContent] [varbinary](max) NULL,
	[FileSize] [bigint] NOT NULL,
 CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Files] ADD  DEFAULT (newsequentialid()) FOR [FileId]
GO


/******************************************************************************************************************************
Name:			IsSchemaVersionLessOrEqual

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual]
GO

CREATE FUNCTION [dbo].[IsSchemaVersionLessOrEqual]
(
	@value AS nvarchar(max)
)
RETURNS bit
AS
BEGIN
DECLARE @value1 AS int = CAST(PARSENAME(@value, 1) AS int);
DECLARE @value2 AS int = CAST(PARSENAME(@value, 2) AS int);
DECLARE @value3 AS int = CAST(PARSENAME(@value, 3) AS int);
DECLARE @value4 AS int = CAST(PARSENAME(@value, 4) AS int);
WHILE (@value4 IS NULL)
BEGIN
	SET @value4 = @value3;
	SET @value3 = @value2;
	SET @value2 = @value1;
	SET @value1 = 0;
END;

DECLARE @schemaVersion AS nvarchar(max);
SELECT TOP(1) @schemaVersion = [SchemaVersion] FROM [dbo].[DbVersionInfo] WHERE ([SchemaVersion] IS NOT NULL);
DECLARE @schemaVersion1 AS int = CAST(PARSENAME(@schemaVersion, 1) AS int);
DECLARE @schemaVersion2 AS int = CAST(PARSENAME(@schemaVersion, 2) AS int);
DECLARE @schemaVersion3 AS int = CAST(PARSENAME(@schemaVersion, 3) AS int);
DECLARE @schemaVersion4 AS int = CAST(PARSENAME(@schemaVersion, 4) AS int);
WHILE (@schemaVersion4 IS NULL)
BEGIN
	SET @schemaVersion4 = @schemaVersion3;
	SET @schemaVersion3 = @schemaVersion2;
	SET @schemaVersion2 = @schemaVersion1;
	SET @schemaVersion1 = 0;
END;

RETURN CASE WHEN
	((@schemaVersion4 > @value4) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 > @value3)) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 = @value3) AND (@schemaVersion2 > @value2)) OR
	((@schemaVersion4 = @value4) AND (@schemaVersion3 = @value3) AND (@schemaVersion2 = @value2) AND (@schemaVersion1 > @value1)))
THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END;
END

GO

--GRANT  EXECUTE  ON [dbo].[IsSchemaVersionLessOrEqual]  TO [Blueprint]
--GO


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

--GRANT  EXECUTE  ON [dbo].[SetSchemaVersion]  TO [Blueprint]
--GO

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
	@DeletedFileId AS uniqueidentifier OUTPUT
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON

	DECLARE @op TABLE (ColGuid uniqueidentifier)
    DELETE FROM [dbo].[Files]
	OUTPUT DELETED.FileId INTO @op
    WHERE [FileId] = @FileId
	SELECT  @DeletedFileId = t.ColGuid FROM @op t
END

GO

--GRANT  EXECUTE  ON [dbo].[DeleteFile]  TO [Blueprint]
--GO

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

/******************************************************************************************************************************
Name:			GetStatus

Description: 
			
Change History:
Date			Name					Change
2015/10/28		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus]
AS
BEGIN
       SELECT TOP 1 COUNT(*) FROM [dbo].[Files];       
END

GO

--GRANT  EXECUTE  ON [dbo].[GetStatus]  TO [Blueprint]
--GO

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
	,[FileName]
	,[FileType]
	,[FileSize]
	FROM [dbo].[Files]
	WHERE [FileId] = @FileId
END

GO

--GRANT  EXECUTE  ON [dbo].[HeadFile]  TO [Blueprint]
--GO

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

--GRANT  EXECUTE  ON [dbo].[PostFile]  TO [Blueprint]
--GO


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([dbo].[IsSchemaVersionLessOrEqual](N'6.5.0') <> 0)
	EXEC [dbo].[SetSchemaVersion] @value = N'6.5.0';
GO
-- --------------------------------------------------


