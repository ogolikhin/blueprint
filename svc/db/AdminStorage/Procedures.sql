
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
DECLARE @currentDate datetime = GETUTCDATE();
DECLARE @startMonth date = DATEADD(month, @month, DATEADD(year, (@year-1900), 0));
DECLARE @currentMonth date = DATEADD(month, MONTH(@currentDate)-1, DATEADD(year, (YEAR(@currentDate)-1900), 0));


-- To Hold registered user (first time login)
DECLARE @registration TABLE  (
	[YearMonth] int NULL,
	[UserId] int NULL,
	[License] int NULL
)
INSERT INTO @registration
SELECT YEAR(R.FirstTime) * 100 + MONTH(R.FirstTime) as [YearMonth], R.UserId, R.License  
FROM (		
	SELECT 
		UserId,
		UserLicenseType AS License,
		MIN(la.[TimeStamp]) AS FirstTime
	FROM LicenseActivities AS la WITH (NOLOCK) 
	WHERE la.ConsumerType = 1 AND la.ActionType = 1 -- Client(ConsumerType) and Login(ActionType)
	GROUP BY la.[UserId], la.UserLicenseType
) AS R

-- To hold user activities
DECLARE @useractivity TABLE  (
	[YearMonth] int NULL,
	[UserId] int NULL,
	[Consumer] int NULL,
	[License] int NULL,
	[CountLicense] int NULL,
	[Count] int NULL
)
INSERT INTO @useractivity
SELECT YEAR(la.[TimeStamp]) * 100 + MONTH(la.[TimeStamp]) as [YearMonth],
       la.[UserId],
       la.[ConsumerType] AS [Consumer],
       la.[UserLicenseType] AS [License],
       da.[LicenseType] AS [CountLicense],
       da.[Count] AS [Count]
FROM LicenseActivities AS la WITH (NOLOCK) 
	 LEFT JOIN LicenseActivityDetails AS da WITH (NOLOCK) 
ON la.[LicenseActivityId] = da.[LicenseActivityId]


-- Create cumulative table 
DECLARE @cumulative TABLE (
	[YearMonth] int NULL,
	[Authors] int NULL,
	[Collaborators] int NULL,
	[CumulativeAuthors] int NULL,
	[CumulativeCollaborators] int NULL
);

-- Populate cumulative with all dates from the range
DECLARE @startDate int;
SELECT @startDate = MIN(YearMonth) from @useractivity;
WITH d AS (
	SELECT DATEADD(Month,DATEDIFF(Month,0, DateAdd(day, 0, DateAdd(month, @startDate%100-1, DateAdd(Year, @startDate/100-1900, 0)))),0) AS [Date]
	UNION ALL
	SELECT DATEADD(Month,1,[Date])
		FROM d
	WHERE DATEADD(Month,1,[Date]) <=  @currentMonth
)
INSERT INTO @cumulative
SELECT Year([Date])*100 + Month([Date]), 0, 0, 0, 0
FROM d
OPTION (MAXRECURSION 0);

--update counters per month and per license type
UPDATE c SET 
	c.Authors = ISNULL((
		SELECT Count(UserId) as [Count] from @registration r 
			WHERE r.YearMonth = c.YearMonth AND r.License = 3
		GROUP BY r.YearMonth, r.License), 0),
	c.Collaborators = ISNULL((
		SELECT Count(UserId) as [Count] from @registration r 
			WHERE r.YearMonth = c.YearMonth AND r.License = 2
		GROUP BY r.YearMonth, r.License), 0)
FROM @cumulative c;

--Update Cumulative values
WITH L
AS (
	SELECT YearMonth,
	    SUM([Authors]) OVER (ORDER BY [YearMonth]) as [CA],
		SUM([Collaborators]) OVER (ORDER BY [YearMonth]) as [CC]
	FROM  @cumulative
)
UPDATE c SET 
	[CumulativeAuthors] = L.CA,
	[CumulativeCollaborators] = L.CC
FROM @cumulative c INNER JOIN L 
	ON c.YearMonth = l.YearMonth;

