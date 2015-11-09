USE [master]
GO
IF EXISTS(select * from sys.databases where name='FileStorage') 
DROP DATABASE [FileStorage]
GO
/****** Object:  Database [FileStorage]    Script Date: 9/3/2015 4:19:58 PM ******/
CREATE DATABASE [FileStorage]
GO
ALTER DATABASE [FileStorage] MODIFY FILE
( NAME = N'FileStorage', SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
GO
ALTER DATABASE [FileStorage] MODIFY FILE
( NAME = N'FileStorage_log', SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [FileStorage] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [FileStorage].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [FileStorage] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [FileStorage] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [FileStorage] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [FileStorage] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [FileStorage] SET ARITHABORT OFF 
GO
ALTER DATABASE [FileStorage] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [FileStorage] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [FileStorage] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [FileStorage] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [FileStorage] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [FileStorage] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [FileStorage] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [FileStorage] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [FileStorage] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [FileStorage] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [FileStorage] SET  DISABLE_BROKER 
GO
ALTER DATABASE [FileStorage] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [FileStorage] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [FileStorage] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [FileStorage] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [FileStorage] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [FileStorage] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [FileStorage] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [FileStorage] SET RECOVERY FULL 
GO
ALTER DATABASE [FileStorage] SET  MULTI_USER 
GO
ALTER DATABASE [FileStorage] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [FileStorage] SET DB_CHAINING OFF 
GO
ALTER DATABASE [FileStorage] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [FileStorage] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'FileStorage', N'ON'
GO
USE [FileStorage]
GO
/****** Object:  StoredProcedure [dbo].[AddFile]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  StoredProcedure [dbo].[DeleteFile]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteFile]
	@FileId uniqueidentifier,
	@DeletedFileId AS uniqueidentifier OUTPUT
AS
BEGIN
	DECLARE @op TABLE (ColGuid uniqueidentifier)
    DELETE FROM [dbo].[Files]
	OUTPUT DELETED.FileId INTO @op
    WHERE [FileId] = @FileId
	SELECT  @DeletedFileId = t.ColGuid FROM @op t
END

GO
/****** Object:  StoredProcedure [dbo].[GetFile]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFile]
@FileId uniqueidentifier
AS
BEGIN
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
/****** Object:  StoredProcedure [dbo].[GetStatus]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetStatus]
AS
BEGIN
       SELECT TOP 1 COUNT(*) FROM [dbo].[Files];       
END
GO
/****** Object:  StoredProcedure [dbo].[GetFileInfo]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[HeadFile]
@FileId uniqueidentifier
AS
BEGIN
       SELECT [FileId]
      ,[StoredTime]
      ,[FileName]
      ,[FileType]
	  ,[FileSize]
       FROM [dbo].[Files]
       WHERE [FileId] = @FileId
END

GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
       [MigrationId] [nvarchar](150) NOT NULL,
       [ContextKey] [nvarchar](300) NOT NULL,
       [Model] [varbinary](max) NOT NULL,
       [ProductVersion] [nvarchar](32) NOT NULL,
CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
       [MigrationId] ASC,
       [ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Files]    Script Date: 9/3/2015 4:19:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Files](
       [FileId] [uniqueidentifier] NOT NULL,
       [StoredTime] [datetime] NOT NULL,
       [FileName] [nvarchar](256) NOT NULL,
       [FileType] [nvarchar](64) NOT NULL,
       [FileContent] [varbinary](max) NULL,
       [FileSize] [bigint] NOT NULL,
CONSTRAINT [PK_dbo.Files] PRIMARY KEY CLUSTERED 
(
       [FileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Files] ADD  CONSTRAINT [DF_Files_FileId]  DEFAULT (newsequentialid()) FOR [FileId]
GO
USE [master]
GO
ALTER DATABASE [FileStorage] SET  READ_WRITE 
GO

