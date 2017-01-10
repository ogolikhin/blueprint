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
	DECLARE @licensetype int;

	WITH L
	AS (
		
		SELECT	 YEAR(la.[TimeStamp]) AS ActivityYear
				,MONTH(la.[TimeStamp]) AS ActivityMonth
				,la.UserId as UserId
				,la.ConsumerType AS Consumer
				,la.UserLicenseType AS License
				,da.LicenseType AS CountLicense
				,da.[Count] AS [Count]
		FROM LicenseActivities AS la WITH (NOLOCK) 
			LEFT JOIN LicenseActivityDetails AS da WITH (NOLOCK) 
		ON la.LicenseActivityId = da.LicenseActivityId
		WHERE (@year IS NULL OR @month IS NULL OR la.[TimeStamp] > @startMonth)
				AND la.[TimeStamp] < @currentMonth
	)
	-- Consumer: Client-1, Analytics-2, REST-3
	-- LicenseType: Viewer-1, Collaborator- 2, Author- 3
	SELECT 
		 L.ActivityYear 
		,L.ActivityMonth
		,COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 3 THEN L.UserId ELSE NULL END) AS UniqueAuthors
		,COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 2 THEN L.UserId ELSE NULL END) AS UniqueCollaborators
		,COUNT(DISTINCT CASE WHEN L.Consumer = 1 AND L.License = 1 THEN L.UserId ELSE NULL END) AS UniqueViewers
		,ISNULL(MAX(CASE WHEN L.CountLicense = 3 THEN L.[Count] ELSE NULL END), 0) AS MaxConCurrentAuthors
		,ISNULL(MAX(CASE WHEN L.CountLicense = 2 THEN L.[Count] ELSE NULL END), 0) AS MaxConCurrentCollaborators
		,ISNULL(MAX(CASE WHEN L.CountLicense = 1 THEN L.[Count] ELSE NULL END), 0) AS MaxConCurrentViewers
		,COUNT(CASE WHEN L.Consumer = 2 THEN 1 ELSE NULL END) AS UsersFromAnalytics
		,COUNT(CASE WHEN L.Consumer = 3 THEN 1 ELSE NULL END) AS UsersFromRestApi
			
	FROM L
	GROUP BY L.ActivityYear, L.ActivityMonth;

END

GO 

