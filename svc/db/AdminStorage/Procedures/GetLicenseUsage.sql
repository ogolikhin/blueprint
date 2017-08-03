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

