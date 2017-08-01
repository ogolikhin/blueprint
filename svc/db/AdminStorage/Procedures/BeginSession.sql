IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AdminStore].[BeginSession]') AND type in (N'P', N'PC'))
DROP PROCEDURE [AdminStore].[BeginSession]
GO

CREATE PROCEDURE [AdminStore].[BeginSession] 
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
		SELECT @OldSessionId = SessionId FROM [AdminStore].[Sessions] WHERE UserId = @UserId;
		DECLARE @NewSessionId uniqueidentifier = NEWID();
		IF @OldSessionId IS NULL
		BEGIN
			INSERT [AdminStore].[Sessions] (UserId, SessionId, BeginTime, EndTime, UserName, LicenseLevel, IsSso)
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			VALUES (@UserId, @NewSessionId, @BeginTime, @EndTime, @UserName, @LicenseLevel, @IsSso);
		END
		ELSE
		BEGIN
			UPDATE [AdminStore].[Sessions]
			SET [SessionId] = @NewSessionId, [BeginTime] = @BeginTime, [EndTime] = @EndTime, [UserName] = @UserName, [LicenseLevel] = @LicenseLevel, [IsSso] = @IsSso 
			OUTPUT Inserted.[UserId], Inserted.[SessionId], Inserted.[BeginTime], Inserted.[EndTime], Inserted.[UserName], Inserted.[LicenseLevel], Inserted.[IsSso]
			WHERE [SessionId] = @OldSessionId;
		END

		-- [LicenseActivities]
		INSERT INTO [AdminStore].[LicenseActivities] ([UserId], [UserLicenseType], [TransactionType], [ActionType], [ConsumerType], [TimeStamp])
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
		INSERT INTO @ActiveLicenses EXEC [AdminStore].[GetActiveLicenses] @BeginTime, @LicenseLockTimeMinutes
		INSERT INTO [AdminStore].[LicenseActivityDetails] ([LicenseActivityId], [LicenseType], [Count])
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