--Create final result
-- Consumer: Client-1, Analytics-2, REST-3
-- LicenseType: Viewer-1, Collaborator- 2, Author- 3
SELECT 
	ua.[YearMonth] / 100 AS [UsageYear]
	,ua.[YearMonth] % 100 AS [UsageMonth]
	
	
	,COUNT(DISTINCT CASE WHEN ua.[Consumer] = 1 AND ua.[License] = 3 THEN ua.[UserId] ELSE NULL END) AS UniqueAuthors
	,COUNT(DISTINCT CASE WHEN ua.[Consumer] = 1 AND ua.[License] = 2 THEN ua.[UserId] ELSE NULL END) AS UniqueCollaborators
	,STUFF((
		SELECT DISTINCT ',' + CAST(a.UserId AS VARCHAR(10)) 
		FROM @useractivity as a
		WHERE ua.[YearMonth] = a.[YearMonth]
			AND a.[Consumer] = 1 
			AND a.[License] = 3 
		FOR XML PATH('')), 1, 1,'') as [UniqueAuthorUserIds]		
	,STUFF((
		SELECT DISTINCT ',' + CAST(a.[UserId] AS VARCHAR(10)) 
		FROM @useractivity as a 
		WHERE ua.[YearMonth] = a.[YearMonth]
			AND a.[Consumer] = 1 
			AND a.[License] = 2
		FOR XML PATH('')), 1, 1, '') as [UniqueCollaboratorUserIds]

	,(SELECT r.Authors FROM @cumulative as r WHERE r.[YearMonth] = ua.[YearMonth])  as RegisteredAuthorsCreated
	,STUFF((
		SELECT ',' + CAST(R.[UserId] AS VARCHAR(10)) 
		FROM @registration as r 
		WHERE r.[YearMonth] = ua.[YearMonth]
			AND r.[License] = 3 
		FOR XML PATH('')), 1, 1,'') as RegisteredAuthorsCreatedUserIds

	,(SELECT r.Collaborators FROM @cumulative as r WHERE r.[YearMonth] = ua.[YearMonth])  as RegisteredCollaboratorsCreated
	,STUFF((
		SELECT ',' + CAST(r.[UserId] AS VARCHAR(10)) 
		FROM @registration as r
		WHERE r.[YearMonth] = ua.[YearMonth]
			AND r.[License] = 2 
		FOR XML PATH('')), 1, 1,'') as RegisteredCollaboratorCreatedUserIds

	,ISNULL((SELECT c.[CumulativeAuthors]
		FROM @cumulative c
		WHERE c.YearMonth = ua.[YearMonth]), 0) as [AuthorsCreatedToDate]
	,ISNULL((SELECT c.[CumulativeCollaborators]
		FROM @cumulative c
		WHERE c.YearMonth = ua.[YearMonth]), 0) as [CollaboratorsCreatedToDate]
	

	,ISNULL(MAX(CASE WHEN ua.[CountLicense] = 3 THEN ua.[Count] ELSE NULL END), 0) AS MaxConCurrentAuthors
	,ISNULL(MAX(CASE WHEN ua.[CountLicense] = 2 THEN ua.[Count] ELSE NULL END), 0) AS MaxConCurrentCollaborators
	,ISNULL(MAX(CASE WHEN ua.[CountLicense] = 1 THEN ua.[Count] ELSE NULL END), 0) AS MaxConCurrentViewers
	,COUNT(CASE WHEN ua.[Consumer] = 2 THEN 1 ELSE NULL END) AS UsersFromAnalytics
	,COUNT(CASE WHEN ua.[Consumer]= 3 THEN 1 ELSE NULL END) AS UsersFromRestApi

			
FROM @useractivity AS ua
GROUP BY ua.[YearMonth]
HAVING (@year IS NULL OR @month IS NULL OR ua.[YearMonth] >= Year(@startMonth) * 100 +  Month(@startMonth))
	   AND ua.[YearMonth] < Year(@currentMonth) * 100 +  Month(@currentMonth)


END
GO 

