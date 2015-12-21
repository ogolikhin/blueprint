/******************************************************************************************************************************
Name:			EndSession

Description: 
			
Change History:
Date			Name					Change
2015/11/03		Chris Dufour			Initial Version
******************************************************************************************************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndSession]
GO

CREATE PROCEDURE [dbo].[EndSession] 
(
	@SessionId uniqueidentifier,
	@EndTime datetime,
	@Timeout bit,
	@licenseLockTimeMinutes int
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
	BEGIN TRANSACTION;
	UPDATE [dbo].[Sessions] SET BeginTime = NULL, EndTime = @EndTime WHERE SessionId = @SessionId;

	DECLARE @UserId int
	DECLARE @LicenseLevel int
	SELECT @UserId = UserId, @LicenseLevel = LicenseLevel FROM [dbo].[Sessions] WHERE SessionId = @SessionId;
	INSERT INTO [dbo].[LicenseActivities]
		([UserId]
		,[UserLicenseType]
		,[TransactionType]
		,[ActionType]
		,[ConsumerType]
		,[TimeStamp])
	VALUES
		(@UserId
		,@LicenseLevel
		,2 -- LicenseTransactionType.Release
		,2 + @timeout -- LicenseActionType.LogOut or LicenseActionType.Timeout
		,1 -- LicenseConsumerType.Client
		,@EndTime)

	IF SCOPE_IDENTITY() > 0
	BEGIN
		INSERT INTO [dbo].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
		SELECT SCOPE_IDENTITY(), LicenseLevel, COUNT(*) as [Count]
		FROM [dbo].[Sessions]
		WHERE EndTime IS NULL OR EndTime > DATEADD(MINUTE, -@licenseLockTimeMinutes, @EndTime)
		GROUP BY LicenseLevel
	END
	COMMIT TRANSACTION;
END
GO
