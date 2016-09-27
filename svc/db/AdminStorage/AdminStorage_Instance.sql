
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
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [dbo].[ApplicationLabels]
GO

CREATE TABLE [dbo].[ApplicationLabels](
	[ApplicationLabelId] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](128) NOT NULL,
	[Locale] [nvarchar](32) NOT NULL,
	[Text] [nvarchar](512) NOT NULL,

	CONSTRAINT [PK_ApplicationLabels_ApplicationLabelId] PRIMARY KEY CLUSTERED 
	(
		[ApplicationLabelId] ASC
	)
) ON [PRIMARY]
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_ApplicationLabels_Key_Locale')
	DROP INDEX IX_ApplicationLabels_Key_Locale on [dbo].[ApplicationLabels]
GO

CREATE NONCLUSTERED INDEX IX_ApplicationLabels_Key_Locale on  [dbo].[ApplicationLabels] 
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
)

INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Yes', 'en-US', N'Yes')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_No', 'en-US', N'No')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Ok', 'en-US', N'OK')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Cancel', 'en-US', N'Cancel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_Open', 'en-US', N'Open')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Button_PublishAndContinue', 'en-US', N'Publish and Continue')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Alert', 'en-US', N'Warning')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_DialogTitle_Confirmation', 'en-US', N'Confirmation')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_LoadingMsg', 'en-US', N'Loading ...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PublishBeforeGeneratingUserStoriesMsg', 'en-US', N'The Process must be published before user stories are generated.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_PrerequisiteMsg_JavaScript', 'en-US', N'You must enable JavaScript to use this application')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Name', 'en-US', N'Blueprint')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_SignedInAs', 'en-US', N'Signed in as')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Header_Welcome', 'en-US', N'Welcome')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_App_Header_Help', 'en-US', N'Help')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_App_Header_Logout', 'en-US', N'Log out')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Tooltip_Artifact_Header_LegacyArtifact', 'en-US', N'Legacy artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Sidebar_Left', 'en-US', N'Explorer')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Sidebar_Right', 'en-US', N'Utility Panel')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Open', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Project_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Project', 'en-US', N'Open Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Open_Recent_Projects', 'en-US', N'Open Recent Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_Project', 'en-US', N'Close Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Close_All_Projects', 'en-US', N'Close All Projects')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Save_All', 'en-US', N'Save All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Discard_All', 'en-US', N'Discard All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Publish_All', 'en-US', N'Publish All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Refresh_All', 'en-US', N'Refresh All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Delete', 'en-US', N'Delete')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Toolbar_Tour', 'en-US', N'Tour')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Collapsible_ShowMore', 'en-US', N'Show more')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Collapsible_ShowLess', 'en-US', N'Show less')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Discussions', 'en-US', N'Discussions')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Properties', 'en-US', N'Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Attachments', 'en-US', N'Files')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Relationships', 'en-US', N'Relationships')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_History', 'en-US', N'History')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UtilityPanel_Reviews', 'en-US', N'Reviews')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Error', 'en-US', N'Upload error.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Empty', 'en-US', N'There are no files to upload.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Total_Failed', 'en-US', N'Total items failed:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Upload_Dialog_Header', 'en-US', N'File Upload')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Attachments_Add_Same_DocRef_Error', 'en-US', N'Artifact already contains this document reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Change', 'en-US', N'Change')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Upload', 'en-US', N'Upload')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Document_File_Download', 'en-US', N'Download')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Introduction_Message', 'en-US', N'You may collaborate with others using this discussion panel to add comments to any selected artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_Start_Discussion', 'en-US', N'+ Start a new discussion')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_New_Discussion', 'en-US', N'+ New comment')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Discussions_No_Artifact', 'en-US', N'The item you selected has no information to display or you do not have access to view it.')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_All', 'en-US', N'All')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Traces', 'en-US', N'Traces')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Association', 'en-US', N'Other')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Collapse', 'en-US', N'Hide details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Expand', 'en-US', N'Show details')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Relationships', 'en-US', N'No relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Traces', 'en-US', N'No traces available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_No_Association', 'en-US', N'No other relationships available')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Association', 'en-US', N'Association')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Actor_Inherits', 'en-US', N'Actor Inherits')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Subheading_Document_Reference', 'en-US', N'Document Reference')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Unauthorized', 'en-US', N'Unauthorized')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_UP_Relationships_Selected', 'en-US', N'Selected:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Loading_Indicator_Label', 'en-US', N'Loading...')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_System_Properties_Label', 'en-US', N'System Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Additional_Properties_Label', 'en-US', N'Additional Properties')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_StepOf_Actor', 'en-US', N'Actor (Input)')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_StepOf_System', 'en-US', N'System (Expected Outcome)')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_InheritancePicker_Title', 'en-US', N'Select Actor')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Properties_Actor_SameBaseActor_ErrorMessage', 'en-US', N'Actor cannot be set as its own parent')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_200', 'en-US', N'The artifact has been saved.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_400', 'en-US', N'An error has occurred and the artifact is no longer valid. Please contact an administrator.<br><br>')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_404', 'en-US', N'Sorry, but the artifact cannot be saved because it has been deleted or moved. Please Refresh All.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409', 'en-US', N'There was a conflict while saving the artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_400_114', 'en-US', N'The artifact cannot be saved. Please ensure all values are correct.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_115', 'en-US', N'Sorry, but the artifact cannot be saved because your lock on it was stolen by another user. Please Refresh the artifact. Any changes you have made will be lost.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_116', 'en-US', N'The artifact cannot be saved. It is a reused artifact with read-only settings. You have modified read-only properties.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_117', 'en-US', N'A property type is out of date.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_409_123', 'en-US', N'Cannot save an artifact because it has a cyclic relationship.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('App_Save_Artifact_Error_Other', 'en-US', N'Sorry, but an error has occurred and the artifact cannot be saved. Please contact an administrator.<br><br>')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Term', 'en-US', N'Term')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Definition', 'en-US', N'Definition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Glossary_Empty', 'en-US', N'No terms have been defined.')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_NewPasswordCriteria', 'en-US', N'Your new password must contain at least one number, <br>uppercase letter, and non-alphanumeric character.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Login_Session_Timeout', 'en-US', N'​Your session has expired or was overridden.<br>Please log in to continue.​')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Input_Required', 'en-US', N'Required')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Folder_NotFound', 'en-US', N'Couldn''t find specified project or folder')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_NotFound', 'en-US', N'Couldn''t find the project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_MetaDataNotFound', 'en-US', N'Couldn''t find the project meta data')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_NotFound', 'en-US', N'Couldn''t find the artifact')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_DoesNotExistOrMoved', 'en-US', N'The artifact doesn''t exists or moved to different location')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ArtifactType_NotFound', 'en-US', N'Couldn''t find the artifact type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_AlreadyLocked', 'en-US', N'Artifact locked by another user')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_DoesNotExist', 'en-US', N'The artifact has been deleted or moved.<br />The Explorer will now be refreshed.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_AccessDenied', 'en-US', N'Cannot establish the artifact lock. Access denied.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Artifact_Lock_Failure', 'en-US', N'Cannot establish the artifact lock due to general failure')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Name', 'en-US', N'Name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Type', 'en-US', N'Type')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedBy', 'en-US', N'Created by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_CreatedOn', 'en-US', N'Created on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditBy', 'en-US', N'Last edited by')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_LastEditOn', 'en-US', N'Last edited on')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Description', 'en-US', N'Description')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Project', 'en-US', N'Project')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Collections', 'en-US', N'Collections')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_ActorInheritFrom', 'en-US', N'Inherits from')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Label_Group_Identifier', 'en-US', N'(g)')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Property_UserPicker_Display_Top_N_Results', 'en-US', N'Displaying top {0} results')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_ServiceUnavailable', 'en-US', N'Service is unavailable')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_Forbidden', 'en-US', N'You do not have permission to view this artifact.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_NotFound', 'en-US', N'Not found. A project or an artifact for the specified ids is not found, does not exist or is deleted')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('HttpError_InternalServer', 'en-US', N'Internal Server Error. An error occurred')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Project_NoProjectsAvailable', 'en-US', N'Either no projects are available or you do not have the required permissions to access them.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message1', 'en-US', N'An error ocurred while loading the page.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message2', 'en-US', N'Please try again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Message3', 'en-US', N'If the problem persists, contact your Blueprint administrator.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Error_Page_Label', 'en-US', N'Error')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('Email_Discussions_Disabled_Message', 'en-US', N'Note: Email Discussions have been disabled.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded_Initial_Load', 'en-US', N'The Process has {0} shapes. It exceeds the maximum of {1} shapes and cannot be edited. Please refactor it and move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Shape_Limit_Exceeded', 'en-US', N'The shape cannot be added. The Process will exceed the maximum {0} shapes. Please refactor it and move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Eighty_Percent_of_Shape_Limit_Reached', 'en-US', N'The Process now has {0} of the maximum {1} shapes. Please consider refactoring it to move more detailed user tasks to included Processes.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Decision_Modal_New_System_Task_Edge_Label', 'en-US', N'Condition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Add_CannotAdd_MaximumConditionsReached', 'en-US', N'Cannot add any more conditions because the maximum number of conditions has been reached.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_UD_AtleastTwoConditions', 'en-US', N'The task cannot be deleted. The preceding decision point would be left with one condition. Decision points require at least two conditions.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_OnlyUserTask', 'en-US', N'The task cannot be deleted. A Process requires at least one user task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Delete_CannotDelete_UT_Between_Two_UD', 'en-US', N'The task cannot be deleted. User decision points model user choices. Each decision-point condition must begin with a task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Settings_Label', 'en-US', N'Settings')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_System_Decision', 'en-US', N'Please confirm the deletion of the selected decision point. All conditions will also be deleted except for the first one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Decision', 'en-US', N'Please confirm the deletion of the selected decision point. All conditions will also be deleted except for the first one.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Comments_Label', 'en-US', N'Comments:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Relationships_Label', 'en-US', N'Relationships:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Userstory_Label', 'en-US', N'UserStory')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Task', 'en-US', N'Please confirm the deletion of the selected user task.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Confirm_Delete_User_Task_System_Decision', 'en-US', N'Please confirm the deletion of the selected user task. Its associated decision point and conditions will also be deleted.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Duplicate_Link_OrderIndex', 'en-US', N'There is an issue with decision point ''{0}({1})''. To resolve it, please remove one of the conditions (''{2}'' or ''{3}'') and add it again.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_View_OpenedInReadonly_Message', 'en-US', N'Storyteller has been opened in read-only mode.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Task_Label', 'en-US', N'UT')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Task_Persona', 'en-US', N'User')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Task_Label', 'en-US', N'ST')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Task_Persona', 'en-US', N'System')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_User_Decision_Label', 'en-US', N'UD')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_New_System_Decision_Label', 'en-US', N'SD')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_NEW_MERGE_NODE_NAME', 'en-US', N'M')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Decision_Modal_Add_Condition_Button_Label', 'en-US', N'Add Condition')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_User_Task_Label', 'en-US', N'Add User Task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_System_Decision_Label', 'en-US', N'Add System Decision Point')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Popup_Menu_Add_User_Decision_Label', 'en-US', N'Add User Decision Point')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Task_Name_Label', 'en-US', N'Action:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_System_Task_Name_Label', 'en-US', N'System Response:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_TaskNameNotValid_Label', 'en-US', N'The shape requires a label.')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Persona_Label', 'en-US', N'Persona:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Persona_Helper_Text', 'en-US', N'Who/What is doing the task')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_User_Task_Name_Helper_Text', 'en-US', N'The persona wants to [Action]')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Objective_Label', 'en-US', N'Objective:')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Objective_Helper_Text', 'en-US', N'So that the persona can [Objective]')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Show_Less', 'en-US', N'Show Less')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Show_More', 'en-US', N'Show More')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Additional_Info_Tab_Label', 'en-US', N'Additional Info')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Include_Tab_Label', 'en-US', N'Include')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Include_Tab_Placeholder', 'en-US', N'Search by artifact name')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Inline_Traces_Search_No_Matches_Found', 'en-US', N'No matches found')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Inline_Traces_Search_Bad_Request', 'en-US', N'Bad search request')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_Search_Project_Scope_Mentions_Includes', 'en-US', N'#mention or include artifacts from the current project only')
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ST_System_Task_Name_Helper_Text', 'en-US', N'The system will be [Response]')
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
INSERT INTO #tempAppLabels ([Key], [Locale], [Text]) VALUES ('ag-Grid_noRowsToShow', 'en-US', N'Empty')
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
 WHERE NOT EXISTS ( SELECT *
					  FROM [dbo].[ApplicationLabels] 
					 WHERE [dbo].[ApplicationLabels].[Key] = #tempAppLabels.[Key]
					   AND [dbo].[ApplicationLabels].[Locale] = #tempAppLabels.[Locale])

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
EXEC [dbo].[SetSchemaVersion] @value = N'7.4.0';
GO
-- --------------------------------------------------


