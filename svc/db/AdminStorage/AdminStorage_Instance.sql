
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
Name:			Traces

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Traces]') AND type in (N'U'))
DROP TABLE [dbo].[Traces]
GO

CREATE TABLE [dbo].[Traces](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[MethodName] [nvarchar](100),
	[FilePath] [nvarchar](1000),
	[LineNumber] [int],
	[StackTrace] [nvarchar](4000),
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
	[FormattedMessage] [nvarchar](4000) NULL,
	[Payload] [xml] NULL,
	[ActivityId] [uniqueidentifier],
	[RelatedActivityId] [uniqueidentifier],
	[ProcessId] [int],
	[ThreadId] [int],
	 CONSTRAINT [PK_Traces] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)) ON [PRIMARY]
GO

/******************************************************************************************************************************
Name:			TracesType

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TracesType' AND ss.name = N'dbo')
DROP TYPE [dbo].[TracesType]
GO

CREATE TYPE TracesType AS TABLE
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
	[ActivityId] [uniqueidentifier], 
	[RelatedActivityId] [uniqueidentifier],
	[ProcessId] [int],
	[ThreadId] [int],
	[IpAddress] [nvarchar](45),
	[Source] [nvarchar](100),
	[MethodName] [nvarchar](100),
	[FilePath] [nvarchar](1000),
	[LineNumber] [int],
	[StackTrace] [nvarchar](4000)
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
2015/12/22		Glen Stone				Added inserts to LicenseActivities and LicenseActivityDetails
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
	@OldSessionId uniqueidentifier OUTPUT,
	@licenseLockTimeMinutes int
)
AS
BEGIN
	BEGIN TRY
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION;

		-- [Sessions]
		SELECT @OldSessionId = SessionId FROM [dbo].[Sessions] WHERE UserId = @UserId;
		SELECT @NewSessionId = NEWID();
		IF @OldSessionId IS NULL
		BEGIN
			INSERT [dbo].[Sessions] (UserId, SessionId, BeginTime, UserName, LicenseLevel, IsSso)
			VALUES (@UserId, @NewSessionId, @BeginTime, @UserName, @LicenseLevel, @IsSso);
		END
		ELSE
		BEGIN
			UPDATE [dbo].[Sessions] 
			SET [SessionId] = @NewSessionId, [BeginTime] = @BeginTime, [EndTime] = NULL, 
				[UserName] = @UserName, [LicenseLevel] = @LicenseLevel, [IsSso] = @IsSso 
			WHERE [UserId] = @UserId;
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
		INSERT INTO @ActiveLicenses EXEC [dbo].[GetActiveLicenses] @BeginTime, @licenseLockTimeMinutes
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

/******************************************************************************************************************************
Name:			EndSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
2015/12/22		Glen Stone				Added inserts to LicenseActivities and LicenseActivityDetails
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndSession]
GO

CREATE PROCEDURE [dbo].[EndSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime,
	@Timeout bit,
	@licenseLockTimeMinutes int
)
AS
BEGIN
	BEGIN TRY
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION;

		-- [Sessions]
		UPDATE [dbo].[Sessions] SET BeginTime = NULL, EndTime = @EndTime WHERE SessionId = @SessionId;

		-- [LicenseActivities]
		DECLARE @UserId int
		DECLARE @LicenseLevel int
		SELECT @UserId = [UserId], @LicenseLevel = [LicenseLevel] FROM [dbo].[Sessions] WHERE [SessionId] = @SessionId;
		INSERT INTO [dbo].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
		VALUES
			(@UserId
			,@LicenseLevel
			,2 -- LicenseTransactionType.Release
			,2 + @timeout -- LicenseActionType.LogOut or LicenseActionType.Timeout
			,1 -- LicenseConsumerType.Client
			,@EndTime)

		-- [LicenseActivityDetails]
		DECLARE @LicenseActivityId int = SCOPE_IDENTITY()
		DECLARE @ActiveLicenses table ( LicenseLevel int, Count int )
		INSERT INTO @ActiveLicenses EXEC [dbo].[GetActiveLicenses] @EndTime, @licenseLockTimeMinutes
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
	SELECT [UserId], [UserLicenseType], [TransactionType], [ActionType], [TimeStamp],
		ISNULL(STUFF((SELECT CONCAT(';', [LicenseType], ':', [Count])
		FROM [dbo].[LicenseActivityDetails] D
		WHERE D.[LicenseActivityId] = A.[LicenseActivityId]
		FOR XML PATH('')), 1, 1, ''), '') AS [Details]
	FROM [dbo].[LicenseActivities] A
	WHERE [TimeStamp] >= @StartTime
	AND [ConsumerType] = @ConsumerType
END
GO

/******************************************************************************************************************************
Name:			WriteTraces

Description: 
			
Change History:
Date			Name					Change
2015/12/17		Chris Dufour			Initial Version
******************************************************************************************************************************/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WriteTraces]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[WriteTraces]
GO

CREATE PROCEDURE [dbo].[WriteTraces]  
(
  @InsertTraces TracesType READONLY
)
AS
BEGIN
  INSERT INTO [Traces] (
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
		[ActivityId],
		[RelatedActivityId],
		[ProcessId],
		[ThreadId],
		[IpAddress],
		[Source],
		[MethodName],
		[FilePath],
		[LineNumber],
		[StackTrace]
	)
  SELECT * FROM @InsertTraces;
END

GO


-- --------------------------------------------------
-- Always add your code just above this comment block
-- --------------------------------------------------
EXEC [dbo].[SetSchemaVersion] @value = N'7.0.0';
GO
-- --------------------------------------------------


