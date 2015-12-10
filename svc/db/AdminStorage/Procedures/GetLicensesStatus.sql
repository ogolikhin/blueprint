IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLicensesStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLicensesStatus]
GO

CREATE PROCEDURE [dbo].[GetLicensesStatus] 
(
	@TimeUtc datetime,
	@TimeDiff int
)
AS
BEGIN
	SELECT LicenseLevel, COUNT(*) as [Count]
	FROM [dbo].[Sessions] 
	WHERE (EndTime IS NULL OR EndTime > DATEADD(MINUTE, @TimeDiff, @TimeUtc))
	GROUP BY LicenseLevel
END
GO 