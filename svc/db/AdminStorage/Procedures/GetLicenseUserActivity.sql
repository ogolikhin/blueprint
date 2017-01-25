/******************************************************************************************************************************
Name:			GetLicenseUsage

Description:	Returns license usage information 

******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicenseUserActivity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicenseUserActivity]
GO

CREATE PROCEDURE [dbo].[GetLicenseUserActivity]

AS
BEGIN


SELECT 
	UserId, 
	MAX(UserLicenseType) as LicenseType,  
	YEAR([TimeStamp])* 100 + MONTH([TimeStamp]) AS [YearMonth]
FROM [dbo].[LicenseActivities]  WITH (NOLOCK) 
WHERE ConsumerType = 1 AND ActionType = 1 --and UserLicenseType in (3,2) 
GROUP BY UserId, YEAR([TimeStamp])* 100 + MONTH([TimeStamp])
ORDER BY UserId, [YearMonth]

END
GO 

