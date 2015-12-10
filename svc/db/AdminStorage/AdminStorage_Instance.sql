
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_AdminStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
Print 'Creating AdminStorage Database...'
GO
-- --------------------------------------------------

-- Create Blueprint Roles
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_reader' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_reader]
	GRANT SELECT TO db_blueprint_reader
End
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_writer' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_writer]
	GRANT DELETE, INSERT, UPDATE TO db_blueprint_writer
End
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_blueprint_executor' AND type = 'R')
Begin
	CREATE ROLE [db_blueprint_executor]
	GRANT EXECUTE TO db_blueprint_executor
End
GO



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
)) ON [PRIMARY]
GO
/******************************************************************************************************************************
Name:			ApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [dbo].[ApplicationLabels]
GO

CREATE TABLE [dbo].[ApplicationLabels](
	[Key] [nvarchar](64) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_ApplicationLabels_Key_Locale] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Locale] ASC
)) ON [PRIMARY]
GO

/******************************************************************************************************************************
Name:			ConfigSettings

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigSettings]') AND type in (N'U'))
DROP TABLE [dbo].[ConfigSettings]
GO

CREATE TABLE [dbo].[ConfigSettings](
	[Key] [nvarchar](64) NOT NULL,
	[Value] [nvarchar](128) NOT NULL,
	[Group] [nvarchar](128) NOT NULL,
	[IsRestricted] [bit] NOT NULL,
 CONSTRAINT [PK_ConfigSettings] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)) ON [PRIMARY]
GO

/******************************************************************************************************************************
Name:			Sessions

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sessions]') AND type in (N'U'))
DROP TABLE [dbo].[Sessions]
GO

CREATE TABLE [dbo].[Sessions](
	[UserId] [int] NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[BeginTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[LicenseLevel] [int] NOT NULL,
	[IsSso] [bit] NOT NULL
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)) ON [PRIMARY]
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
Name:			GetStatus

Description: //TODO: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO

CREATE PROCEDURE [dbo].[GetStatus] 
AS
BEGIN
	SELECT COUNT(*) from [dbo].[Sessions];
END
GO 


/******************************************************************************************************************************
Name:			BeginSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BeginSession]
GO

CREATE PROCEDURE [dbo].[BeginSession] 
(
	@UserId int,
	@BeginTime datetime,
	@UserName nvarchar(max),
	@LicenseLevel int,
	@IsSso bit = 0,
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
		INSERT [dbo].[Sessions](UserId, SessionId, BeginTime, UserName, LicenseLevel, IsSso) 
		VALUES(@UserId, @NewSessionId, @BeginTime, @UserName, @LicenseLevel, @IsSso);
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Sessions] 
		SET SessionId = @NewSessionId, BeginTime = @BeginTime, EndTime = NULL, 
			UserName = @UserName, LicenseLevel = @LicenseLevel, IsSso = @IsSso 
		WHERE UserId = @UserId;
	END
	COMMIT TRANSACTION;
END
GO

/******************************************************************************************************************************
Name:			EndSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndSession]
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

/******************************************************************************************************************************
Name:			GetApplicationLables

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationLables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationLables]
GO

CREATE PROCEDURE [dbo].[GetApplicationLables] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [dbo].ApplicationLabels WHERE Locale = @Locale;
END
GO
/******************************************************************************************************************************
Name:			GetConfigSettings

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetConfigSettings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetConfigSettings]
GO

CREATE PROCEDURE [dbo].[GetConfigSettings] 
	@AllowRestricted bit
AS
BEGIN
	SELECT [Key], [Value], [Group], IsRestricted FROM [dbo].ConfigSettings WHERE IsRestricted = @AllowRestricted OR @AllowRestricted = 1;
END
GO
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSession]
GO

CREATE PROCEDURE [dbo].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [dbo].[Sessions] where SessionId = @SessionId;
END
GO 
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/17		Anton Trinkunas			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserSession]
GO

CREATE PROCEDURE [dbo].[GetUserSession] 
(
	@UserId int
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [dbo].[Sessions] where UserId = @UserId;
END
GO 
/******************************************************************************************************************************
Name:			SelectSessions

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SelectSessions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SelectSessions]
GO

CREATE PROCEDURE [dbo].[SelectSessions] 
(
	@ps int,
	@pn int
)
AS
BEGIN
	WITH SessionsRN AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY BeginTime DESC) AS RN, UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso 
		FROM [dbo].[Sessions] WHERE EndTime IS NULL
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicensesStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicensesStatus]
GO

CREATE PROCEDURE [dbo].[GetLicensesStatus] 
(
	@TimeUtc datetime,
	@TimeDiff int
)
AS
BEGIN
	SELECT LicenseLevel, COUNT(*) as [Count]
	FROM [dbo].[Sessions] 
	WHERE (EndTime IS NULL OR EndTime > DATEADD(MINUTE, @TimeDiff, @TimeUtc))
	GROUP BY LicenseLevel
END
GO 

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
EXEC [dbo].[SetSchemaVersion] @value = N'7.0.0';
GO
-- --------------------------------------------------


