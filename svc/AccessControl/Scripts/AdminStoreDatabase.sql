USE [master]
GO

/****** Object:  Database [AdminStore]    Script Date: 10/20/2015 4:35:17 PM ******/
DROP DATABASE [AdminStore]
GO

/****** Object:  Database [AdminStore]    Script Date: 10/20/2015 4:35:17 PM ******/
CREATE DATABASE [AdminStore]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'AdminStore', FILENAME = N'AdminStore.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'AdminStore_log', FILENAME = N'AdminStore_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [AdminStore] SET COMPATIBILITY_LEVEL = 110
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AdminStore].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [AdminStore] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [AdminStore] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [AdminStore] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [AdminStore] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [AdminStore] SET ARITHABORT OFF 
GO

ALTER DATABASE [AdminStore] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [AdminStore] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [AdminStore] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [AdminStore] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [AdminStore] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [AdminStore] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [AdminStore] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [AdminStore] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [AdminStore] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [AdminStore] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [AdminStore] SET  DISABLE_BROKER 
GO

ALTER DATABASE [AdminStore] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [AdminStore] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [AdminStore] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [AdminStore] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [AdminStore] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [AdminStore] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [AdminStore] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [AdminStore] SET RECOVERY FULL 
GO

ALTER DATABASE [AdminStore] SET  MULTI_USER 
GO

ALTER DATABASE [AdminStore] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [AdminStore] SET DB_CHAINING OFF 
GO

ALTER DATABASE [AdminStore] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [AdminStore] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO

ALTER DATABASE [AdminStore] SET  READ_WRITE 
GO

EXEC sys.sp_db_vardecimal_storage_format N'AdminStore', N'ON'
GO

USE [AdminStore]
GO

/****** Object:  StoredProcedure [dbo].[GetSession]    Script Date: 10/20/2015 4:45:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime from [dbo].[Sessions] where SessionId = @SessionId;
END

GO

/****** Object:  StoredProcedure [dbo].[BeginSession]    Script Date: 10/20/2015 4:46:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[BeginSession] 
(
	@UserId int,
	@BeginTime datetime,
	@NewSessionId uniqueidentifier OUTPUT,
	@OldSessionId uniqueidentifier OUTPUT
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
	SELECT @OldSessionId = SessionId from [dbo].[Sessions] where UserId = @UserId;
	SELECT @NewSessionId = NEWID();
	IF @OldSessionId IS NULL
	BEGIN
		INSERT [dbo].[Sessions](UserId, SessionId, BeginTime) VALUES(@UserId, @NewSessionId, @BeginTime);
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Sessions] SET SessionId = @NewSessionId, BeginTime = @BeginTime, EndTime = NULL where UserId = @UserId;
	END
	COMMIT TRANSACTION;
END

GO


/****** Object:  StoredProcedure [dbo].[EndSession]    Script Date: 10/20/2015 4:46:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

/****** Object:  StoredProcedure [dbo].[SelectSession]    Script Date: 10/20/2015 4:47:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SelectSession] 
(
	@ps int,
	@pn int
)
AS
BEGIN
	WITH SessionsRN AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY BeginTime DESC) AS RN, UserId, SessionId, BeginTime, EndTime FROM [dbo].[Sessions]
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END

GO
