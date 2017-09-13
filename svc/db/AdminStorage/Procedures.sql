
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
	UPDATE [AdminStore].[Sessions] SET EndTime = @EndTime
	OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
	WHERE SessionId = @SessionId AND BeginTime IS NOT NULL AND GETUTCDATE() < EndTime;
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

