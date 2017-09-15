
-- --------------------------------------------------
-- Set the DB
-- --------------------------------------------------
SET QUOTED_IDENTIFIER ON;
GO
USE [Blueprint_AdminStorage]; -- REPLACE --
GO
SET NOCOUNT ON;
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

-- Create the AdminStore Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'AdminStore')
EXEC sys.sp_executesql N'CREATE SCHEMA [AdminStore]'
GO

/******************************************************************************************************************************
Name:			IsSchemaVersionLessOrEqual

Description: 
			
Change History:
Date			Name					Change

******************************************************************************************************************************/

-- Migrate table to the AdminStore schema
IF (OBJECT_ID(N'[dbo].[DbVersionInfo]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[DbVersionInfo]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[DbVersionInfo];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [AdminStore].[IsSchemaVersionLessOrEqual]
GO

CREATE FUNCTION [AdminStore].[IsSchemaVersionLessOrEqual]
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
SELECT TOP(1) @schemaVersion = [SchemaVersion] FROM [AdminStore].[DbVersionInfo] WHERE ([SchemaVersion] IS NOT NULL);
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[SetSchemaVersion]
GO

CREATE PROCEDURE [AdminStore].[SetSchemaVersion]
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

IF EXISTS (SELECT * FROM [AdminStore].[DbVersionInfo])
	BEGIN 
		UPDATE [AdminStore].[DbVersionInfo] SET [SchemaVersion] = @value FROM [AdminStore].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [AdminStore].[DbVersionInfo] SELECT 1, @value;
	END 

GO


-- Migrate tables to the AdminStore schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[ApplicationLabels]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[ApplicationLabels]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[ApplicationLabels];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[ConfigSettings]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[ConfigSettings]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[ConfigSettings];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[Sessions]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[Sessions]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[Sessions];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[LicenseActivityDetails]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[LicenseActivityDetails]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[LicenseActivityDetails];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[LicenseActivities]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[LicenseActivities]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[LicenseActivities];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[Logs]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[Logs]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[Logs];
GO

IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	AND (OBJECT_ID(N'[dbo].[PasswordRecoveryTokens]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[PasswordRecoveryTokens]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[PasswordRecoveryTokens];
GO


/******************************************************************************************************************************
Name:			LogsType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogsType' AND ss.name = N'AdminStore')
DROP TYPE [AdminStore].[LogsType]
GO

CREATE TYPE [AdminStore].[LogsType] AS TABLE
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


-- --------------------------------------------------
-- Migration 7.0.1.0
-- --------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0) 
	set noexec on
Print 'Migrating 7.0.1.0 ...'
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
Name:			LogsType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'LogsType' AND ss.name = N'AdminStore')
DROP TYPE [AdminStore].[LogsType]
GO

CREATE TYPE [AdminStore].[LogsType] AS TABLE
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

-- Migrate table to the AdminStore schema
IF (OBJECT_ID(N'[dbo].[DbVersionInfo]', 'U') IS NOT NULL) AND (OBJECT_ID(N'[AdminStore].[DbVersionInfo]', 'U') IS NULL)
	ALTER SCHEMA [AdminStore] TRANSFER [dbo].[DbVersionInfo];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[IsSchemaVersionLessOrEqual]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [AdminStore].[IsSchemaVersionLessOrEqual]
GO

CREATE FUNCTION [AdminStore].[IsSchemaVersionLessOrEqual]
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
SELECT TOP(1) @schemaVersion = [SchemaVersion] FROM [AdminStore].[DbVersionInfo] WHERE ([SchemaVersion] IS NOT NULL);
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[SetSchemaVersion]
GO

CREATE PROCEDURE [AdminStore].[SetSchemaVersion]
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

IF EXISTS (SELECT * FROM [AdminStore].[DbVersionInfo])
	BEGIN 
		UPDATE [AdminStore].[DbVersionInfo] SET [SchemaVersion] = @value FROM [AdminStore].[DbVersionInfo];
	END
ELSE
	BEGIN 
		INSERT INTO [AdminStore].[DbVersionInfo] SELECT 1, @value;
	END 

GO

/******************************************************************************************************************************
Name:			GetStatus

Description:	Returns the version of the database.

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetStatus]
GO

CREATE PROCEDURE [AdminStore].[GetStatus] 
AS
BEGIN
	SELECT [SchemaVersion] FROM [AdminStore].[DbVersionInfo] WHERE [Id] = 1;
END
GO 


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[BeginSession]
GO

CREATE PROCEDURE [AdminStore].[BeginSession] 
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
		SELECT @OldSessionId = SessionId FROM [AdminStore].[Sessions] WHERE UserId = @UserId;
		DECLARE @NewSessionId uniqueidentifier = NEWID();
		IF @OldSessionId IS NULL
		BEGIN
			INSERT [AdminStore].[Sessions] (UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso)
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			VALUES (@UserId, @NewSessionId, @BeginTime, @EndTime, @UserName, @LicenseLevel, @IsSso);
		END
		ELSE
		BEGIN
			UPDATE [AdminStore].[Sessions]
			SET [SessionId] = @NewSessionId, [BeginTime] = @BeginTime, [EndTime] = @EndTime, [UserName] = @UserName, [LicenseLevel] = @LicenseLevel, [IsSso] = @IsSso 
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			WHERE [SessionId] = @OldSessionId;
		END

		-- [LicenseActivities]
		INSERT INTO [AdminStore].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
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
		INSERT INTO @ActiveLicenses EXEC [AdminStore].[GetActiveLicenses] @BeginTime, @LicenseLockTimeMinutes
		INSERT INTO [AdminStore].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ExtendSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[ExtendSession]
GO

CREATE PROCEDURE [AdminStore].[ExtendSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	DECLARE @utcNow DATETIME = GETUTCDATE();

	UPDATE [AdminStore].[Sessions] SET EndTime = @EndTime
	OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
	WHERE SessionId = @SessionId AND BeginTime IS NOT NULL AND @utcNow < EndTime;
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[EndSession]
GO

CREATE PROCEDURE [AdminStore].[EndSession] 
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
		UPDATE [AdminStore].[Sessions] SET [BeginTime] = NULL, [EndTime] = @EndTime
		OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
		WHERE [SessionId] = @SessionId AND [BeginTime] IS NOT NULL
		AND (@TimeoutTime IS NULL OR @TimeoutTime = [EndTime]);

		IF @@ROWCOUNT > 0 BEGIN
			-- [LicenseActivities]
			DECLARE @UserId int
			DECLARE @LicenseLevel int
			SELECT @UserId = [UserId], @LicenseLevel = [LicenseLevel] FROM [AdminStore].[Sessions] WHERE [SessionId] = @SessionId;
			INSERT INTO [AdminStore].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
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
			INSERT INTO @ActiveLicenses EXEC [AdminStore].[GetActiveLicenses] @EndTime, @LicenseLockTimeMinutes
			INSERT INTO [AdminStore].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetApplicationLabels]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetApplicationLabels]
GO

CREATE PROCEDURE [AdminStore].[GetApplicationLabels] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [AdminStore].ApplicationLabels WHERE Locale = @Locale;
END
GO
/******************************************************************************************************************************
Name:			GetConfigSettings

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetConfigSettings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetConfigSettings]
GO

CREATE PROCEDURE [AdminStore].[GetConfigSettings] 
	@AllowRestricted bit
AS
BEGIN
	SELECT [Key], [Value], [Group], IsRestricted FROM [AdminStore].ConfigSettings WHERE IsRestricted = @AllowRestricted OR @AllowRestricted = 1;
END
GO
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetSession]
GO

CREATE PROCEDURE [AdminStore].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where SessionId = @SessionId;
END
GO 
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/17		Anton Trinkunas			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetUserSession]
GO

CREATE PROCEDURE [AdminStore].[GetUserSession] 
(
	@UserId int
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where UserId = @UserId;
END
GO 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SelectSessions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[SelectSessions]
GO

CREATE PROCEDURE [AdminStore].[SelectSessions] 
(
	@ps int,
	@pn int
)
AS
BEGIN
	WITH SessionsRN AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY BeginTime DESC) AS RN, UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso 
		FROM [AdminStore].[Sessions] WHERE BeginTime IS NOT NULL
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetActiveLicenses]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetActiveLicenses]
GO

CREATE PROCEDURE [AdminStore].[GetActiveLicenses] 
(
	@Now datetime,
	@LicenseLockTimeMinutes int
)
AS
BEGIN
	DECLARE @EndTime datetime = DATEADD(MINUTE, -@LicenseLockTimeMinutes, @Now)
	SELECT [LicenseLevel], COUNT(*) as [Count]
	FROM [AdminStore].[Sessions] 
	WHERE [EndTime] IS NULL OR [EndTime] > @EndTime
	GROUP BY [LicenseLevel]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLicenseTransactions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLicenseTransactions]
GO

CREATE PROCEDURE [AdminStore].[GetLicenseTransactions] 
(
	@StartTime datetime,
	@ConsumerType int
)
AS
BEGIN
	SELECT [UserId], [UserLicenseType] AS [LicenseType], [TransactionType], [ActionType], [TimeStamp] AS [Date],
		ISNULL(STUFF((SELECT ';' + CAST([LicenseType] AS VARCHAR(10)) + ':' + CAST([Count] AS VARCHAR(10))
		FROM [AdminStore].[LicenseActivityDetails] D
		WHERE D.[LicenseActivityId] = A.[LicenseActivityId]
		FOR XML PATH('')), 1, 1, ''), '') AS [Details]
	FROM [AdminStore].[LicenseActivities] A
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

CREATE PROCEDURE [AdminStore].[WriteLogs]  
(
  @InsertLogs LogsType READONLY
)
AS
BEGIN
  INSERT INTO [AdminStore].[Logs] (
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[DeleteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[DeleteLogs]
GO

CREATE PROCEDURE [AdminStore].[DeleteLogs] 
AS
BEGIN
  -- Get the number of days to keep from config settings - DEFAULT 7
  DECLARE @days int
  SELECT @days=c.[Value] FROM [AdminStore].ConfigSettings c WHERE c.[Key] = N'DaysToKeepInLogs'
  SELECT @days=COALESCE(@days, 7) 

  -- Delete old log records
  DELETE FROM [AdminStore].[Logs] WHERE [Timestamp] <= DATEADD(DAY, @days*-1, SYSDATETIMEOFFSET()) 
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLogs]
GO


CREATE PROCEDURE [AdminStore].[GetLogs]  
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

	SELECT @total = COUNT(*) FROM [AdminStore].[Logs] where @id = 0 OR ID <= @id 	

	SET @fetch = IIF(@recordlimit < 0, @total, @recordlimit)

	SELECT TOP (@fetch) * FROM [AdminStore].[Logs] WHERE @id = 0 OR ID <= @id ORDER BY Id DESC

END



GO




DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
-- We can't do the following line, because we don't have access to the table in Amazon RDS:
--      IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN TRY
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END TRY
BEGIN CATCH
END CATCH

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
SET @cmd = N'[AdminStore].[DeleteLogs]'
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

IF NOT EXISTS(SELECT [KEY] FROM [AdminStore].[ConfigSettings] WHERE [Key]=N'DaysToKeepInLogs')
BEGIN
	INSERT INTO [AdminStore].[ConfigSettings] ([Key], [Value], [Group], [IsRestricted])
		 VALUES (N'DaysToKeepInLogs', N'7', N'Maintenance', 0)
END

-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.0.1') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.0.1';
GO
set noexec off
-- --------------------------------------------------

-- -----------------------------------------------------------------------------------------------
-- Migration 7.1.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0) 
	set noexec on
Print 'Migrating 7.1.0.0 ...'
-- -----------------------------------------------------------------------------------------------

/******************************************************************************************************************************
Name:			ApplicationLabels

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
2016/09/29		Areag Osman				Extends character limit for Key & Text columns, adds index for table
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [AdminStore].[ApplicationLabels]
GO

CREATE TABLE [AdminStore].[ApplicationLabels](
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
	DROP INDEX IX_ApplicationLabels_Key_Locale on [AdminStore].[ApplicationLabels]
GO

CREATE NONCLUSTERED INDEX IX_ApplicationLabels_Key_Locale on  [AdminStore].[ApplicationLabels] 
(
	[Key] ASC,
	[Locale] ASC
)
GO


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.1.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.1.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 7.2.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.2.0') <> 0) 
	set noexec on
Print 'Migrating 7.2.0.0 ...'
-- -----------------------------------------------------------------------------------------------



-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.2.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.2.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 7.3.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0) 
	set noexec on
Print 'Migrating 7.3.0.0 ...'
-- -----------------------------------------------------------------------------------------------

-- -----------------------------------------------------------------------------
-- Modify the database filegrowth if it has not changed from the prior defaults
-- -----------------------------------------------------------------------------
DECLARE @db_name AS NVARCHAR(128) = DB_NAME();
DECLARE @sql AS NVARCHAR(max);
DECLARE @file_name AS SYSNAME

SELECT @file_name = d.name FROM sys.database_files d WHERE d.type = 0 AND d.is_percent_growth = 0 AND d.growth = 1280
IF (@file_name IS NOT NULL) 
BEGIN
    SET @sql = N'ALTER DATABASE [' + @db_name + '] MODIFY FILE ( NAME = N''' + @file_name + ''', FILEGROWTH = 10% )'

    EXEC(@sql);
END 

GO

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.3.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.3.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 7.4.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0) 
	set noexec on
Print 'Migrating 7.4.0.0 ...'
-- -----------------------------------------------------------------------------------------------

-- -----------------------------------------------------------------------------
-- Modify [AdminStore].[ApplicationLabels] to have a primary key on [Key] and [Locale]
-- -----------------------------------------------------------------------------

IF  EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_ApplicationLabels_ApplicationLabelId')
BEGIN

	-- Remove existing constraint
	ALTER TABLE [AdminStore].[ApplicationLabels] 
	DROP CONSTRAINT [PK_ApplicationLabels_ApplicationLabelId]

	-- Creating primary key on [ApplicationLabelId], [Key], [Locale] in table 'ApplicationLabels'
	ALTER TABLE [AdminStore].[ApplicationLabels]
	ADD CONSTRAINT [PK_ApplicationLabels] PRIMARY KEY NONCLUSTERED 
	(
		[Key], [Locale] ASC
	);
END

GO

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.4.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 7.4.1.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.1') <> 0) 
	set noexec on
Print 'Migrating 7.4.1.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'7.4.1') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'7.4.1';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 8.0.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.0.0') <> 0) 
	set noexec on
Print 'Migrating 8.0.0.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.0.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'8.0.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 8.1.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0) 
	set noexec on
Print 'Migrating 8.1.0.0 ...'
-- -----------------------------------------------------------------------------------------------

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[PasswordRecoveryTokens]') AND type in (N'U'))
    CREATE TABLE [AdminStore].[PasswordRecoveryTokens](
        [Login] [nvarchar](max),
        [CreationTime] [datetime] NOT NULL,
        [RecoveryToken] [uniqueidentifier] NOT NULL,

	     CONSTRAINT [PK_PasswordRecoveryTokens] PRIMARY KEY CLUSTERED 
    (
	    [RecoveryToken] ASC
    )) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ApplicationLabels]') AND type in (N'U'))
DROP TABLE [AdminStore].[ApplicationLabels]
GO


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.1.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'8.1.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 8.2.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0) 
	set noexec on
Print 'Migrating 8.2.0.0 ...'
-- -----------------------------------------------------------------------------------------------

-- Drop all functions associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
	AND (OBJECT_ID(N'[dbo].[IsSchemaVersionLessOrEqual]', 'FN') IS NOT NULL)
	DROP FUNCTION [dbo].[IsSchemaVersionLessOrEqual];
GO

-- Drop all procedures associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
BEGIN
	DECLARE @name AS sysname, @tsql AS nvarchar(max), @beginCount AS int, @endCount AS int;
	DECLARE NameCursor CURSOR FOR
	SELECT o.[name] FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
	WHILE (0 = 0)
	BEGIN
		SELECT @beginCount = COUNT(*) FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
		OPEN NameCursor;
		FETCH NEXT FROM NameCursor INTO @name;
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			SET @tsql = N'DROP PROCEDURE [dbo].[' + CAST(@name AS nvarchar(256)) + N'];';
			BEGIN TRY
				EXEC(@tsql);
			END TRY
			BEGIN CATCH
			END CATCH
			FETCH NEXT FROM NameCursor INTO @name;
		END
		CLOSE NameCursor;
		SELECT @endCount = COUNT(*) FROM [sys].[objects] AS o INNER JOIN [sys].[schemas] AS s ON (o.[schema_id] = s.[schema_id]) WHERE (s.[name] = N'dbo') AND (o.[type] = 'P');
		IF ((@endCount = 0) OR (@endCount = @beginCount))
			BREAK;
	END
	DEALLOCATE NameCursor;
END
GO

-- Drop all types associated with dbo schema
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	-- Distributed database installation only
	AND (OBJECT_ID(N'[dbo].[Instances]', 'U') IS NULL)
	AND (TYPE_ID(N'[dbo].[LogsType]') IS NOT NULL)
	DROP TYPE [dbo].[LogsType];
GO

DECLARE @blueprintDB SYSNAME, @jobname SYSNAME, @schedulename SYSNAME
DECLARE @jobId BINARY(16), @cmd varchar(2000)

SET @blueprintDB = DB_NAME()
SET @jobname = @blueprintDB+N'_Maintenance'
SET @schedulename = @blueprintDB+N'_Maintenance_Schedule'

-- drop the job if it exists
-- We can't do the following line, because we don't have access to the table in Amazon RDS:
--      IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs j where j.name=@jobname)
BEGIN TRY
	EXEC msdb.dbo.sp_delete_job @job_name=@jobname, @delete_unused_schedule=1
END TRY
BEGIN CATCH
END CATCH

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
SET @cmd = N'[AdminStore].[DeleteLogs]'
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

-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.2.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'8.2.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Migration 8.3.0.0
-- -----------------------------------------------------------------------------------------------
IF NOT ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.3.0') <> 0) 
	set noexec on
Print 'Migrating 8.3.0.0 ...'
-- -----------------------------------------------------------------------------------------------


-- -----------------------------------------------------------------------------------------------
-- Always add your code just above this comment block
-- -----------------------------------------------------------------------------------------------
IF ([AdminStore].[IsSchemaVersionLessOrEqual](N'8.3.0') <> 0)
	EXEC [AdminStore].[SetSchemaVersion] @value = N'8.3.0';
GO
set noexec off
-- -----------------------------------------------------------------------------------------------



/******************************************************************************************************************************
Name:			GetStatus

Description:	Returns the version of the database.

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetStatus]
GO

CREATE PROCEDURE [AdminStore].[GetStatus] 
AS
BEGIN
	SELECT [SchemaVersion] FROM [AdminStore].[DbVersionInfo] WHERE [Id] = 1;
END
GO 


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[BeginSession]
GO

CREATE PROCEDURE [AdminStore].[BeginSession] 
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
		SELECT @OldSessionId = SessionId FROM [AdminStore].[Sessions] WHERE UserId = @UserId;
		DECLARE @NewSessionId uniqueidentifier = NEWID();
		IF @OldSessionId IS NULL
		BEGIN
			INSERT [AdminStore].[Sessions] (UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso)
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			VALUES (@UserId, @NewSessionId, @BeginTime, @EndTime, @UserName, @LicenseLevel, @IsSso);
		END
		ELSE
		BEGIN
			UPDATE [AdminStore].[Sessions]
			SET [SessionId] = @NewSessionId, [BeginTime] = @BeginTime, [EndTime] = @EndTime, [UserName] = @UserName, [LicenseLevel] = @LicenseLevel, [IsSso] = @IsSso 
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			WHERE [SessionId] = @OldSessionId;
		END

		-- [LicenseActivities]
		INSERT INTO [AdminStore].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
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
		INSERT INTO @ActiveLicenses EXEC [AdminStore].[GetActiveLicenses] @BeginTime, @LicenseLockTimeMinutes
		INSERT INTO [AdminStore].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[ExtendSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[ExtendSession]
GO

CREATE PROCEDURE [AdminStore].[ExtendSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime
)
AS
BEGIN
	DECLARE @utcNow DATETIME = GETUTCDATE();

	UPDATE [AdminStore].[Sessions] SET EndTime = @EndTime
	OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
	WHERE SessionId = @SessionId AND BeginTime IS NOT NULL AND @utcNow < EndTime;
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[EndSession]
GO

CREATE PROCEDURE [AdminStore].[EndSession] 
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
		UPDATE [AdminStore].[Sessions] SET [BeginTime] = NULL, [EndTime] = @EndTime
		OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
		WHERE [SessionId] = @SessionId AND [BeginTime] IS NOT NULL
		AND (@TimeoutTime IS NULL OR @TimeoutTime = [EndTime]);

		IF @@ROWCOUNT > 0 BEGIN
			-- [LicenseActivities]
			DECLARE @UserId int
			DECLARE @LicenseLevel int
			SELECT @UserId = [UserId], @LicenseLevel = [LicenseLevel] FROM [AdminStore].[Sessions] WHERE [SessionId] = @SessionId;
			INSERT INTO [AdminStore].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
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
			INSERT INTO @ActiveLicenses EXEC [AdminStore].[GetActiveLicenses] @EndTime, @LicenseLockTimeMinutes
			INSERT INTO [AdminStore].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetApplicationLabels]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetApplicationLabels]
GO

CREATE PROCEDURE [AdminStore].[GetApplicationLabels] 
	@Locale nvarchar(32)
AS
BEGIN
	SELECT [Key], [Text] FROM [AdminStore].ApplicationLabels WHERE Locale = @Locale;
END
GO
/******************************************************************************************************************************
Name:			GetConfigSettings

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetConfigSettings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetConfigSettings]
GO

CREATE PROCEDURE [AdminStore].[GetConfigSettings] 
	@AllowRestricted bit
AS
BEGIN
	SELECT [Key], [Value], [Group], IsRestricted FROM [AdminStore].ConfigSettings WHERE IsRestricted = @AllowRestricted OR @AllowRestricted = 1;
END
GO
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetSession]
GO

CREATE PROCEDURE [AdminStore].[GetSession] 
(
	@SessionId uniqueidentifier
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where SessionId = @SessionId;
END
GO 
/******************************************************************************************************************************
Name:			GetSession

Description: 
			
Change History:
Date			Name					Change
2015/11/17		Anton Trinkunas			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetUserSession]
GO

CREATE PROCEDURE [AdminStore].[GetUserSession] 
(
	@UserId int
)
AS
BEGIN
	SELECT UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso from [AdminStore].[Sessions] where UserId = @UserId;
END
GO 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SelectSessions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[SelectSessions]
GO

CREATE PROCEDURE [AdminStore].[SelectSessions] 
(
	@ps int,
	@pn int
)
AS
BEGIN
	WITH SessionsRN AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY BeginTime DESC) AS RN, UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso 
		FROM [AdminStore].[Sessions] WHERE BeginTime IS NOT NULL
	)
	SELECT * FROM SessionsRN
	WHERE RN BETWEEN(@pn - 1)*@ps + 1 AND @pn * @ps
	ORDER BY BeginTime DESC;
END
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetActiveLicenses]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetActiveLicenses]
GO

CREATE PROCEDURE [AdminStore].[GetActiveLicenses] 
(
	@Now datetime,
	@LicenseLockTimeMinutes int
)
AS
BEGIN
	DECLARE @EndTime datetime = DATEADD(MINUTE, -@LicenseLockTimeMinutes, @Now)
	SELECT [LicenseLevel], COUNT(*) as [Count]
	FROM [AdminStore].[Sessions] 
	WHERE [EndTime] IS NULL OR [EndTime] > @EndTime
	GROUP BY [LicenseLevel]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLicenseTransactions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLicenseTransactions]
GO

CREATE PROCEDURE [AdminStore].[GetLicenseTransactions] 
(
	@StartTime datetime,
	@ConsumerType int
)
AS
BEGIN
	SELECT [UserId], [UserLicenseType] AS [LicenseType], [TransactionType], [ActionType], [TimeStamp] AS [Date],
		ISNULL(STUFF((SELECT ';' + CAST([LicenseType] AS VARCHAR(10)) + ':' + CAST([Count] AS VARCHAR(10))
		FROM [AdminStore].[LicenseActivityDetails] D
		WHERE D.[LicenseActivityId] = A.[LicenseActivityId]
		FOR XML PATH('')), 1, 1, ''), '') AS [Details]
	FROM [AdminStore].[LicenseActivities] A
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[WriteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[WriteLogs]
GO

CREATE PROCEDURE [AdminStore].[WriteLogs]  
(
  @InsertLogs LogsType READONLY
)
AS
BEGIN
  INSERT INTO [AdminStore].[Logs] (
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[DeleteLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[DeleteLogs]
GO

CREATE PROCEDURE [AdminStore].[DeleteLogs] 
AS
BEGIN
  -- Get the number of days to keep from config settings - DEFAULT 7
  DECLARE @days int
  SELECT @days=c.[Value] FROM [AdminStore].ConfigSettings c WHERE c.[Key] = N'DaysToKeepInLogs'
  SELECT @days=COALESCE(@days, 7) 

  -- Delete old log records
  DELETE FROM [AdminStore].[Logs] WHERE [Timestamp] <= DATEADD(DAY, @days*-1, SYSDATETIMEOFFSET()) 
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLogs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLogs]
GO


CREATE PROCEDURE [AdminStore].[GetLogs]  
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

	SELECT @total = COUNT(*) FROM [AdminStore].[Logs] where @id = 0 OR ID <= @id 	

	SET @fetch = IIF(@recordlimit < 0, @total, @recordlimit)

	SELECT TOP (@fetch) * FROM [AdminStore].[Logs] WHERE @id = 0 OR ID <= @id ORDER BY Id DESC

END



GO



/******************************************************************************************************************************
Name:			GetLicenseUsage

Description:	Returns license usage information 

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLicenseUserActivity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLicenseUserActivity]
GO

CREATE PROCEDURE [AdminStore].[GetLicenseUserActivity]
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
	[AdminStore].[LicenseActivities] la  WITH (NOLOCK) 
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetLicenseUsage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetLicenseUsage]
GO

CREATE PROCEDURE [AdminStore].[GetLicenseUsage]
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
		[AdminStore].LicenseActivities AS la WITH (NOLOCK) LEFT JOIN [AdminStore].LicenseActivityDetails AS da WITH (NOLOCK) 
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



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserPasswordRecoveryRequestCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].GetUserPasswordRecoveryRequestCount
GO

CREATE PROCEDURE [AdminStore].GetUserPasswordRecoveryRequestCount
(
    @login as nvarchar(max)
)
AS
BEGIN
    SELECT COUNT([Login])
    FROM [AdminStore].[PasswordRecoveryTokens]
    WHERE [Login] = @login
    AND [CreationTime] > DATEADD(d,-1,CURRENT_TIMESTAMP)
END
GO 



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[SetUserPasswordRecoveryToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].SetUserPasswordRecoveryToken
GO

CREATE PROCEDURE [AdminStore].SetUserPasswordRecoveryToken 
(
    @login as nvarchar(max),
    @recoverytoken as uniqueidentifier
)
AS
BEGIN
    INSERT INTO [AdminStore].[PasswordRecoveryTokens]
    ([Login],[CreationTime],[RecoveryToken])
    VALUES (@login, CURRENT_TIMESTAMP, @recoverytoken)
END
GO 


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[GetUserPasswordRecoveryTokens]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[GetUserPasswordRecoveryTokens]
GO

CREATE PROCEDURE [AdminStore].[GetUserPasswordRecoveryTokens]
(
    @token as nvarchar(max)
)
AS
BEGIN
	SELECT b.[Login],b.[CreationTime],b.[RecoveryToken] FROM [AdminStore].[PasswordRecoveryTokens] a
	INNER JOIN [AdminStore].[PasswordRecoveryTokens] b
	ON a.[Login] = b.[Login]
	WHERE a.[RecoveryToken] = @token
	ORDER BY [CreationTime] DESC
END
GO 


