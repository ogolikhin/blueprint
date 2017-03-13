
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


DECLARE @db_id AS int = DB_ID();
DECLARE @kills AS nvarchar(max) = N'';
SELECT @kills = @kills+N'KILL '+CAST([spid] AS nvarchar(16))+N'; ' FROM [sys].[sysprocesses] WHERE ([dbid] = @db_id) AND ([spid] <> @@SPID);
IF(LEN(@kills) > 0)
	BEGIN TRY
		EXEC(@kills);
	END TRY
	BEGIN CATCH
	END CATCH
GO

DECLARE @db_id AS int = DB_ID();
DECLARE @db_name AS nvarchar(128) = DB_NAME();
IF NOT EXISTS(SELECT * FROM [sys].[sysprocesses] WHERE ([dbid] = @db_id) AND ([spid] <> @@SPID))
	AND EXISTS(SELECT * FROM [sys].[databases] WHERE ([database_id] = @db_id) AND ([is_read_committed_snapshot_on] = 0))
	BEGIN TRY
		EXEC(N'ALTER DATABASE [' + @db_name + N'] SET ALLOW_SNAPSHOT_ISOLATION ON');
		EXEC(N'ALTER DATABASE [' + @db_name + N'] SET READ_COMMITTED_SNAPSHOT ON');
	END TRY
	BEGIN CATCH
	END CATCH
GO

DECLARE @db_name AS nvarchar(128) = DB_NAME();
DECLARE @sql AS nvarchar(max);

SET @sql = N'ALTER DATABASE [' + @db_name + N'] SET COMPATIBILITY_LEVEL = 110'; -- SQL Server 2012
EXEC(@sql);

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
2016/09/29		Areag Osman				Extends character limit for Key & Text columns, adds index for table
2016/10/06		Areag Osman				Creates a primary key on [Key], [Locale]
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [dbo].[ApplicationLabels]
GO

	CREATE TABLE [dbo].[ApplicationLabels](
		[ApplicationLabelId] [int] IDENTITY(1,1) NOT NULL,
		[Key] [nvarchar](128) NOT NULL,
		[Locale] [nvarchar](32) NOT NULL,
		[Text] [nvarchar](512) NOT NULL,

		CONSTRAINT [PK_ApplicationLabels] PRIMARY KEY NONCLUSTERED 
		(
			[Key], [Locale] ASC
		)
	) ON [PRIMARY]
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ApplicationLabels_Key_Locale')
	DROP INDEX IX_ApplicationLabels_Key_Locale on [dbo].[ApplicationLabels]
GO

CREATE NONCLUSTERED INDEX IX_ApplicationLabels_Key_Locale on [dbo].[ApplicationLabels] 
(
	[Key] ASC,
	[Locale] ASC
)
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
	[EndTime] [datetime] NOT NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[LicenseLevel] [int] NOT NULL,
	[IsSso] [bit] NOT NULL
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)) ON [PRIMARY]
GO

