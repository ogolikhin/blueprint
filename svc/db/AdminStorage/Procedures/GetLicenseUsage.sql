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
DECLARE @startMonth date = CAST(DATEFROMPARTS(@year, @month, 1) as Datetime); --DATEADD(month, @month, DATEADD(year, (@year-1900), 0));
DECLARE @currentMonth date = CAST(DATEFROMPARTS(YEAR(@currentDate), MONTH(@currentDate), 1) as Datetime);--,DATEADD(month, MONTH(@currentDate)-1, DATEADD(year, (YEAR(@currentDate)-1900), 0));

-- To Hold registered user (first time login)
DECLARE @registration TABLE  (
	[YearMonth] int NULL,
	[UserId] int NULL,
	[License] int NULL
)
INSERT INTO @registration
SELECT 
	YEAR(R.FirstTime) * 100 + MONTH(R.FirstTime) as YearMonth, 
	R.UserId, 
	R.License  
FROM (		
	SELECT 
		UserId,
		UserLicenseType AS License,
		MIN(la.TimeStamp) AS FirstTime
	FROM LicenseActivities AS la WITH (NOLOCK) 
	WHERE la.ConsumerType = 1 AND la.ActionType = 1 -- Client(ConsumerType) and Login(ActionType)
	GROUP BY la.UserId, la.UserLicenseType
) AS R

-- To hold user activities
DECLARE @usage TABLE  (
	[YearMonth] int NULL,
	[UserId] int NULL,
	[Consumer] int NULL,
	[License] int NULL,
	[CountLicense] int NULL,
	[Count] int NULL
)
INSERT INTO @usage
SELECT 
	YEAR(la.TimeStamp) * 100 + MONTH(la.TimeStamp) as YearMonth,
	la.UserId,
	la.ConsumerType AS Consumer,
	la.UserLicenseType AS License,
	da.LicenseType AS CountLicense,
	da.[Count] AS [Count]
FROM LicenseActivities AS la WITH (NOLOCK) 
	LEFT JOIN LicenseActivityDetails AS da WITH (NOLOCK) 
	ON la.LicenseActivityId = da.LicenseActivityId


--Create final result
-- Consumer: Client-1, Analytics-2, REST-3
-- LicenseType: Viewer-1, Collaborator- 2, Author- 3
SELECT 
	 u.YearMonth / 100 AS UsageYear
	,u.YearMonth % 100 AS UsageMonth
	
	,COUNT(DISTINCT CASE WHEN u.Consumer = 1 AND u.License = 3 THEN u.UserId ELSE NULL END) AS UniqueAuthors
	,STUFF((
		SELECT DISTINCT ',' + CAST(a.UserId AS VARCHAR(10)) 
			FROM @usage as a
			WHERE u.YearMonth = a.YearMonth AND a.Consumer = 1  AND a.License = 3 
			FOR XML PATH('')), 1, 1,'') as UniqueAuthorUserIds	
				
	,COUNT(DISTINCT CASE WHEN u.Consumer = 1 AND u.License = 2 THEN u.UserId ELSE NULL END) AS UniqueCollaborators
	,STUFF((
		SELECT DISTINCT ',' + CAST(a.UserId AS VARCHAR(10)) 
		FROM @usage as a 
		WHERE u.YearMonth = a.YearMonth AND a.Consumer = 1  AND a.License = 2
		FOR XML PATH('')), 1, 1, '') as UniqueCollaboratorUserIds

	, ISNULL((
		SELECT Count(r.UserId) as [Count] 
			FROM @registration r 
			WHERE r.YearMonth = u.YearMonth AND r.License = 3
		), 0) as RegisteredAuthorsCreated
	,STUFF((
		SELECT ',' + CAST(r.UserId AS VARCHAR(10)) 
			FROM @registration as r 
			WHERE r.YearMonth = u.YearMonth AND r.License = 3 
		FOR XML PATH('')), 1, 1,'') as RegisteredAuthorsCreatedUserIds

	, ISNULL((
		SELECT Count(r.UserId) as [Count] 
			FROM @registration r 
			WHERE r.YearMonth = u.YearMonth AND r.License = 2
		), 0) as RegisteredCollaboratorsCreated
	,STUFF((
		SELECT ',' + CAST(r.UserId AS VARCHAR(10)) 
			FROM @registration as r
			WHERE r.YearMonth = u.YearMonth AND r.License = 2 
		FOR XML PATH('')), 1, 1,'') as RegisteredCollaboratorCreatedUserIds

	, ISNULL((
		SELECT Count(r.UserId) as [Count] 
			FROM @registration r 
			WHERE r.YearMonth <= u.YearMonth AND r.License = 3
		), 0) as AuthorsCreatedToDate

	, ISNULL((
		SELECT Count(r.UserId) as [Count] 
			FROM @registration r 
			WHERE r.YearMonth <= u.YearMonth AND r.License = 2
		), 0) as CollaboratorsCreatedToDate
	

	,ISNULL(MAX(CASE WHEN u.CountLicense = 3 THEN u.[Count] ELSE NULL END), 0) AS MaxConcurrentAuthors
	,ISNULL(MAX(CASE WHEN u.CountLicense = 2 THEN u.[Count] ELSE NULL END), 0) AS MaxConcurrentCollaborators
	,ISNULL(MAX(CASE WHEN u.CountLicense = 1 THEN u.[Count] ELSE NULL END), 0) AS MaxConcurrentViewers
	,COUNT(CASE WHEN u.Consumer = 2 THEN 1 ELSE NULL END) AS UsersFromAnalytics
	,COUNT(CASE WHEN u.Consumer= 3 THEN 1 ELSE NULL END) AS UsersFromRestApi

			
FROM @usage AS u
GROUP BY u.YearMonth
HAVING (@year IS NULL OR @month IS NULL OR u.YearMonth >= Year(@startMonth) * 100 +  Month(@startMonth))
	   AND u.YearMonth < Year(@currentMonth) * 100 +  Month(@currentMonth)


END
GO 

