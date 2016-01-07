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
