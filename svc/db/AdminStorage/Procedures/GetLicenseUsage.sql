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

