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