/******************************************************************************************************************************
Name:			LicenseActivityDetails

Description: 
			
Change History:
Date			Name					Change
2015/12/16		Glen Stone				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LicenseActivityDetails]') AND type in (N'U'))
DROP TABLE [dbo].[LicenseActivityDetails]
GO

CREATE TABLE [dbo].[LicenseActivityDetails](
	[LicenseActivityId] [int] NOT NULL,
	[LicenseType] [int] NOT NULL,
	[Count] [int] NOT NULL,
 CONSTRAINT [PK_LicenseActivityDetails] PRIMARY KEY CLUSTERED 
(
	[LicenseActivityId] ASC,
	[LicenseType] ASC
)) ON [PRIMARY]

/******************************************************************************************************************************
Name:			LicenseActivities

Description: 
			
Change History:
Date			Name					Change
2015/12/16		Glen Stone				Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LicenseActivities]') AND type in (N'U'))
DROP TABLE [dbo].[LicenseActivities]
GO

CREATE TABLE [dbo].[LicenseActivities](
	[LicenseActivityId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserLicenseType] [int] NOT NULL,
	[TransactionType] [int] NOT NULL,
	[ActionType] [int] NOT NULL,
	[ConsumerType] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_LicenseActivities] PRIMARY KEY CLUSTERED 
(
	[LicenseActivityId] ASC
)) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LicenseActivityDetails]  WITH CHECK ADD  CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities] FOREIGN KEY([LicenseActivityId])
REFERENCES [dbo].[LicenseActivities] ([LicenseActivityId])
GO

ALTER TABLE [dbo].[LicenseActivityDetails] CHECK CONSTRAINT [FK_LicenseActivityDetails_LicenseActivities]
GO

/******************************************************************************************************************************
Name:			Logs

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
DROP TABLE [dbo].[Logs]
GO

CREATE TABLE [dbo].[Logs](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[FormattedMessage] [nvarchar](4000) NULL,
	[OccurredAt] [datetimeoffset](7) NOT NULL,
	[UserName] [nvarchar](max),
	[SessionId] [nvarchar](40),
	[ActionName] [nvarchar](200),
	[CorrelationId] [uniqueidentifier],
	[Duration] [float],
	[InstanceName] [nvarchar](1000) NOT NULL,
	[ProviderId] [uniqueidentifier] NOT NULL,
	[ProviderName] [nvarchar](500) NOT NULL,
	[EventId] [int] NOT NULL,
	[EventKeywords] [bigint] NOT NULL,
	[Level] [int] NOT NULL,
	[Opcode] [int] NOT NULL,
	[Task] [int] NOT NULL,
	[Timestamp] [datetimeoffset](7) NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [xml] NULL,
	 CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)) ON [PRIMARY]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordRecoveryTokens]') AND type in (N'U'))
DROP TABLE [dbo].[PasswordRecoveryTokens]
GO

CREATE TABLE [dbo].[PasswordRecoveryTokens](
    [Login] [nvarchar](max),
    [CreationTime] [datetime] NOT NULL,
    [RecoveryToken] [uniqueidentifier] NOT NULL,

	 CONSTRAINT [PK_PasswordRecoveryTokens] PRIMARY KEY CLUSTERED 
(
	[RecoveryToken] ASC
)) ON [PRIMARY]
GO


/******************************************************************************************************************************
Name:			LogsType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteLogs]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogsType' AND ss.name = N'dbo')
DROP TYPE [dbo].[LogsType]
GO

CREATE TYPE LogsType AS TABLE
(
	[InstanceName] [nvarchar](1000),
	[ProviderId] [uniqueidentifier],
	[ProviderName] [nvarchar](500),
	[EventId] [int],
	[EventKeywords] [bigint],
	[Level] [int],
	[Opcode] [int],
	[Task] [int],
	[Timestamp] [datetimeoffset](7),
	[Version] [int],
	[FormattedMessage] [nvarchar](4000),
	[Payload] [xml],
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[UserName] [nvarchar](Max),
	[SessionId] [nvarchar](40),
	[OccurredAt] [datetimeoffset](7) NOT NULL,
	[ActionName] [nvarchar](200),
	[CorrelationId] [uniqueidentifier],
	[Duration] [float]
);
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

Description:	Returns the version of the database.

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


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BeginSession]
GO

CREATE PROCEDURE [dbo].[BeginSession] 
(
	@UserId int,
	@BeginTime datetime,
	@EndTime datetime,
	@UserName nvarchar(max),
	@LicenseLevel int,
	@IsSso bit = 0,
	@LicenseLockTimeMinutes int,
	@OldSessionId uniqueidentifier OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		BEGIN TRANSACTION;

		-- [Sessions]
		SELECT @OldSessionId = SessionId FROM [dbo].[Sessions] WHERE UserId = @UserId;
		DECLARE @NewSessionId uniqueidentifier = NEWID();
		IF @OldSessionId IS NULL
		BEGIN
			INSERT [dbo].[Sessions] (UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso)
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			VALUES (@UserId, @NewSessionId, @BeginTime, @EndTime, @UserName, @LicenseLevel, @IsSso);
		END
		ELSE
		BEGIN
			UPDATE [dbo].[Sessions]
			SET [SessionId] = @NewSessionId, [BeginTime] = @BeginTime, [EndTime] = @EndTime, [UserName] = @UserName, [LicenseLevel] = @LicenseLevel, [IsSso] = @IsSso 
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			WHERE [SessionId] = @OldSessionId;
		END

		-- [LicenseActivities]
		INSERT INTO [dbo].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
		VALUES
			(@UserId
			,@LicenseLevel
			,1 -- LicenseTransactionType.Acquire
			,1 -- LicenseActionType.Login
			,1 -- LicenseConsumerType.Client
			,@BeginTime)

		-- [LicenseActivityDetails]
		DECLARE @LicenseActivityId int = SCOPE_IDENTITY()
		DECLARE @ActiveLicenses table ( LicenseLevel int, Count int )
		INSERT INTO @ActiveLicenses EXEC [dbo].[GetActiveLicenses] @BeginTime, @LicenseLockTimeMinutes
		INSERT INTO [dbo].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
		SELECT @LicenseActivityId, [LicenseLevel], [Count]
		FROM @ActiveLicenses

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN

		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();

		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtendSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtendSession]
GO

CREATE PROCEDURE [dbo].[ExtendSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	UPDATE [dbo].[Sessions] SET EndTime = @EndTime
	OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
	WHERE SessionId = @SessionId AND BeginTime IS NOT NULL;
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndSession]
GO

CREATE PROCEDURE [dbo].[EndSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime,
	@TimeoutTime datetime,
	@LicenseLockTimeMinutes int
)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		BEGIN TRANSACTION;

		-- [Sessions]
		UPDATE [dbo].[Sessions] SET [BeginTime] = NULL, [EndTime] = @EndTime
		OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
		WHERE [SessionId] = @SessionId AND [BeginTime] IS NOT NULL
		AND (@TimeoutTime IS NULL OR @TimeoutTime = [EndTime]);

		IF @@ROWCOUNT > 0 BEGIN
			-- [LicenseActivities]
			DECLARE @UserId int
			DECLARE @LicenseLevel int
			SELECT @UserId = [UserId], @LicenseLevel = [LicenseLevel] FROM [dbo].[Sessions] WHERE [SessionId] = @SessionId;
			INSERT INTO [dbo].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
			VALUES
				(@UserId
				,@LicenseLevel
				,2 -- LicenseTransactionType.Release
				,IIF(@TimeoutTime IS NULL, 2, 3) -- LicenseActionType.LogOut or LicenseActionType.Timeout
				,1 -- LicenseConsumerType.Client
				,@EndTime)

			-- [LicenseActivityDetails]
			DECLARE @LicenseActivityId int = SCOPE_IDENTITY()
			DECLARE @ActiveLicenses table ( LicenseLevel int, Count int )
			INSERT INTO @ActiveLicenses EXEC [dbo].[GetActiveLicenses] @EndTime, @LicenseLockTimeMinutes
			INSERT INTO [dbo].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
			SELECT @LicenseActivityId, [LicenseLevel], [Count]
			FROM @ActiveLicenses
		END

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN

		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();

		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

/******************************************************************************************************************************
Name:			GetApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplicationLabels]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplicationLabels]
GO

CREATE PROCEDURE [dbo].[GetApplicationLabels] 
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
		FROM [dbo].[Sessions] WHERE BeginTime IS NOT NULL
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetActiveLicenses]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetActiveLicenses]
GO

CREATE PROCEDURE [dbo].[GetActiveLicenses] 
(
	@Now datetime,
	@LicenseLockTimeMinutes int
)
AS
BEGIN
	DECLARE @EndTime datetime = DATEADD(MINUTE, -@LicenseLockTimeMinutes, @Now)
	SELECT [LicenseLevel], COUNT(*) as [Count]
	FROM [dbo].[Sessions] 
	WHERE [EndTime] IS NULL OR [EndTime] > @EndTime
	GROUP BY [LicenseLevel]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicenseTransactions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicenseTransactions]
GO

CREATE PROCEDURE [dbo].[GetLicenseTransactions] 
(
	@StartTime datetime,
	@ConsumerType int
)
AS
BEGIN
	SELECT [UserId], [UserLicenseType] AS [LicenseType], [TransactionType], [ActionType], [TimeStamp] AS [Date],
		ISNULL(STUFF((SELECT ';' + CAST([LicenseType] AS VARCHAR(10)) + ':' + CAST([Count] AS VARCHAR(10))
		FROM [dbo].[LicenseActivityDetails] D
		WHERE D.[LicenseActivityId] = A.[LicenseActivityId]
		FOR XML PATH('')), 1, 1, ''), '') AS [Details]
	FROM [dbo].[LicenseActivities] A
	WHERE [TimeStamp] >= @StartTime
	AND [ConsumerType] = @ConsumerType
END
GO

/******************************************************************************************************************************
Name:			WriteLogs

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteLogs]
GO

CREATE PROCEDURE [dbo].[WriteLogs]  
(
  @InsertLogs LogsType READONLY
)
AS
BEGIN
  INSERT INTO [Logs] (
		[InstanceName],
		[ProviderId],
		[ProviderName],
		[EventId],
		[EventKeywords],
		[Level],
		[Opcode],
		[Task],
		[Timestamp],
		[Version],
		[FormattedMessage],
		[Payload],
		[IpAddress],
		[Source],
		[UserName],
		[SessionId],
		[OccurredAt],
		[ActionName],
		[CorrelationId],
		[Duration]
	)
  SELECT * FROM @InsertLogs;
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteLogs]
GO

CREATE PROCEDURE [dbo].[DeleteLogs] 
AS
BEGIN
  -- Get the number of days to keep from config settings - DEFAULT 7
  DECLARE @days int
  SELECT @days=c.[Value] FROM ConfigSettings c WHERE c.[Key] = N'DaysToKeepInLogs'
  SELECT @days=COALESCE(@days, 7) 

  -- Delete old log records
  DELETE FROM [Logs] WHERE [Timestamp] <= DATEADD(DAY, @days*-1, SYSDATETIMEOFFSET()) 
END

GO


/******************************************************************************************************************************
Name:			GetLogs

Description:	Returns last @limit records from Logs table
			
Change History:
Date			Name					Change
Feb 25 2016		Dmitry Lopyrev			Initial Version
Jun 7 2016		Dmitry Lopyrev			Updated
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLogs]
GO


CREATE PROCEDURE [dbo].[GetLogs]  
(
  @recordlimit int,
  @recordid int = null
)
AS
BEGIN
	DECLARE @total int
	DECLARE @fetch int
	DECLARE @id int

	SET @id = IsNULL(@recordid, 0);

	SELECT @total = COUNT(*) FROM [Logs] where @id = 0 OR ID <= @id 	

	SET @fetch = IIF(@recordlimit < 0, @total, @recordlimit)

	SELECT TOP (@fetch) * FROM [Logs] WHERE @id = 0 OR ID <= @id ORDER BY Id DESC

END



GO



/******************************************************************************************************************************
Name:			GetLicenseUsage

Description:	Returns license usage information 

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicenseUserActivity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicenseUserActivity]
GO

CREATE PROCEDURE [dbo].[GetLicenseUserActivity]
(
	@month int = null,
	@year int = null
)
AS
BEGIN
IF (NOT @month IS NULL AND (@month < 1 OR @month > 12))  
	SET @month = NULL
IF (NOT @year IS NULL AND (@year < 2000 OR @year > 2999))
	SET @year = NULL

DECLARE @startMonth date = ISNULL(DATEFROMPARTS(@year, @month, 1), '1900/1/1'); 
DECLARE @currentMonth date = DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1);

SELECT 
	la.UserId, 
	MAX(la.UserLicenseType) as LicenseType,  
	YEAR(la.[TimeStamp])* 100 + MONTH(la.[TimeStamp]) AS [YearMonth]
FROM 
	[dbo].[LicenseActivities] la  WITH (NOLOCK) 
WHERE 
	@startMonth < @currentMonth AND
	la.ConsumerType = 1 AND 
	la.ActionType = 1 
GROUP BY 
	la.UserId, YEAR(la.[TimeStamp])* 100 + MONTH(la.[TimeStamp])
ORDER BY 
	UserId, YearMonth


END
GO
--------------------------------------------------------------



/******************************************************************************************************************************
Name:			GetLicenseUsage

Description:	Returns license usage information 

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicenseUsage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicenseUsage]
GO

CREATE PROCEDURE [dbo].[GetLicenseUsage]
(
	@month int = null,
	@year int = null
)
AS
BEGIN

IF (NOT @month IS NULL AND (@month < 1 OR @month > 12))  
	SET @month = NULL
IF (NOT @year IS NULL AND (@year < 2000 OR @year > 2999))
	SET @year = NULL

DECLARE @currentDate datetime = GETUTCDATE();
DECLARE @startMonth date = CAST(DATEFROMPARTS(@year, @month, 1) as Datetime); 
DECLARE @currentMonth date = CAST(DATEFROMPARTS(YEAR(@currentDate), MONTH(@currentDate), 1) as Datetime);

WITH L
AS (
	-- Client: 1, Analytics: 2, REST: 3
	-- Viewer: 1, Collaborator: 2, Author: 3
	SELECT	 
		YEAR(la.[TimeStamp]) AS 'UsageYear',
		MONTH(la.[TimeStamp]) AS 'UsageMonth',
		la.UserId as 'UserId',
		la.ConsumerType AS 'Consumer',
		la.UserLicenseType AS 'License',
		ISNULL(da.LicenseType, 0) AS 'CountLicense',
		ISNULL(da.[Count], 0) AS 'Count'
	FROM 
		LicenseActivities AS la WITH (NOLOCK) LEFT JOIN LicenseActivityDetails AS da WITH (NOLOCK) 
			ON la.LicenseActivityId = da.LicenseActivityId
	WHERE 
		(@year IS NULL OR @month IS NULL OR la.[TimeStamp] > @startMonth) AND la.[TimeStamp] < @currentMonth
)

--Selects and returns the summary
SELECT 
	L.UsageYear * 100 + L.UsageMonth as 'YearMonth',
	COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 3 THEN L.UserId ELSE NULL END) AS 'UniqueAuthors',
	COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 2 THEN L.UserId ELSE NULL END) AS 'UniqueCollaborators',
	COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 1 THEN L.UserId ELSE NULL END) AS 'UniqueViewers',
	ISNULL(MAX(CASE WHEN L.CountLicense = 3 THEN L.[Count] ELSE 0 END), 0) AS 'MaxConcurrentAuthors',
	ISNULL(MAX(CASE WHEN L.CountLicense = 2 THEN L.[Count] ELSE 0 END), 0) AS 'MaxConcurrentCollaborators',
	ISNULL(MAX(CASE WHEN L.CountLicense = 1 THEN L.[Count] ELSE 0 END), 0) AS 'MaxConcurrentViewers',
	-- following two fields need to be set to 0 because actual data is stored in maid DB
	0 AS 'UsersFromAnalytics',
	0 AS 'UsersFromRestApi'
FROM 
	L
GROUP BY 
	L.UsageYear, L.UsageMonth;
END
GO 



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserPasswordRecoveryRequestCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].GetUserPasswordRecoveryRequestCount
GO

CREATE PROCEDURE [dbo].GetUserPasswordRecoveryRequestCount
(
    @login as nvarchar(max)
)
AS
BEGIN
    SELECT COUNT([Login])
    FROM [Blueprint_AdminStorage].[dbo].[PasswordRecoveryTokens]
    WHERE [Login] = @login
    AND [CreationTime] > DATEADD(d,-1,CURRENT_TIMESTAMP)
END
GO 



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetUserPasswordRecoveryToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].SetUserPasswordRecoveryToken
GO

CREATE PROCEDURE [dbo].SetUserPasswordRecoveryToken 
(
    @login as nvarchar(max),
    @recoverytoken as uniqueidentifier
)
AS
BEGIN
    INSERT INTO [dbo].[PasswordRecoveryTokens]
    ([Login],[CreationTime],[RecoveryToken])
    VALUES (@login, CURRENT_TIMESTAMP, @recoverytoken)
END
GO 




DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END

BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0

-- Add Job
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=@jobname, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Blueprint admin storage maintenance', 
		@job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Step 1 - Delete old logs from AdminStorage
SET @cmd = N'
-- Delete log entries
EXECUTE [dbo].[DeleteLogs] '
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete old logs from AdminStorage', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=@cmd, 
		@database_name=@blueprintDB, 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

-- Add Schedule
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=@schedulename, 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20160101, 
		@active_end_date=99991231, 
		@active_start_time=10000, 
		@active_end_time=235959
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO

IF NOT EXISTS(SELECT [KEY] FROM [dbo].[ConfigSettings] WHERE [Key]=N'DaysToKeepInLogs')
BEGIN
	INSERT INTO [dbo].[ConfigSettings] ([Key], [Value], [Group], [IsRestricted])
		 VALUES (N'DaysToKeepInLogs', N'7', N'Maintenance', 0)
END

-- -----------------------------------------------------------------------------------------------
-- Insert statements for Application Labels are Auto-Generated by T4 template file.
--
-- To add/edit/remove application labels from the [dbo].[ApplicationLabels] table, please update the
-- CSV file located at: '~\blueprint\svc\db\AdminStorage\Data\ApplicationLabels_en-US.csv'
-- 
-- DO NOT EDIT THESE INSERT STATEMENTS DIRECTLY AS YOUR CHANGES WILL BE OVERWRITTEN

CREATE TABLE #tempAppLabels (
	[Key] [nvarchar](128) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](512) NOT NULL

	CONSTRAINT [PK_ApplicationLabels] PRIMARY KEY NONCLUSTERED 
	(
		[Key], [Locale] ASC
	)
)

INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'en-US', N'Yes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'en-US', N'No')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'en-US', N'OK')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Close', 'en-US', N'Close')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Open', 'en-US', N'Open')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Publish', 'en-US', N'Publish Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Discard', 'en-US', N'Discard')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Publish_All', 'en-US', N'Publish All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Discard_All', 'en-US', N'Discard All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Create', 'en-US', N'Create')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_PublishAndContinue', 'en-US', N'Publish and Continue')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Add', 'en-US', N'Add')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Move', 'en-US', N'Move')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Copy', 'en-US', N'Copy')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_ConfirmCloseProject', 'en-US', N'Close Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Proceed', 'en-US', N'Proceed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Download', 'en-US', N'Download')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Refresh', 'en-US', N'Refresh')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Save', 'en-US', N'Save')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Alert', 'en-US', N'Warning')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'en-US', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'en-US', N'Loading ...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'en-US', N'You must enable JavaScript to use this application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'en-US', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'en-US', N'Signed in as')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Welcome', 'en-US', N'Welcome')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_ChangePassword', 'en-US', N'Change password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Logout', 'en-US', N'Log out')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Help', 'en-US', N'Help')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Tour', 'en-US', N'Tour')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_Artifact_Header_LegacyArtifact', 'en-US', N'Blueprint artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_Browse_Actor', 'en-US', N'Browse Actor')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Sidebar_Left', 'en-US', N'Explorer')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Sidebar_Right', 'en-US', N'Utility Panel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Explorer_EmptyMessage', 'en-US', N'You have no projects loaded in the Explorer')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Explorer_LoadProject_ButtonLabel', 'en-US', N'Load project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Explorer_LoadProject_ButtonTooltip', 'en-US', N'Load project in Explorer')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Explorer_BottomBar_UnpublishedChanges', 'en-US', N'Unpublished Changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Open', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_CloseTabWithUnsavedChanges', 'en-US', N'You have unsaved changes that will be lost if you leave.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_ConfirmLogout_WithUnpublishedArtifacts', 'en-US', N'You have {0} saved but unpublished change(s). Artifacts will continue to be locked to other users if you do not discard or publish them before continuing.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_ConfirmLogout_Logout', 'en-US', N'Log Out')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_ConfirmLogout_Cancel', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Artifact_Info_OpenLatestVersion', 'en-US', N'Open latest version')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Project', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Recent_Projects', 'en-US', N'Open Recent Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_Project', 'en-US', N'Close Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_All_Projects', 'en-US', N'Close All Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Save', 'en-US', N'Save Changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Save_All', 'en-US', N'Save All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Discard', 'en-US', N'Discard Changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Discard_All', 'en-US', N'Discard All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Publish', 'en-US', N'Publish Changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Publish_All', 'en-US', N'Publish All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Refresh', 'en-US', N'Refresh Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Refresh_All', 'en-US', N'Refresh All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Delete', 'en-US', N'Delete Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_New', 'en-US', N'New')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Impact_Analysis', 'en-US', N'Analyze Impact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Tour', 'en-US', N'Tour')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Move_Copy', 'en-US', N'Move/Copy Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Move', 'en-US', N'Move Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Copy', 'en-US', N'Copy Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Copy_Shapes', 'en-US', N'Copy Shapes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Menu', 'en-US', N'Additional actions')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Generate', 'en-US', N'Generate')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Generate_Test_Cases', 'en-US', N'Generate Test Cases')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Generate_Test_Cases_Title', 'en-US', N'Add Process to Generate Tests')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Generate_Test_Cases_Success_Message', 'en-US', N'The operation has been added to the {0} list as Job {1}')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Generate_Test_Cases_Failure_Message', 'en-US', N'Failed to generate test cases.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Tooltip_Dirty', 'en-US', N'Unsaved')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Collapsible_ShowMore', 'en-US', N'Show more')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Collapsible_ShowLess', 'en-US', N'Show less')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Discussions', 'en-US', N'Discussions')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Properties', 'en-US', N'Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Attachments', 'en-US', N'Files')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Relationships', 'en-US', N'Relationships')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_History', 'en-US', N'History')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Reviews', 'en-US', N'Reviews')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_NoPanel', 'en-US', N'No additional details to display')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_BetterTop_Label', 'en-US', N'Better')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_Stories_Label', 'en-US', N'Stories')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_Drive_Label', 'en-US', N'drive')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_BetterBottom_Label', 'en-US', N'Better')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_Products_Label', 'en-US', N'Products.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_OpenProject_ButtonLabel', 'en-US', N'Open a project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_LeftCircle_Label', 'en-US', N'Speeds up projects by auto-generating a complete set of highquality, well formatted and consistent User Stories.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_CenterCircle_Label', 'en-US', N'Improves Business and IT Alignment through visualization and traceability.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_RightCircle_Label', 'en-US', N'Enhances enterprise collaboration and communication from a broad range of users, of any skills.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_UsefulResources_Header', 'en-US', N'Useful resources to get started')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_GetStartedWithStoryteller_Hypelink', 'en-US', N'Get started with Storyteller')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_GetStartedWithProcessEditor_Hypelink', 'en-US', N'Get started with the Process Editor')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_BlueprintCommunityForum_Hypelink', 'en-US', N'Blueprint Community Forum')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_MainArea_DefaultScreen_StorytellerTour_Hypelink', 'en-US', N'Storyteller Tour')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Filter_SortByLatest', 'en-US', N'Sort by latest')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Filter_SortByEarliest', 'en-US', N'Sort by earliest')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Version', 'en-US', N'Version')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Draft', 'en-US', N'Draft')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_Deleted', 'en-US', N'Deleted')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_History_No_Artifact', 'en-US', N'No history available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_New', 'en-US', N'Add New')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Attachment', 'en-US', N'Attachment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Attachment', 'en-US', N'Add Attachment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Ref_Doc', 'en-US', N'Document Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Ref_Doc', 'en-US', N'Add Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_All', 'en-US', N'All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Attachments', 'en-US', N'Attachments')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Ref_Docs', 'en-US', N'Document References')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Uploaded_By', 'en-US', N'Uploaded by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Attachments', 'en-US', N'There are no associated files')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Attachment_Desc', 'en-US', N'A file attachment is supporting content that is unique to the artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Reference_Desc', 'en-US', N'A document reference is a document-type artifact that is linked to multiple artifacts.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Attachments_Only', 'en-US', N'No attachments available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_No_Documents_Only', 'en-US', N'No document references available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Download', 'en-US', N'Download')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Download_No_Attachment', 'en-US', N'There''s no attachment available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Document_Picker_Title', 'en-US', N'Add Document Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Max_Filesize_Error', 'en-US', N'The file exceeds')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Max_Attachments_Error', 'en-US', N'The artifact has the maximum number of attachments.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Have_Wrong_Type', 'en-US', N'The attachment has wrong file type.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Image_Wrong_Type', 'en-US', N'Specified image type isn''t supported.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Error', 'en-US', N'Upload error.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Empty', 'en-US', N'There are no files to upload.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Total_Failed', 'en-US', N'Total items failed:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Dialog_Header', 'en-US', N'File Upload')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Same_DocRef_Error', 'en-US', N'Artifact already contains this document reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Delete_Attachment_Header', 'en-US', N'Delete Attachment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Delete_Attachment', 'en-US', N'Please confirm the deletion of this attachment.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Filesize_Zero_Error', 'en-US', N'The file is invalid (its size is 0 bytes).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Change', 'en-US', N'Change')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Upload', 'en-US', N'Upload')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Download', 'en-US', N'Download')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Introduction_Message', 'en-US', N'You may collaborate with others using this discussion panel to add comments to any selected published artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Start_Discussion', 'en-US', N'+ Start a new discussion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Discussion', 'en-US', N'+ New comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_No_Artifact', 'en-US', N'The item you selected has no information to display or you do not have access to view it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_No_Artifact_Id', 'en-US', N'You may collaborate with others using this discussion panel to add comments to any selected published sub-artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Post_Button_Text', 'en-US', N'Post comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Cancel_Button_Text', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Comment_Place_Holder', 'en-US', N'Add a new comment...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Post_Button_Text', 'en-US', N'Post reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Cancel_Button_Text', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Reply_Comment_Place_Holder', 'en-US', N'Add a reply...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Show_Replies', 'en-US', N'Show replies')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Hide_Replis', 'en-US', N'Hide replies')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Delete_Comment_Tooltip', 'en-US', N'Delete comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Edit_Comment_Tooltip', 'en-US', N'Edit comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Reply_Tooltip', 'en-US', N'Reply to the comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Delete_Reply_Tooltip', 'en-US', N'Delete reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Edit_Reply_Tooltip', 'en-US', N'Edit reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Reply_Link', 'en-US', N'Reply')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_StateTooltip', 'en-US', N'Discussion State')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_All', 'en-US', N'All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Traces', 'en-US', N'Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Association', 'en-US', N'Other')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Manage_Traces', 'en-US', N'Manage Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Current_Traces', 'en-US', N'Current Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Collapse', 'en-US', N'Hide details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Expand', 'en-US', N'Show details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Relationships', 'en-US', N'No relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Traces', 'en-US', N'No traces available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Association', 'en-US', N'No other relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Association', 'en-US', N'Association')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Actor_Inherits', 'en-US', N'Actor Inherits')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Document_Reference', 'en-US', N'Document Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Unauthorized', 'en-US', N'Unauthorized')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Selected_Singular', 'en-US', N'item selected')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Selected_Plural', 'en-US', N'items selected')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Change_Traces', 'en-US', N'Change Direction')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_To', 'en-US', N'To')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_From', 'en-US', N'From')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Bidirectional', 'en-US', N'Bidirectional')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Flag_Trace', 'en-US', N'Flag Trace')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Flag_Traces', 'en-US', N'Flag Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Un_Flag_Trace', 'en-US', N'Unflag Trace')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Un_Flag_Traces', 'en-US', N'Unflag Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Select', 'en-US', N'Select')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Delete_Traces', 'en-US', N'Delete Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Change_Trace', 'en-US', N'Change Direction')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Add_Trace', 'en-US', N'Add Trace')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Selected_Artifact', 'en-US', N'Selected Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Reviews_No_Relationships', 'en-US', N'This Baseline has not been used for any Reviews')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Confirmation_Delete_Trace', 'en-US', N'Please confirm the deletion of the trace.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Confirmation_Delete_Traces', 'en-US', N'Please confirm the deletion of the selected traces ({0}).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Introduction_Message', 'en-US', N'A trace is one type of relationship that can exist between artifacts. Traces define the direction of the relationship.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Add_Relationship', 'en-US', N'Add a trace')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Loading_Indicator_Label', 'en-US', N'Loading...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_System_Properties_Label', 'en-US', N'System Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Additional_Properties_Label', 'en-US', N'Additional Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_StepOf_Actor', 'en-US', N'Actor (Input)')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_StepOf_System', 'en-US', N'System (Expected Outcome)')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_InheritancePicker_Title', 'en-US', N'Select Actor')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_SameBaseActor_ErrorMessage', 'en-US', N'Actor cannot be set as its own parent')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Properties_No_Properties', 'en-US', N'No properties available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Properties_Unsaved_No_Properties', 'en-US', N'The item you selected has not been saved or published. No properties available.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_200', 'en-US', N'The artifact has been saved.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_400', 'en-US', N'An error has occurred and the artifact {0} is no longer valid. Please contact an administrator.<br><br>')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_404', 'en-US', N'Sorry, but the artifact {0} cannot be saved because it has been deleted or moved. Please Refresh All.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409', 'en-US', N'There was a conflict while saving the artifact {0}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_400_114', 'en-US', N'The artifact {0} cannot be saved. Please ensure all values are correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_115', 'en-US', N'The artifact {0} cannot be saved because your lock on it was stolen by another user. Please Refresh, any changes you have made will be lost.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_116', 'en-US', N'The artifact {0} cannot be saved. Changes were made to an artifact you do not have permission to access, or has read-only properties.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_117', 'en-US', N'A property type is out of date.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_123', 'en-US', N'The artifact could not be saved. You have changed relationship details with an artifact that no longer exists. Please Refresh All.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_124', 'en-US', N'The artifact {0} cannot be saved. It inherits from an actor that already inherits from this artifact. Actor artifacts cannot inherit from each other.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_130', 'en-US', N'The Item name cannot be empty.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_Other', 'en-US', N'An error has occurred and the artifact cannot be saved. Please contact an administrator.<br><br>')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Auto_Confirm', 'en-US', N'Your changes could not be autosaved.<br/>Try saving manually for more information. If you proceed, your changes will be lost.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Possible_SubArtifact_Validation_Error', 'en-US', N'There may be issues with one or more sub-artifact property values. Please validate the artifact to confirm.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Refresh_Project_NotFound', 'en-US', N'You have attempted to access a project that has been deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Refresh_Artifact_Deleted', 'en-US', N'The artifact you were viewing has been deleted. The artifact''s parent or project is now being displayed.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('SubArtifact_Has_Been_Deleted', 'en-US', N'The subartifact has been deleted or moved. Please refresh your display.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_All_Dialog_Header', 'en-US', N'Publish All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_All_Dialog_Message', 'en-US', N'Artifacts with unpublished changes: {0}<br/>After publishing, all changes made to each artifact will be available to other users. Please review and confirm:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_All_Success_Message', 'en-US', N'Changes have been published for all artifacts ({0}).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_All_No_Unpublished_Changes', 'en-US', N'There are no artifacts that can be published.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Close_Project_UnpublishedArtifacts', 'en-US', N'You have {0} saved but unpublished change(s).<br/><br/>Considering publishing or discarding before continuing.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_List_Show_More', 'en-US', N'Show more')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_List_Show_First_N_Results', 'en-US', N'Displaying first {0} results')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_Dependents_Dialog_Message', 'en-US', N'If you publish the artifact, other related artifacts will also need to be published. Please review and confirm:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_Success_Message', 'en-US', N'The artifact has been published.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_Failure_Message', 'en-US', N'The artifact cannot be published.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_Artifact_Failure_Message', 'en-US', N'An error has occurred and the artifact cannot be published. Please contact an administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Publish_Failure_LockedByOtherUser_Message', 'en-US', N'The artifact cannot be published because it is locked by another user.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_All_No_Unpublished_Changes', 'en-US', N'There are no artifacts that can be discarded.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_All_Dialog_Message', 'en-US', N'Artifacts with unpublished changes: {0}<br/>Artifacts that have never been published will be deleted, and published artifacts will be restored to their previous versions:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_All_Many_Dialog_Message', 'en-US', N'Artifacts with unpublished changes: {0}<br/>Artifacts that have never been published will be deleted, and published artifacts will be restored to their previous versions.<br/><br/>Because you are discarding a large number of changes, this operation may take longer than usual.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_All_Success_Message', 'en-US', N'Changes have been discarded for all artifacts ({0}).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_Single_Dialog_Message', 'en-US', N'This artifact will be restored to its last published version. If it has never been published, it will be deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_Dependents_Dialog_Message', 'en-US', N'If you discard changes to the artifact, other related artifacts will also need to be restored to previously published versions. Please review and confirm:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_Success_Message', 'en-US', N'Changes have been discarded.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_No_Changes', 'en-US', N'This artifact has no changes to discard and will now be refreshed to the most recent version.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_Single_Artifact_Confirm', 'en-US', N'This artifact will be restored to its last published version. If it has never been published, it will be deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Discard_Multiple_Artifacts_Confirm', 'en-US', N'Artifacts with unpublished changes: {0}<br/> Artifacts that have never been published will be deleted, and published artifacts will be restored to their previous versions.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Move_Artifacts_Picker_Header', 'en-US', N'Move to...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Copy_Artifacts_Picker_Header', 'en-US', N'Copy to...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Move_Artifacts_Picker_Insert_Label', 'en-US', N'Insert:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Move_Artifacts_Picker_Insert_Choice_Inside', 'en-US', N'Inside')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Move_Artifacts_Picker_Insert_Choice_Above', 'en-US', N'Above')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Move_Artifacts_Picker_Insert_Choice_Below', 'en-US', N'Below')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Dialog_Header', 'en-US', N'New Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Dialog_Artifact_Type', 'en-US', N'Artifact Type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Dialog_Artifact_Name', 'en-US', N'Name of Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Error_404_101', 'en-US', N'The artifact could not be created. Its parent artifact has likely been deleted or moved. The parent artifact''s parent or project is now being displayed.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Error_404_102', 'en-US', N'The artifact could not be created. Its project has likely been deleted or moved. The Explorer has been refreshed.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Error_404_109', 'en-US', N'The artifact could not be created. Its type has likely been removed by an administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_New_Artifact_Error_Generic', 'en-US', N'Sorry, but an error has occurred and the artifact cannot be created. Please contact an administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Confirm_Publish_Collection', 'en-US', N'Please publish your changes before entering the review. Would you like to proceed?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Create_Rapid_Review', 'en-US', N'Create Rapid Review')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Term', 'en-US', N'Term')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Definition', 'en-US', N'Definition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Empty', 'en-US', N'No terms have been defined.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Content_Header', 'en-US', N'Baseline Contents')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Items', 'en-US', N'Items')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Permissions_Warning', 'en-US', N'Some items are hidden due to permission level')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_No_Artifacts_In_Baseline', 'en-US', N'No artifacts available in this baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Add_Artifacts_Picker_Header', 'en-US', N'Add Artifacts to Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Add_Artifacts_To_Baseline', 'en-US', N'Click Here to Add Artifacts to This Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Add_Artifacts_Success', 'en-US', N'{0} artifacts have been added to this Baseline.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Add_Artifacts_Already_Included', 'en-US', N'{0} artifacts were already included in this Baseline.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Seal_Baseline', 'en-US', N'Seal this Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Available_In_Analytics', 'en-US', N'Baseline Data Available In Analytics')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Save_Warning', 'en-US', N'Baseline is going to be saved. Do you want to continue?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Seal_Warning', 'en-US', N'Are You Sure? If you Seal this Baseline, you can never again make updates to the artifacts you have added to it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_Seal_Confirm', 'en-US', N'Seal It Permanently')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Baseline_TimestampChangeOnSave_WarningMessage', 'en-US', N'Are you sure you want to apply this Timestamp to your Baseline? If you do, you will permanently remove the following artifacts, since they did not exist on the date you have selected:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Content_Header', 'en-US', N'Collection Contents')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Edit_Rapid_Review', 'en-US', N'Edit Rapid Review')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_No_Artifacts_In_Collection', 'en-US', N'No artifacts available in this collection')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Add_Artifacts_Picker_Header', 'en-US', N'Add Artifacts to Collection')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Add_Artifacts_To_Collection', 'en-US', N'Click Here to Add Artifacts to This Collection')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Add_Artifacts_Success', 'en-US', N'{0} artifacts have been added to this Collection.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Collection_Add_Artifacts_Already_Included', 'en-US', N'{0} artifacts were already included in this Collection.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Container_Items_Selected', 'en-US', N'Items selected: {0}')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Container_Delete_Selected', 'en-US', N'Delete selected')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Container_Confirmation_Delete_Item', 'en-US', N'Please confirm the deletion of the item.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Container_Confirmation_Delete_Items', 'en-US', N'Please confirm the deletion of the selected items ({0}).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_Contents_To_Baseline', 'en-US', N'Add Contents to Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Baseline', 'en-US', N'Add to Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Baseline_Picker_Header', 'en-US', N'Add Artifact to Baseline')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Baseline_Success', 'en-US', N'The artifact has been added to the baseline.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Baseline_Failed_Because_Lock', 'en-US', N'The artifact could not be added to the baseline. The baseline is locked by {userName}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Collection_Picker_Header', 'en-US', N'Add Artifact to Collection')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Collection', 'en-US', N'Add to Collection')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Collection_Success', 'en-US', N'The artifact has been added to the collection.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Collection_Failed_Because_Lock', 'en-US', N'The artifact could not be added to the collection. The collection is locked by {userName}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Add_To_Container_Include_Descendants', 'en-US', N'Include artifact descendants')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Change_Password_Dialog_Header', 'en-US', N'Change Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Change_Password_Dialog_Message', 'en-US', N'Use the form below to change your password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Confirmation_Delete_Comment', 'en-US', N'Are you sure you want to permanently delete the selected comment?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Confirmation_Delete_Comment_Thread', 'en-US', N'Are you sure you want to permanently delete all comments in this discussion?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Comment_Deleted', 'en-US', N'This comment has been deleted. Please refresh.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'en-US', N'Cannot get current user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'en-US', N'Wrong request id. Please click ''Retry'' in Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'en-US', N'Log in Failed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'en-US', N'To continue your session, please log in with the same user that the session was started with.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'en-US', N'Cannot verify license')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'en-US', N'Cannot get Session Token')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'en-US', N'No licenses found or Blueprint is using an invalid server license. Please contact your Blueprint administrator')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'en-US', N'The maximum concurrent license limit has been reached. Please contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedAuthFailed', 'en-US', N'There is a problem with federated authentication. Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedFallbackDisabled', 'en-US', N'Please log in with your corporate credentials.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'en-US', N'Username')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'en-US', N'Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'en-US', N'Forgot Password?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'en-US', N'Change Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_UpdatePasswordButton', 'en-US', N'Update Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'en-US', N'Log In')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'en-US', N'Log in with corporate credentials')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'en-US', N'Go back to log in')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'en-US', N'Reset Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Retry', 'en-US', N'Retry')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginPrompt', 'en-US', N'Log in with username and password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_CurrentPassword', 'en-US', N'Current Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_NewPassword', 'en-US', N'New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_ConfirmPassword', 'en-US', N'Confirm New Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'en-US', N'Blueprint Software Systems Inc. All rights reserved.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'en-US', N'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsCannotBeEmpty', 'en-US', N'Username and password cannot be empty.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'en-US', N'You are already logged in to Blueprint in another browser.<br><br>Do you want to override your other session and work here?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'en-US', N'Please enter your username and password.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'en-US', N'Please enter your username.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired_ChangePasswordPrompt', 'en-US', N'Your password has expired. Please update it below.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterSamlCredentials_Verbose', 'en-US', N'Please authenticate using your corporate credentials in the popup window that has opened. If you do not see the window, please ensure your popup blocker is disabled and then click the Retry button.<br><br>You will be automatically logged in after you are authenticated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsInvalid', 'en-US', N'Ensure your username and password are correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_ADUserNotInDB', 'en-US', N'You do not have a Blueprint account. <br>Please contact your administrator for access.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'en-US', N'Your account is disabled. <br>Please contact your administrator for assistance.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired', 'en-US', N'Your password has expired.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCurrentPassword', 'en-US', N'Ensure your current password is correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CurrentPasswordCannotBeEmpty', 'en-US', N'Please enter your current password.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordChangedSuccessfully', 'en-US', N'Your password has been updated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordConfirmMismatch', 'en-US', N'Ensure your new password and confirmation match.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMaxLength', 'en-US', N'Your new password cannot be longer than 128 characters.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMinLength', 'en-US', N'Your new password must be at least 8 characters long.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeEmpty', 'en-US', N'New password cannot be empty.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordSameAsOld', 'en-US', N'Ensure your new password is different <br>from the current one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeUsername', 'en-US', N'Ensure your new password is different from your username.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeDisplayname', 'en-US', N'Ensure your new password is different from your display name.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordChangeCooldown', 'en-US', N'Your password has not been updated. It can be changed <br> once every 24 hours. Please try again later.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCriteria', 'en-US', N'Your new password must contain at least one number, <br>uppercase letter, and non-alphanumeric character.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordAlreadyUsedPreviously', 'en-US', N'Ensure your new password is different from previously used ones.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_Timeout', 'en-US', N'​Your session has expired or was overridden.<br>Please log in to continue.​')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_InvalidLicense', 'en-US', N'Your license does not include access to this application.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'en-US', N'Required')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Folder_NotFound', 'en-US', N'Couldn''t find specified project or folder')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Filter_Artifact_All_Types', 'en-US', N'All types')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_NotFound', 'en-US', N'Couldn''t find the project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_MetaDataNotFound', 'en-US', N'Couldn''t find the project meta data')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_NotFound', 'en-US', N'Couldn''t find the artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_DoesNotExistOrMoved', 'en-US', N'The artifact doesn''t exists or moved to different location')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ArtifactType_NotFound', 'en-US', N'Couldn''t find the artifact type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_AlreadyLocked', 'en-US', N'Artifact locked by another user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_DoesNotExist', 'en-US', N'The artifact has been deleted or moved.<br />The Explorer will now be refreshed.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_AccessDenied', 'en-US', N'Cannot establish the artifact lock. Access denied.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_Failure', 'en-US', N'Cannot establish the artifact lock due to general failure')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_Refresh', 'en-US', N'This artifact has been changed on the server and will now be refreshed to the most recent version.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Version_NotFound', 'en-US', N'The specified artifact version does not exist.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_GoTo_NotAvailable', 'en-US', N'This artifact type cannot be opened directly using the Go To feature.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_NoPermissions', 'en-US', N'You do not have permission to edit this artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_Locked', 'en-US', N'Locked.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_LockedBy', 'en-US', N'Locked by {0}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_LockedOn', 'en-US', N'Locked on {0}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_LockedByOn', 'en-US', N'Locked by {0} on {1}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_DeletedByOn', 'en-US', N'Deleted by {0} on {1}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_Deleted', 'en-US', N'This artifact has been deleted or moved.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_InfoBanner_Historical', 'en-US', N'Version {0}, published by {1} on {2}.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Type', 'en-US', N'Type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedBy', 'en-US', N'Created by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedOn', 'en-US', N'Created on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditBy', 'en-US', N'Last edited by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditOn', 'en-US', N'Last edited on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Collections', 'en-US', N'Collections')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_BaselinesAndReviews', 'en-US', N'Baselines and Reviews')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_ActorInheritFrom', 'en-US', N'Inherits from')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Group_Identifier', 'en-US', N'(g)')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Search_Projects', 'en-US', N'Search for projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Search_Artifacts', 'en-US', N'Search for artifacts')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Search_Results', 'en-US', N'Displaying top {0} results.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Search_Refine', 'en-US', N'Try a more specific search term to refine your search.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Searching', 'en-US', N'Searching...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Filter_By_Type', 'en-US', N'Filter by type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_No_Results_Found', 'en-US', N'No results found.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_X', 'en-US', N'X')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Y', 'en-US', N'Y')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Width', 'en-US', N'Width')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Height', 'en-US', N'Height')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Label', 'en-US', N'Label')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Options', 'en-US', N'Options')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_ID', 'en-US', N'ID')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Diagram_OldFormat_Message', 'en-US', N'This diagram is stored in an old format that is incompatible with this version. To display the diagram, please open it in Silverlight Main Experience, make a modification, and publish it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Details_FieldNameError', 'en-US', N'The field name isn''t specified')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Today', 'en-US', N'Today')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Clear', 'en-US', N'Clear')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Datepicker_Done', 'en-US', N'Close')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Placeholder_Select_Option', 'en-US', N'Select an option')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_No_Matching_Options', 'en-US', N'No matching options')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Cannot_Be_Empty', 'en-US', N'This value is required.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Wrong_Format', 'en-US', N'Please check the format of the value.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Decimal_Places', 'en-US', N'There is a maximum number of decimal places:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Value_Must_Be', 'en-US', N'The value must be')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Date_Must_Be', 'en-US', N'The date must be')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Later', 'en-US', N'or later.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Earlier', 'en-US', N'or earlier.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Greater', 'en-US', N'or greater.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Suffix_Or_Less', 'en-US', N'or less.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Actor_Section_Name', 'en-US', N'Actor Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Artifact_Section_Name', 'en-US', N'Artifact Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_SubArtifact_Section_Name', 'en-US', N'Sub-Artifact Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_UserPicker_Placeholder', 'en-US', N'Type the user or group''s name or email')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_UserPicker_Searching', 'en-US', N'Searching...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_UserPicker_ShowMore', 'en-US', N'Show more')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_UserPicker_Display_Top_N_Results', 'en-US', N'Displaying first {0} results')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_RTF_Add_InlineTrace', 'en-US', N'Add Inline Trace')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_RTF_InlineTrace_Error_Itself', 'en-US', N'An artifact cannot have an inline trace to itself.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_RTF_InlineTrace_Error_Invalid_Selection', 'en-US', N'The traced artifact could not be added. The selection is invalid.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_RTF_InlineTrace_Error_Permissions', 'en-US', N'You do not have permission to add inline traces. Please contact an administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Not_Available', 'en-US', N'n/a')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_Max_Images_Error', 'en-US', N'This property exceeds the maximum number of images ({0}).')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_ServiceUnavailable', 'en-US', N'A service on the Blueprint web server is unavailable. Please try again later. If the problem continues, contact your administrator for assistance.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_Forbidden', 'en-US', N'The operation could not be completed because of privilege-related issues.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_NotFound', 'en-US', N'The artifact could not be found.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_Collection_NotFound', 'en-US', N'The Collection has been deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_InternalServer', 'en-US', N'The operation could not be completed. Please try again later. If the problem continues, contact your administrator for assistance.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message1', 'en-US', N'An error ocurred while loading the page.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message2', 'en-US', N'Please try again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message3', 'en-US', N'If the problem persists, contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Label', 'en-US', N'Error')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Email_Discussions_Disabled_Message', 'en-US', N'Note: Email Discussions have been disabled.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded_Initial_Load', 'en-US', N'The Process has {0} shapes. It exceeds the maximum of {1} shapes and cannot be edited. Please refactor it and move more detailed tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded', 'en-US', N'The shape cannot be added. The Process will exceed the maximum {0} shapes. Please refactor it and move more detailed tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Eighty_Percent_of_Shape_Limit_Reached', 'en-US', N'The Process now has {0} of the maximum {1} shapes. Please consider refactoring it to move more detailed tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Add_CannotAdd_MaximumConditionsReached', 'en-US', N'Cannot add any more conditions because the maximum number of conditions has been reached.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_UD_AtleastTwoConditions', 'en-US', N'The task cannot be deleted. The preceding decision point would be left with one choice. Decision points require at least two choices.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_OnlyUserTask', 'en-US', N'The task cannot be deleted. A Process requires at least one task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_UT_Between_Two_UD', 'en-US', N'The task cannot be deleted. Choices must begin with a task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Bulk_Delete_Confirmation', 'en-US', N'Process Shapes selected: {0}<br/>Deleting a shape also removes any associated shapes. Please review and confirm:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Settings_Label', 'en-US', N'Settings')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_System_Decision', 'en-US', N'Please confirm the deletion of the selected decision point. All conditions will also be deleted except for the first one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Decision', 'en-US', N'Please confirm the deletion of the selected decision point. All choices will also be deleted except for the first one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Comments_Label', 'en-US', N'Comments:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Relationships_Label', 'en-US', N'Relationships:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Userstory_Label', 'en-US', N'UserStory')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Task', 'en-US', N'Please confirm the deletion of the selected task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Task_System_Decision', 'en-US', N'Please confirm the deletion of the selected task. Its associated choice and conditions will also be deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Duplicate_Link_OrderIndex', 'en-US', N'There is an issue with ''{0} {1}({2})''. To resolve it, please remove one of the conditions (''{3}'' or ''{4}'') and add it again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_View_OpenedInReadonly_Message', 'en-US', N'Storyteller has been opened in read-only mode.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Task_Label', 'en-US', N'Action')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Task_Persona', 'en-US', N'User')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Task_Label', 'en-US', N'Response')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Task_Persona', 'en-US', N'System')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Decision_Label', 'en-US', N'UD')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Decision_Label', 'en-US', N'SD')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_NEW_MERGE_NODE_NAME', 'en-US', N'M')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_User_Task_Label', 'en-US', N'Add Task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_System_Decision_Label', 'en-US', N'Add Condition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_User_Decision_Label', 'en-US', N'Add Choice')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Insert_Shapes_Label', 'en-US', N'Insert Copied Shapes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Task_Name_Label', 'en-US', N'Action:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_System_Task_Name_Label', 'en-US', N'System Response:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_TaskNameNotValid_Label', 'en-US', N'The shape requires a label.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Persona_Label', 'en-US', N'Actor:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Persona_Helper_Text', 'en-US', N'Who/What is doing the task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_System_Name_Label', 'en-US', N'System Actor:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_System_Name_Helper_Text', 'en-US', N'Who/What is doing the task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Task_Name_Helper_Text', 'en-US', N'The actor wants to [Action]')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Objective_Label', 'en-US', N'Objective:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Objective_Helper_Text', 'en-US', N'So that the actor can [Objective]')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Show_Less', 'en-US', N'Show Less')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Show_More', 'en-US', N'Show More')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Additional_Info_Tab_Label', 'en-US', N'Additional Info')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Include_Tab_Label', 'en-US', N'Include')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Add_Include_Tab_Label', 'en-US', N'Add Include')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Include_New_Process_Label', 'en-US', N'New Process')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Include_Existing_Artifact_Label', 'en-US', N'Existing Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Inaccessible_Include_Artifact_Label', 'en-US', N'Inaccessible Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Browse_Include_Label', 'en-US', N'Browse')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Select_Include_Artifact_Label', 'en-US', N'Select Artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Inline_Traces_Search_No_Matches_Found', 'en-US', N'No matches found')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Inline_Traces_Search_Bad_Request', 'en-US', N'Bad search request')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Search_Project_Scope_Mentions_Includes', 'en-US', N'#mention or include artifacts from the current project only')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Response_Helper_Text', 'en-US', N'The system will be [Response]')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shapes_Delete_Tooltip', 'en-US', N'Delete Shape')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Decision_Modal_Decision_Label', 'en-US', N'Label:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Condition_Label', 'en-US', N'Condition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Choice_Label', 'en-US', N'Choice')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Decision_Modal_Next_Task_Label', 'en-US', N'Next Task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Cannot_Include_Parent_Process', 'en-US', N'A task cannot include its parent Process')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Userstory_Has_Been_Deleted', 'en-US', N'User story has been deleted or moved. To preview it, please generate it again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_SubArtifact_Has_Been_Deleted', 'en-US', N'This shape has been deleted or moved. Please refresh your display.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Given_Label', 'en-US', N'Given')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Then_Label', 'en-US', N'Then')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_When_Label', 'en-US', N'When')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Story_Acceptance_Criteria_Label', 'en-US', N'Acceptance Criteria')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Story_Nonfunctionals_Tab_Label', 'en-US', N'Nonfunctionals')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Story_Businessrules_Tab_Label', 'en-US', N'Business Rules')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Story_Title_Label', 'en-US', N'User Story Title')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Publish_User_Story', 'en-US', N'Publish User Story')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Search_Project_Scope_Mentions', 'en-US', N'#mention artifacts from the current project only')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_ProcessType_BusinessProcess_Label', 'en-US', N'Business Process mode')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_ProcessType_UserToSystemProcess_Label', 'en-US', N'User-System Process mode')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_ProcessType_RegenerateUS_Message', 'en-US', N'Regenerate user stories to synchronize your changes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_ProcessType_RegenerateUSS_Message', 'en-US', N'Published artifacts include one or more Processes. Regenerate corresponding user stories to synchronize changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Process_Validation_Error', 'en-US', N'Validation error found!')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Process_Include_Creation_Validation_Error_409_121', 'en-US', N'The Process ''{0}'' (ID:{1}) has been created and saved, but has issues with property values that prevent it from being published and included. Please open the Process and review its properties. After publishing it, you can include it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Breadcrumb_InaccessibleArtifact', 'en-US', N'<Inaccessible>')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_Dropdown_Tooltip', 'en-US', N'User Stories')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_All_Label', 'en-US', N'Generate All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_From_UserTask_Label', 'en-US', N'Generate from Task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_Confirm_Publish', 'en-US', N'The Process must be published before user stories are generated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_All_Success_Message', 'en-US', N'All user stories have been generated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_From_UserTask_Success_Message', 'en-US', N'The selected task’s user story has been generated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_Generic_Failure_Message', 'en-US', N'User stories cannot be generated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_US_Generate_LockedByOtherUser_Failure_Message', 'en-US', N'User stories cannot be generated. This artifact is now locked because another user has started editing it. Please refresh the artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ag-Grid_noRowsToShow', 'en-US', N'Empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Validation_MinThreeChars', 'en-US', N'Please use at least 3 characters.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Validation_Required', 'en-US', N'Required search term.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_Author', 'en-US', N'Author')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_CreatedOn', 'en-US', N'Created On:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_LastEdited', 'en-US', N'Last Edited:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_SubartifactMessage', 'en-US', N'Opening this item displays the artifact that contains it.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_NoResults', 'en-US', N'No Results Found')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_ServiceUnavailable', 'en-US', N'Service Unavailable. Please try again later.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Placeholder', 'en-US', N'Search open projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Delete_Artifact_Confirmation_All_Descendants', 'en-US', N'If you delete the artifact, any child artifact (listed below) and its descendants will also need to be deleted.<br/>Please review and confirm:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Delete_Artifact_Confirmation_Single', 'en-US', N'Please confirm the deletion of this artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Delete_Artifact_All_Success_Message', 'en-US', N'All ({0}) artifacts have been deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Delete_Artifact_Single_Success_Message', 'en-US', N'The artifact has been deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Search_Results_ResultsFound', 'en-US', N'{0} Results found')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Unpublished_Empty', 'en-US', N'No Unpublished Changes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('GO_TO_tooltip', 'en-US', N'Enter Artifact ID')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Copy_Images_Failed', 'en-US', N'Images were not copied into clipboard.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Label', 'en-US', N'Jobs')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Empty', 'en-US', N'No Jobs Available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_User', 'en-US', N'Author')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status', 'en-US', N'Status')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Scheduled', 'en-US', N'Scheduled')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Terminated', 'en-US', N'Terminated')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Running', 'en-US', N'Running')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Completed', 'en-US', N'Completed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Failed', 'en-US', N'Failed')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Cancelling', 'en-US', N'Cancelling')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Suspending', 'en-US', N'Suspending')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Status_Suspended', 'en-US', N'Suspended')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_None', 'en-US', N'None')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_System', 'en-US', N'System')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_DocGen', 'en-US', N'Document Generation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_TfsExport', 'en-US', N'TFS Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_QcExport', 'en-US', N'HP ALM-COM Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_HpAlmRestExport', 'en-US', N'HP ALM-REST Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_TfsChangeSummary', 'en-US', N'TFS Change Summary')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_QcChangeSummary', 'en-US', N'HP ALM-COM Change Summary')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_HpAlmRestChangeSummary', 'en-US', N'HP ALM-REST Change Summary')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_TfsExportTests', 'en-US', N'TFS Test Plan Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_QcExportTests', 'en-US', N'HP ALM-COM Test Plan Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_HpAlmRetExportTests', 'en-US', N'HP ALM-REST Test Plan Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_ExcelImport', 'en-US', N'Artifact Import from Excel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_ProjectImport', 'en-US', N'Project Import')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_ProjectExport', 'en-US', N'Project Export')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_GenerateTests', 'en-US', N'Test Plan Generation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Type_GenerateProcessTests', 'en-US', N'Process Test Case Generation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_SubmittedDate', 'en-US', N'Submitted on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_StartDate', 'en-US', N'Started on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_EndDate', 'en-US', N'Completed on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Error_Label', 'en-US', N'Error')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Jobs_Button_Label_GetJobs', 'en-US', N'Get Jobs')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Walkthrough_Button_Label', 'en-US', N'Walkthrough')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Process_Walkthrough_Mode_Tooltip', 'en-US', N'Walkthrough Mode')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Process_Diagram_Mode_Tooltip', 'en-US', N'Diagram Mode')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Zoom_Component_Reset_Button_Tooltip', 'en-US', N'Reset to 100%')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Zoom_Component_FitWidth_Button_Tooltip', 'en-US', N'Fit to width')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'fr-CA', N'Oui')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'fr-CA', N'Non')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'fr-CA', N'D''accord')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'fr-CA', N'Annuler')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'fr-CA', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'fr-CA', N'Chargement …')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'fr-CA', N'Vous devez activer JavaScript pour utiliser cette application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'fr-CA', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'fr-CA', N'Signé en tant')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Welcome', 'fr-CA', N'Bienvenue')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_CannotGetUser', 'fr-CA', N'Vous ne pouvez pas obtenir l''utilisateur actuel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_IncorrectRequestId', 'fr-CA', N'Mauvais demande id . S''il vous plaît cliquer sur ''Retry'' dans Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LoginFailed', 'fr-CA', N'Échec de la connexion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SamlContinueSessionWithOriginalUser', 'fr-CA', N'Pour continuer votre session, s''il vous plaît vous connecter avec le même utilisateur que la session a commencé avec.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseVerificationFailed', 'fr-CA', N'Impossible de vérifier la licence')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_SessionTokenRetrievalFailed', 'fr-CA', N'Vous ne pouvez pas obtenir jeton de session')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseNotFound_Verbose', 'fr-CA', N'Aucune licence trouvé ou Blueprint utilise une licence de serveur non valide . S''il vous plaît contactez votre administrateur Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_LicenseLimitReached', 'fr-CA', N'La limite maximale de licence concurrente a été atteint. S''il vous plaît contactez votre administrateur Blueprint.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Auth_FederatedAuthFailed', 'fr-CA', N'There is a problem with federated authentication. Please contact your administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Username', 'fr-CA', N'Nom d''utilisateur')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Password', 'fr-CA', N'Mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ForgotPassword', 'fr-CA', N'Mot de passe oublié?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePasswordButton', 'fr-CA', N'Changer le mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_UpdatePasswordButton', 'fr-CA', N'Update Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginButton', 'fr-CA', N'S''identifier')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_SamlLink', 'fr-CA', N'Connectez-vous pour blueprint.toronto')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_GoBackToLogin', 'fr-CA', N'Retour à identifier')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ResetPassword', 'fr-CA', N'Réinitialiser le mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Retry', 'fr-CA', N'Recommencez')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_LoginPrompt', 'fr-CA', N'Connexion avec identifiant et mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_CurrentPassword', 'fr-CA', N'Mot de passe actuel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_NewPassword', 'fr-CA', N'Nouveau mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_ChangePassword_ConfirmPassword', 'fr-CA', N'Confirmer le nouveau mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_BlueprintCopyRight', 'fr-CA', N'Blueprint Software Systems Inc. Tous droits réservés')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Version', 'fr-CA', N'Version:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsCannotBeEmpty', 'fr-CA', N'Nom d''utilisateur et mot de passe ne peut pas être vide')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_DuplicateSession_Verbose', 'fr-CA', N'Cet utilisateur est déjà connecté à Blueprint dans un autre navigateur / session. <br><br>Voulez-vous remplacer la session précédente ?')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCredentials', 'fr-CA', N'Veuillez s''il vous plaît entrer votre nom d''utilisateur et votre mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterUsername', 'fr-CA', N'S''il vous plaît entrez votre nom d''utilisateur')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired_ChangePasswordPrompt', 'fr-CA', N'Votre mot de passe a expiré. S''il vous plaît changer votre mot de passe ci-dessous.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterSamlCredentials_Verbose', 'fr-CA', N'S''il vous plaît authentifier en utilisant vos informations d''identification d''entreprise dans la fenêtre qui a ouvert . Si vous ne voyez pas la fenêtre , s''il vous plaît vous assurer que votre bloqueur de popup est désactivé , puis cliquez sur le bouton Retry. <br><br> Vous serez automatiquement connecté après que vous êtes authentifié .')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CredentialsInvalid', 'fr-CA', N'S''il vous plaît entrer un nom d''utilisateur correct et mot de passe')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_AccountDisabled', 'fr-CA', N'Votre compte a été désactivé. <br> S''il vous plaît contactez votre administrateur.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordHasExpired', 'fr-CA', N'Votre mot de passe a expiré.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_EnterCurrentPassword', 'fr-CA', N'Please enter the correct current Password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_CurrentPasswordCannotBeEmpty', 'fr-CA', N'Current Password cannot be empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordChangedSuccessfully', 'fr-CA', N'Password changed successfully')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_PasswordConfirmMismatch', 'fr-CA', N'Confirm password does not match new password')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMaxLength', 'fr-CA', N'New password must be at most 128 characters long')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordMinLength', 'fr-CA', N'New password must be at least 8 characters long')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCannotBeEmpty', 'fr-CA', N'New password cannot be empty')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordSameAsOld', 'fr-CA', N'New password cannot be the same as the old one')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCriteria', 'fr-CA', N'New password must contain at least one capital letter, number and symbol')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'fr-CA', N'Obligatoire')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message1', 'fr-CA', N'An error ocurred while loading the page.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message2', 'fr-CA', N'Please try again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message3', 'fr-CA', N'If the problem persists, contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Label', 'fr-CA', N'Error')

-- Add application label if [Key]/[Locale] combination does not exist
INSERT INTO [dbo].[ApplicationLabels] ([Key], [Locale], [Text])
SELECT #tempAppLabels.[Key], #tempAppLabels.[Locale], #tempAppLabels.[Text]
  FROM #tempAppLabels
  LEFT JOIN [dbo].[ApplicationLabels] 
		ON	[dbo].[ApplicationLabels].[Key] = #tempAppLabels.[Key]
		AND [dbo].[ApplicationLabels].[Locale] = #tempAppLabels.[Locale]
 WHERE [dbo].[ApplicationLabels].[Key] is NULL
   AND [dbo].[ApplicationLabels].[Locale] is NULL

-- Update if [Key]/[Locale] combination exists, but text is different
UPDATE [dbo].[ApplicationLabels]
   SET [dbo].[ApplicationLabels].[Text] = #tempAppLabels.[Text]
  FROM [dbo].[ApplicationLabels]
  JOIN #tempAppLabels 
		ON [dbo].[ApplicationLabels].[Key] = #tempAppLabels.[Key]
	   AND [dbo].[ApplicationLabels].[Locale] = #tempAppLabels.[Locale]
	   AND [dbo].[ApplicationLabels].[Text] <> #tempAppLabels.[Text] COLLATE SQL_Latin1_General_CP437_BIN2

-- Delete if [Key]/[Locale] combination no longer exists
DELETE
  FROM [dbo].[ApplicationLabels]
 WHERE NOT EXISTS ( SELECT *
					  FROM #tempAppLabels 
					 WHERE #tempAppLabels.[Key] = [dbo].[ApplicationLabels].[Key]
					   AND #tempAppLabels.[Locale] = [dbo].[ApplicationLabels].[Locale])

DROP TABLE #tempAppLabels

-- End of Auto-Generation of SQL insert statements for Application Labels
-- -----------------------------------------------------------------------------------------------


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
EXEC [dbo].[SetSchemaVersion] @value = N'8.1.0';
GO
-- --------------------------------------------------


